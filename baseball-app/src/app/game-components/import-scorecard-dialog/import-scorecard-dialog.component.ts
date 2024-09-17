import { Component, Inject } from '@angular/core';
import { BaseballApiService } from '../../baseball-api.service';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { GameDetail } from '../../contracts/game-detail';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { ScorecardDetail } from '../../contracts/scorecard-detail';

@Component({
    selector: 'app-import-scorecard-dialog',
    standalone: true,
    imports: [
        ReactiveFormsModule,
        MatDialogModule,
        MatFormFieldModule,
        MatButtonModule,
        MatSelectModule,
        MatInputModule
    ],
    templateUrl: './import-scorecard-dialog.component.html',
    styleUrl: './import-scorecard-dialog.component.scss'
})
export class ImportScorecardDialogComponent {

    file?: File;
    fileName?: string;
    get fileUploaded(): boolean {
        return this.fileName != null;
    }

    constructor(
        private api: BaseballApiService,
        private dialogRef: MatDialogRef<ImportScorecardDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public game: GameDetail
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
            formData.append('serializedGameId', JSON.stringify(this.game.id));
            this.api.makeApiPost<{ scorecard: ScorecardDetail }>('Media/import-scorecard', formData).subscribe(result => {
                this.dialogRef.close(result.scorecard);
            });
        }
    }
}
