import { AfterViewInit, Component, Inject, OnInit, Output, ViewChild } from "@angular/core";
import { BaseballDataSource } from "./baseball-data-source";
import { PagedApiParameters } from "./paged-api-parameters";
import { merge, tap } from "rxjs";
import { MatSort } from "@angular/material/sort";
import { BaseballApiFilter } from "./baseball-filter.service";

@Component({ template: '' })
export abstract class BaseballScrollTableComponent<ArgType extends PagedApiParameters, ReturnType> implements OnInit, AfterViewInit {

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
            this.dataSource.sort = this.sort;

            this.sort.sortChange.subscribe(() => {
                if (this.dataSource) {
                    this.dataSource.startIndex = 0; // reset start index on sort change
                    this.dataSource.loadData();
                }
            });
        }
    }

    public refresh(): void {
        if (this.dataSource) {
            this.dataSource.loadData();
        }
    }
}
