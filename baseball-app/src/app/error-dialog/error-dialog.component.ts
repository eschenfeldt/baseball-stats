import { HttpErrorResponse } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';

export type ErrorData = string | HttpErrorResponse;

@Component({
    selector: 'app-error-dialog',
    standalone: true,
    imports: [MatDialogModule],
    templateUrl: './error-dialog.component.html',
    styleUrl: './error-dialog.component.scss'
})
export class ErrorDialogComponent {

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
        @Inject(MAT_DIALOG_DATA) public data: ErrorData
    ) { }
}
