import { Component, Inject } from '@angular/core';
import { BaseballApiService } from '../../baseball-api.service';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef, MatDialogTitle } from '@angular/material/dialog';
import { ImportScorecardDialogComponent } from '../import-scorecard-dialog/import-scorecard-dialog.component';
import { GameDetail } from '../../contracts/game-detail';
import { FormsModule } from '@angular/forms';
import { MatFormField } from '@angular/material/form-field';
import { HttpEventType } from '@angular/common/http';
import { MatButton } from '@angular/material/button';
import { MatInput } from '@angular/material/input';

@Component({
    selector: 'app-import-media-dialog',
    standalone: true,
    imports: [
        FormsModule,
        MatFormField,
        MatDialogTitle,
        MatDialogContent,
        MatDialogActions,
        MatDialogClose,
        MatButton,
        MatInput
    ],
    templateUrl: './import-media-dialog.component.html',
    styleUrl: './import-media-dialog.component.scss'
})
export class ImportMediaDialogComponent {

    files: File[] = [];
    fileNames: string[] = [];
    get filesUploaded(): boolean {
        return this.fileNames.length > 0;
    }

    constructor(
        private api: BaseballApiService,
        private dialogRef: MatDialogRef<ImportScorecardDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public game: GameDetail
    ) { }

    onFileSelected(event: Event) {
        this.files = Array.from((event.target as HTMLInputElement).files || []);

        if (this.files) {
            this.fileNames = this.files.map(f => f.name);
        }
    }

    import() {
        if (this.filesUploaded) {
            const formData = new FormData();
            this.files.forEach(f => {
                formData.append('files', f);
            });
            formData.append('serializedGameId', JSON.stringify(this.game.id));
            this.api.makeApiPostWithProgress<{ message: string }>('Media/import-media', formData).subscribe(result => {
                if (result.type === HttpEventType.Response) {
                    this.dialogRef.close(result.body?.message);
                } else if (result.type === HttpEventType.UploadProgress) {
                    console.log('Upload progress:', result.loaded);
                    console.log('Total bytes expected:', result.total);
                }
            });
        }
    }
}
