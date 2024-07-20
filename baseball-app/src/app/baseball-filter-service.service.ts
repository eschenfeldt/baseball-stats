import { Injectable } from '@angular/core';
import { BehaviorSubject, debounceTime, Observable } from 'rxjs';

export interface BaseballApiFilter {
    [propertyName: string]: any
}

@Injectable({
    providedIn: 'root'
})
export class BaseballFilterServiceService {

    private filters: {
        [tableIdentifier: string]: BaseballApiFilter
    } = {};

    private filterSubjects: {
        [tableIdentifier: string]: BehaviorSubject<void>
    } = {};

    constructor() { }

    private getFilters<T>(uniqueIdentifier: string): T {
        return this.filters[uniqueIdentifier] as T;
    }

    public getFilterValue<T>(uniqueIdentifier: string, filterName: keyof T): any {
        const filters = this.getFilters<T>(uniqueIdentifier);
        return filters[filterName];
    }

    public setFilterValue<T>(uniqueIdentifier: string, filterName: keyof T, value: any): void {
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

    public initFilters(uniqueIdentifier: string, defaultFilters?: any): void {
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

    public resetFilters(uniqueIdentifier: string, defaultFilters?: any): void {
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
    private cloneFilter(filter: any): any {
        return { ...filter };
    }
}
