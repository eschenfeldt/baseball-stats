import { AfterViewInit, Component, Inject, OnInit, ViewChild } from "@angular/core";
import { BaseballDataSource } from "./baseball-data-source";
import { PagedApiParameters } from "./paged-api-parameters";
import { MatPaginator } from "@angular/material/paginator";
import { merge, tap } from "rxjs";
import { MatSort } from "@angular/material/sort";
import { BaseballApiFilter, BaseballFilterService } from "./baseball-filter.service";
import { v4 as uuidv4 } from "uuid";

@Component({ template: '' })
export abstract class BaseballTableComponent<ArgType extends PagedApiParameters, ReturnType> implements OnInit, AfterViewInit {

    protected abstract paginator: MatPaginator;
    protected abstract sort: MatSort;
    protected abstract dataSource: BaseballDataSource<ArgType, ReturnType>;

    protected uniqueIdentifier: string;
    protected abstract filterService: BaseballFilterService
    protected abstract readonly defaultFilters?: BaseballApiFilter

    defaultPageSize = BaseballDataSource.defaultPageSize;

    public constructor(
        @Inject(null) protected readonly componentName: string,
        @Inject(null) protected readonly sharePageState: boolean = false
    ) {
        if (this.sharePageState) {
            this.uniqueIdentifier = this.componentName;
        } else {
            this.uniqueIdentifier = `${this.componentName}${uuidv4()}`;
        }
    }

    public ngOnInit(): void {
        this.filterService.initFilters(this.uniqueIdentifier, this.defaultFilters);
        this.refresh();
    }

    public ngAfterViewInit(): void {
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;

        // register the pagination and sorting changes with the data source
        this.sort.sortChange.subscribe(() => this.paginator.pageIndex = 0);
        merge(this.paginator.page, this.sort.sortChange)
            .pipe(tap(() => this.refresh()))
            .subscribe();
    }

    public refresh(): void {
        this.dataSource.loadData();
    }
}
