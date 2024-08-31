import { Component } from '@angular/core';
import { BaseballApiService } from '../baseball-api.service';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

@Component({
    selector: 'app-import-constants-dialog',
    standalone: true,
    imports: [
        ReactiveFormsModule,
        MatDialogModule,
        MatFormFieldModule,
        MatButtonModule,
        MatSelectModule,
        MatInputModule
    ],
    templateUrl: './import-constants-dialog.component.html',
    styleUrl: './import-constants-dialog.component.scss'
})
export class ImportConstantsDialogComponent {

    file?: File;
    fileName?: string;
    get fileUploaded(): boolean {
        return this.fileName != null;
    }

    constructor(
        private api: BaseballApiService,
        private dialogRef: MatDialogRef<ImportConstantsDialogComponent>
    ) { }


    onFileSelected(event: Event) {
        const files = (event.target as HTMLInputElement).files;
        if (files) {
            this.file = files[0];
        }
        if (this.file) {
            this.fileName = this.file.name;
        }
    }

    import() {
        if (this.fileUploaded) {
            const formData = new FormData();
            formData.append('file', this.file!);
            this.api.makeApiPost<number>('Constants/refresh', formData).subscribe(result => {
                this.dialogRef.close(result);
            });
        }
    }
}
