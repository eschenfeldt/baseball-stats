import { AfterViewInit, Component, OnInit, ViewChild } from "@angular/core";
import { BaseballDataSource } from "./baseball-data-source";
import { PagedApiParameters } from "./paged-api-parameters";
import { MatPaginator } from "@angular/material/paginator";
import { merge, tap } from "rxjs";
import { MatSort } from "@angular/material/sort";

@Component({ template: '' })
export abstract class BaseballTableComponent<ArgType extends PagedApiParameters, ReturnType> implements OnInit, AfterViewInit {

    abstract paginator: MatPaginator;
    abstract sort: MatSort;
    abstract dataSource: BaseballDataSource<ArgType, ReturnType>;

    defaultPageSize = BaseballDataSource.defaultPageSize;

    public ngOnInit(): void {
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
