import { AsyncPipe } from '@angular/common';
import { Component, ElementRef, ViewChild } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Observable } from 'rxjs/internal/Observable';
import { BaseballApiService } from '../baseball-api.service';
import { startWith } from 'rxjs/internal/operators/startWith';
import { mergeMap } from 'rxjs';
import { SearchResult, SearchResultType } from '../contracts/search-result';
import { Router } from '@angular/router';

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
    filteredOptions$: Observable<SearchResult[]> = new Observable<SearchResult[]>();
    // TODO: get this to actually work
    @ViewChild('searchInput') searchInput?: ElementRef<HTMLInputElement>;
    options: string[] = [];

    constructor(
        private router: Router,
        private api: BaseballApiService
    ) { }

    ngOnInit() {
        // Simulate fetching options from a service
        this.filteredOptions$ = this.searchQuery.valueChanges.pipe(
            startWith(''),
            mergeMap(value => this.search(value || ''))
        );
    }

    private search(value: string): Observable<SearchResult[]> {
        // TODO: debounce and cancel previous requests
        if (!value) {
            return new Observable<SearchResult[]>(observer => {
                observer.next([]);
                observer.complete();
            });
        } else {
            return this.api.makeApiGet<SearchResult[]>(`search/${value}`);
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
        this.searchInput?.nativeElement.blur();
    }

    resultDisplay(result: SearchResult): string {
        return result ? result.name : '';
    }

    optionId(result: SearchResult): string {
        return result ? `${result.type}:${result.id}` : '';
    }
}
