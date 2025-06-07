import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Observable } from 'rxjs/internal/Observable';
import { BaseballApiService } from '../baseball-api.service';
import { startWith } from 'rxjs/internal/operators/startWith';
import { mergeMap } from 'rxjs';

@Component({
    selector: 'app-search',
    standalone: true,
    imports: [
        FormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatAutocompleteModule,
        ReactiveFormsModule,
        AsyncPipe
    ],
    templateUrl: './search.component.html',
    styleUrl: './search.component.scss'
})
export class SearchComponent {
    searchQuery = new FormControl('');
    filteredOptions$: Observable<string[]> = new Observable<string[]>();
    options: string[] = [];

    constructor(
        private api: BaseballApiService
    ) { }

    ngOnInit() {
        // Simulate fetching options from a service
        this.filteredOptions$ = this.searchQuery.valueChanges.pipe(
            startWith(''),
            mergeMap(value => this.search(value || ''))
        );
    }

    private search(value: string): Observable<string[]> {
        if (!value.trim()) {
            return new Observable<string[]>(observer => {
                observer.next([]);
                observer.complete();
            });
        } else {
            return this.api.makeApiGet<string[]>(`search/${value}`);
        }
    }

    onOptionSelected(option: any): void {
        // Handle the selected option
        console.log('Selected option:', option);
        // You can navigate to a different route or perform any action with the selected option
    }
}
