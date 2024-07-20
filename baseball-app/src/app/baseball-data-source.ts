import { DataSource } from '@angular/cdk/collections';
import { BehaviorSubject, Observable, Subscription } from 'rxjs';
import { ApiMethod, BaseballApiService } from './baseball-api.service';
import { MatSort, Sort } from '@angular/material/sort';
import { MatPaginator } from '@angular/material/paginator';
import { PagedApiParameters } from './paged-api-parameters';
import { PagedResult } from './paged-result';

export abstract class BaseballDataSource<ArgType extends PagedApiParameters, ReturnType> extends DataSource<ReturnType> {

    public static readonly defaultPageSize = 10;

    protected sharePageState: boolean = true;
    protected updateOnFilterChanges: boolean = true;

    private dataSubject = new BehaviorSubject<ReturnType[]>([]);
    private loadingSubject = new BehaviorSubject<boolean>(false);
    private totalCountSubject = new BehaviorSubject<number>(0);
    private activeSortSubject = new BehaviorSubject<Sort | null>(null);

    public uniqueIdentifier: string;
    public loading$ = this.loadingSubject.asObservable();
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
        private api: BaseballApiService
    ) {
        super();
        if (this.sharePageState) {
            this.uniqueIdentifier = this.endpoint;
        } else {
            this.uniqueIdentifier = this.endpoint + + '_' + (Math.random() * 100000).toString();
        }
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
        let queryBase;
        if (this.method == ApiMethod.GET) {
            const uri = this.endpoint + `?${Object.keys(body)
                .filter(key => (body as any)[key] != null)
                .map(key => `${key}=${encodeURIComponent((body as any)[key])}`).join('&')}`;
            queryBase = this.api.makeApiGet<PagedResult<ReturnType>>(uri);
        } else {
            queryBase = this.api.makeApiPost<PagedResult<ReturnType>>(this.endpoint, body);
        }
        this.executingQuery = queryBase.subscribe(result => {
            this.dataSubject.next(result.results);
            this.totalCountSubject.next(result.totalCount);
        });
    }

    protected abstract getParameters(): ArgType;

    private setPaging(body: ArgType): ArgType {
        if (this.paginator) {
            body.skip = this.paginator.pageIndex * this.paginator.pageSize;
            body.take = this.paginator.pageSize;
        } else {
            body.skip = 0;
            body.take = BaseballDataSource.defaultPageSize;
        }
        return body;
    }
}
