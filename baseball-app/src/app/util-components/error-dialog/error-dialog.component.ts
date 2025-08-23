import { HttpErrorResponse } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { BaseballApiService } from '../../baseball-api.service';

export type ErrorData = string | HttpErrorResponse;

@Component({
    selector: 'app-error-dialog',
    imports: [MatDialogModule],
    templateUrl: './error-dialog.component.html',
    styleUrl: './error-dialog.component.scss'
})
export class ErrorDialogComponent implements OnInit {

    public isLoggedIn: boolean = false;

    public get errorText(): string | null {
        if (this.data instanceof HttpErrorResponse) {
            return null;
        } else {
            return this.data;
        }
    }

    public get error(): HttpErrorResponse | null {
        if (this.data instanceof HttpErrorResponse) {
            return this.data;
        } else {
            return null;
        }
    }

    constructor(
        private dialogRef: MatDialogRef<ErrorDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: ErrorData,
        private api: BaseballApiService
    ) { }

    public ngOnInit(): void {
        this.api.isLoggedIn.subscribe((isLoggedIn) => this.isLoggedIn = isLoggedIn);
        this.api.checkLoginStatus()
    }
}
