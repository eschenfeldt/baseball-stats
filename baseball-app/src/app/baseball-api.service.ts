import { HttpClient, HttpErrorResponse, HttpEvent } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { BehaviorSubject, Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ErrorDialogComponent } from './util-components/error-dialog/error-dialog.component';
import { environment } from '../environments/environment';

export enum ApiMethod {
    GET,
    QUERY,
    POST
}

@Injectable({
    providedIn: 'root'
})
export class BaseballApiService {

    static get apiBaseUrl(): string {
        return environment.apiUrl;
    }

    private loggedInSubject = new BehaviorSubject<boolean>(false);
    public get isLoggedIn(): Observable<boolean> {
        return this.loggedInSubject.asObservable();
    }

    constructor(
        private http: HttpClient,
        private dialog: MatDialog
    ) { }

    public checkLoginStatus(): void {
        this.makeApiGet<boolean>('Admin/isAuthorized', null, false, true)
            .pipe(catchError((error) => {
                this.loggedInSubject.next(false);
                throw error;
            }))
            .subscribe(result => {
                this.loggedInSubject.next(result);
            });
    }

    public logIn(email: string, password: string): void {
        this.makeApiPost<boolean>('Admin/login', {
            email: email,
            password: password
        }).subscribe(() => this.checkLoginStatus());
    }

    public makeApiPost<T>(serviceUri: string, body: any, handleErrors: boolean = true): Observable<T> {
        const uri = BaseballApiService.apiBaseUrl + serviceUri;
        let req = this.http.post<T>(uri, body, { responseType: 'json', withCredentials: true });

        if (handleErrors) {
            req = req.pipe(catchError((error) => this.throwError(error)));
        }
        return req;
    }

    public makeApiPostWithProgress<T>(serviceUri: string, body: any, handleErrors: boolean = true): Observable<HttpEvent<T>> {
        const uri = BaseballApiService.apiBaseUrl + serviceUri;
        let req = this.http.post<T>(uri, body, { responseType: 'json', withCredentials: true, reportProgress: true, observe: 'events' });
        if (handleErrors) {
            req = req.pipe(catchError((error) => this.throwError(error)));
        }
        return req;
    }

    private makeApiPostBlob(serviceUri: string, body: any): Observable<Blob> {
        const uri = BaseballApiService.apiBaseUrl + serviceUri;
        return this.http.post(uri, body, { responseType: 'blob' });
    }

    public makeApiGet<T>(serviceUri: string, queryParams: any = null, handleErrors: boolean = true, withCredentials: boolean = false): Observable<T> {
        let uri = BaseballApiService.apiBaseUrl + serviceUri;
        if (queryParams) {
            uri += `?${Object.keys(queryParams)
                .filter(key => queryParams[key] != null)
                .map(key => `${key}=${encodeURIComponent(queryParams[key])}`).join('&')}`;
        }
        let req = this.http.get<T>(uri, { responseType: 'json', withCredentials: withCredentials });

        if (handleErrors) {
            req = req.pipe(catchError((error) => this.throwError(error)));
        }
        return req;
    }

    private throwError(message: string | HttpErrorResponse): never {
        this.dialog.open(ErrorDialogComponent, { data: message });
        if (message instanceof HttpErrorResponse) {
            throw message;
        } else {
            throw new Error(message);
        }
    }
}
