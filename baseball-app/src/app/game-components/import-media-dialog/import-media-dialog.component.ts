import { Component, Inject, OnInit } from '@angular/core';
import { BaseballApiService } from '../../baseball-api.service';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef, MatDialogTitle } from '@angular/material/dialog';
import { ImportScorecardDialogComponent } from '../import-scorecard-dialog/import-scorecard-dialog.component';
import { GameDetail } from '../../contracts/game-detail';
import { FormsModule } from '@angular/forms';
import { MatFormField } from '@angular/material/form-field';
import { HttpEventType } from '@angular/common/http';
import { MatButton } from '@angular/material/button';
import { MatInput } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ImportTask } from '../../contracts/import-task';
import { MediaImportTaskStatus } from '../../contracts/media-import-task-status';

@Component({
    selector: 'app-import-media-dialog',
    imports: [
        FormsModule,
        MatFormField,
        MatDialogTitle,
        MatDialogContent,
        MatDialogActions,
        MatDialogClose,
        MatButton,
        MatInput,
        MatProgressBarModule
    ],
    templateUrl: './import-media-dialog.component.html',
    styleUrl: './import-media-dialog.component.scss'
})
export class ImportMediaDialogComponent implements OnInit {

    files: File[] = [];
    fileNames: string[] = [];
    get filesUploaded(): boolean {
        return this.fileNames.length > 0;
    }
    uploading = false;
    activeTask?: ImportTask;
    uploadPercentage: number = 0;
    processPercentage: number = 0;
    private statusPollTimeout?: ReturnType<typeof setTimeout>;

    constructor(
        private api: BaseballApiService,
        private dialogRef: MatDialogRef<ImportScorecardDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public game: GameDetail
    ) { }

    ngOnInit(): void {
        this.getLatestImportTask();
    }

    private getLatestImportTask(): void {
        this.api.makeApiGet<ImportTask[]>('Media/import-tasks', { gameId: this.game.id }, true, true).subscribe(tasks => {
            if (tasks && tasks.length > 0) {
                this.processTaskStatus(tasks[0]);
            }
        });
    }

    onFileSelected(event: Event) {
        this.files = Array.from((event.target as HTMLInputElement).files || []);

        if (this.files) {
            this.fileNames = this.files.map(f => f.name);
        }
    }

    import() {
        if (this.filesUploaded) {
            this.uploading = true;
            const formData = new FormData();
            this.files.forEach(f => {
                formData.append('files', f);
            });
            formData.append('serializedGameId', JSON.stringify(this.game.id));
            this.api.makeApiPostWithProgress<ImportTask>('Media/import-media', formData).subscribe(result => {
                if (result.type === HttpEventType.Response) {
                    if (result.body && !this.activeTask) {
                        this.processTaskStatus(result.body);
                    } else {
                        this.getLatestImportTask();
                    }
                    this.uploading = false;
                } else if (result.type === HttpEventType.UploadProgress) {
                    this.uploadPercentage = Math.round((result.loaded / (result.total || 1)) * 100);
                }
            });
        }
    }

    processTaskStatus(task: ImportTask): void {
        this.activeTask = task;
        if (task.status === MediaImportTaskStatus.Completed) {
            this.activeTask = undefined;
            this.dialogRef.close(task.message);
        } else if (task.status === MediaImportTaskStatus.Failed) {
            this.dialogRef.close(`Import failed: ${task.message}`);
        } else {
            this.processPercentage = Math.round(task.progress * 100);
            // Poll for status updates
            if (this.statusPollTimeout) {
                clearTimeout(this.statusPollTimeout);
            }
            this.statusPollTimeout = setTimeout(() => {
                this.api.makeApiGet<ImportTask>(`Media/import-status/${task.id}`, null, true, true).subscribe(updatedTask => {
                    this.processTaskStatus(updatedTask);
                });
            }, 1000); // Poll every second
        }
    }

    kickTask() {
        if (this.activeTask) {
            this.api.makeApiPost<ImportTask>(`Media/restart-import-task/${this.activeTask.id}`, null).subscribe(result => {
                this.processTaskStatus(result);
            });
        }
    }
}
