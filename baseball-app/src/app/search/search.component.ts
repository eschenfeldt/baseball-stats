import { AsyncPipe } from '@angular/common';
import { Component, ElementRef, ViewChild } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Observable } from 'rxjs/internal/Observable';
import { BaseballApiService } from '../baseball-api.service';
import { startWith } from 'rxjs/internal/operators/startWith';
import { catchError, switchMap } from 'rxjs';
import { SearchResult, SearchResultType } from '../contracts/search-result';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-search',
    standalone: true,
    imports: [
        FormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatAutocompleteModule,
        ReactiveFormsModule,
        MatIconModule,
        AsyncPipe
    ],
    templateUrl: './search.component.html',
    styleUrl: './search.component.scss'
})
export class SearchComponent {

    searchQuery = new FormControl('');
    filteredOptions$: Observable<SearchResult[]> = new Observable<SearchResult[]>();
    SearchResultType = SearchResultType;

    constructor(
        private router: Router,
        private api: BaseballApiService
    ) { }

    ngOnInit() {
        this.filteredOptions$ = this.searchQuery.valueChanges.pipe(
            startWith(''),
            switchMap(value => this.search(value || ''))
        );
    }

    private search(value: string | SearchResult): Observable<SearchResult[]> {
        let noResponseHandler = new Observable<SearchResult[]>(observer => {
            observer.next([]);
            observer.complete();
        });
        if (!value) {
            return noResponseHandler
        } else if (typeof value === 'string') {
            return this.api.makeApiGet<SearchResult[]>(`search/${value}`)
                // if the search fails, just return no results so we don't cancel the main subscription to search again
                .pipe(catchError(() => noResponseHandler));
        } else {
            // value is already a SearchResult, which only happens
            // when the user selects an option from the autocomplete
            return noResponseHandler;
        }
    }

    onOptionSelected(option: SearchResult): void {
        switch (option.type) {
            case SearchResultType.player:
                this.router.navigate(['/player', option.id]);
                break;
            case SearchResultType.team:
                this.router.navigate(['/team', option.id]);
                break;
            default:
                console.error('Unknown search result type:', option.type);
                break;
        }
        this.searchQuery.reset();
    }

    resultDisplay(result: SearchResult): string {
        return result ? result.name : '';
    }

    optionId(result: SearchResult): string {
        return result ? `${result.type}:${result.id}` : '';
    }
}
