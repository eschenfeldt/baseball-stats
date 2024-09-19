import { AfterViewInit, Component, Inject, OnInit, Output, ViewChild } from "@angular/core";
import { BaseballDataSource } from "./baseball-data-source";
import { PagedApiParameters } from "./paged-api-parameters";
import { MatPaginator } from "@angular/material/paginator";
import { merge, tap } from "rxjs";
import { MatSort } from "@angular/material/sort";
import { BaseballApiFilter } from "./baseball-filter.service";

@Component({ template: '' })
export abstract class BaseballTableComponent<ArgType extends PagedApiParameters, ReturnType> implements OnInit, AfterViewInit {

    protected abstract paginator: MatPaginator;
    protected abstract sort: MatSort;
    protected abstract dataSource?: BaseballDataSource<ArgType, ReturnType>;

    protected abstract readonly defaultFilters?: BaseballApiFilter

    defaultPageSize = BaseballDataSource.defaultPageSize;

    protected get uniqueIdentifier(): string {
        if (this.dataSource) {
            return this.dataSource.uniqueIdentifier;
        } else {
            return '';
        }
    }

    public ngOnInit(): void {
        this.refresh();
    }

    public ngAfterViewInit(): void {
        if (this.dataSource) {
            this.dataSource.paginator = this.paginator;
            this.dataSource.sort = this.sort;

            // register the pagination and sorting changes with the data source
            this.sort.sortChange.subscribe(() => this.paginator.pageIndex = 0);
            merge(this.paginator.page, this.sort.sortChange)
                .pipe(tap(() => this.refresh()))
                .subscribe();
        }
    }

    public refresh(): void {
        if (this.dataSource) {
            this.dataSource.loadData();
        }
    }
}
