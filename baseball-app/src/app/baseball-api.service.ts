import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { BehaviorSubject, Observable } from 'rxjs';
import { catchError, map, mergeMap } from 'rxjs/operators';
import { ErrorDialogComponent } from './error-dialog/error-dialog.component';
import { environment } from '../environments/environment.development';

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
    ) {

    }

    public checkLoginStatus(): void {
        this.makeApiGet<boolean>('Admin/isAuthorized', true, true)
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

    private makeApiPost<T>(serviceUri: string, body: any, handleErrors: boolean = true): Observable<T> {
        const uri = BaseballApiService.apiBaseUrl + serviceUri;
        let req = this.http.post<{ d: T }>(uri, body, { responseType: 'json', withCredentials: true })
            .pipe(map((response: { d: T }) => {
                if (response == null) {
                    return response;
                } else {
                    return response.d;
                }
            }));

        if (handleErrors) {
            req = req.pipe(catchError((error) => this.throwError(error)));
        }
        return req;
    }

    private makeApiPostBlob(serviceUri: string, body: any): Observable<Blob> {
        const uri = BaseballApiService.apiBaseUrl + serviceUri;
        return this.http.post(uri, body, { responseType: 'blob' });
    }

    private makeApiGet<T>(serviceUri: string, handleErrors: boolean = true, withCredentials: boolean = false): Observable<T> {
        const uri = BaseballApiService.apiBaseUrl + serviceUri;
        let req = this.http.get<T>(uri, { responseType: 'json', withCredentials: withCredentials });

        if (handleErrors) {
            req = req.pipe(catchError((error) => this.throwError(error)));
        }
        return req;
    }

    private throwError(message: string): never {
        this.dialog.open(ErrorDialogComponent, { data: message });
        throw new Error(message);
    }
}
