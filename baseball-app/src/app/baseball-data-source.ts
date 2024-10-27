import { DataSource } from '@angular/cdk/collections';
import { BehaviorSubject, Observable, Subscription } from 'rxjs';
import { ApiMethod, BaseballApiService } from './baseball-api.service';
import { MatSort, Sort } from '@angular/material/sort';
import { MatPaginator } from '@angular/material/paginator';
import { PagedApiParameters } from './paged-api-parameters';
import { PagedResult } from './contracts/paged-result';
import { BaseballApiFilter, BaseballFilterService } from './baseball-filter.service';
import { v4 as uuidv4 } from 'uuid';
import { StatDefCollection } from './contracts/stat-def';
import { Leaderboard } from './contracts/leaderboard';

export abstract class BaseballDataSource<ArgType extends PagedApiParameters, ReturnType> extends DataSource<ReturnType> {

    public static readonly defaultPageSize = 20;

    protected updateOnFilterChanges: boolean = true;
    protected postProcess(data: PagedResult<ReturnType>): void {
        // optional post-processing when data is loaded
    }

    private dataSubject = new BehaviorSubject<ReturnType[]>([]);
    private statsSubject = new BehaviorSubject<StatDefCollection>({});
    private loadingSubject = new BehaviorSubject<boolean>(false);
    private totalCountSubject = new BehaviorSubject<number>(0);
    private activeSortSubject = new BehaviorSubject<Sort | null>(null);

    public uniqueIdentifier: string;
    public loading$ = this.loadingSubject.asObservable();
    public stats$ = this.statsSubject.asObservable();
    public totalCount$ = this.totalCountSubject.asObservable();
    public activeSort$ = this.activeSortSubject.asObservable();

    data: ReturnType[] | undefined;
    public paginator: MatPaginator | undefined;
    public sort: MatSort | undefined;

    private executingQuery?: Subscription;
    private filterChangesSubscription?: Subscription;

    public constructor(
        private endpoint: string,
        private method: ApiMethod,
        private api: BaseballApiService,
        private filterService: BaseballFilterService,
        sharePageState: boolean = true,
        defaultFilters?: BaseballApiFilter
    ) {
        super();
        if (sharePageState) {
            this.uniqueIdentifier = this.endpoint;
        } else {
            this.uniqueIdentifier = `${this.endpoint}${uuidv4()}`;
        }
        this.filterService.initFilters(this.uniqueIdentifier, defaultFilters);
        this.subscribeToFilterChanges();
    }

    override connect(): Observable<readonly ReturnType[]> {
        return this.dataSubject.asObservable();
    }

    override disconnect(): void {
        this.dataSubject.complete();
        this.loadingSubject.complete();
        this.totalCountSubject.complete();
        this.activeSortSubject.complete();
        if (this.executingQuery) {
            this.executingQuery.unsubscribe();
        }
        if (this.filterChangesSubscription) {
            this.filterChangesSubscription.unsubscribe();
        }
    }

    public loadData(): void {
        if (this.executingQuery) {
            this.executingQuery.unsubscribe();
        }

        let body = this.getParameters();
        this.setPaging(body);
        this.setSort(body);
        this.setFiltersFromFilterService(body);
        let queryBase;
        if (this.method == ApiMethod.GET) {
            queryBase = this.api.makeApiGet<PagedResult<ReturnType>>(this.endpoint, body);
        } else {
            queryBase = this.api.makeApiPost<PagedResult<ReturnType>>(this.endpoint, body);
        }
        this.executingQuery = queryBase.subscribe(result => {
            this.dataSubject.next(result.results);
            this.totalCountSubject.next(result.totalCount);
            const leaderboard = result as Leaderboard<ReturnType>;
            if (leaderboard) {
                this.statsSubject.next(leaderboard.stats);
            }
            this.postProcess(result);
        });
    }

    protected abstract getParameters(): ArgType;

    private setPaging(body: ArgType): void {
        if (this.paginator) {
            body.skip = this.paginator.pageIndex * this.paginator.pageSize;
            body.take = this.paginator.pageSize;
        } else {
            body.skip = 0;
            body.take = BaseballDataSource.defaultPageSize;
        }
    }

    private setSort(body: ArgType): void {
        if (this.sort && this.sort.active) {
            body.sort = this.sort.active;
            body.asc = this.sort.direction === 'asc';
        }
    }

    protected setFiltersFromFilterService(body: ArgType): void {
        this.filterService.updateParamsFromFilters(this.uniqueIdentifier, body);
    }

    private subscribeToFilterChanges(): void {
        this.filterChangesSubscription = this.filterService.filtersChanged$(this.uniqueIdentifier)
            .subscribe(() => this.loadData());
    }
}
