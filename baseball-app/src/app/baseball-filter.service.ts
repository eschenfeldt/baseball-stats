import { Injectable } from '@angular/core';
import { BehaviorSubject, debounceTime, Observable } from 'rxjs';
import { PagedApiParameters } from './paged-api-parameters';

export interface BaseballApiFilter {
    [propertyName: string]: any
}

@Injectable({
    providedIn: 'root'
})
export class BaseballFilterService {

    private filters: {
        [tableIdentifier: string]: BaseballApiFilter
    } = {};

    private filterSubjects: {
        [tableIdentifier: string]: BehaviorSubject<void>
    } = {};

    constructor() { }

    private getFilters<T extends PagedApiParameters>(uniqueIdentifier: string): T {
        return this.filters[uniqueIdentifier] as T;
    }

    public getFilterValue<T extends PagedApiParameters>(uniqueIdentifier: string, filterName: keyof T): any {
        const filters = this.getFilters<T>(uniqueIdentifier);
        return filters[filterName];
    }

    public setFilterValue<T extends PagedApiParameters>(uniqueIdentifier: string, filterName: keyof T, value: any): void {
        const filters = this.getFilters<T>(uniqueIdentifier);
        filters[filterName] = value;
        this.signalFilterChange(uniqueIdentifier);
    }

    public filtersChanged$(uniqueIdentifier: string): Observable<void> {
        return this.filterSubjects[uniqueIdentifier].asObservable()
            .pipe(
                debounceTime(150)
            );
    }

    public initFilters(uniqueIdentifier: string, defaultFilters?: BaseballApiFilter): void {
        if (this.filters[uniqueIdentifier] == null) {
            if (defaultFilters != null) {
                this.filters[uniqueIdentifier] = this.cloneFilter(defaultFilters);
            } else {
                this.filters[uniqueIdentifier] = {};
            }
        }
        if (this.filterSubjects[uniqueIdentifier] == null) {
            this.filterSubjects[uniqueIdentifier] = new BehaviorSubject<void>(undefined);
        }
    }

    public resetFilters(uniqueIdentifier: string, defaultFilters?: BaseballApiFilter): void {
        if (this.filters[uniqueIdentifier] == null) {
            this.initFilters(uniqueIdentifier, defaultFilters);
        } else {
            if (defaultFilters != null) {
                this.filters[uniqueIdentifier] = this.cloneFilter(defaultFilters);
            } else {
                this.filters[uniqueIdentifier] = {};
            }
        }
        this.signalFilterChange(uniqueIdentifier);
    }

    private signalFilterChange(uniqueIdentifier: string): void {
        const filterSubject = this.filterSubjects[uniqueIdentifier];
        filterSubject.next();
    }

    /**
    * Shallow copy
    */
    private cloneFilter(filter: BaseballApiFilter): BaseballApiFilter {
        return { ...filter };
    }
}
