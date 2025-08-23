import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, UntypedFormControl, Validators } from '@angular/forms';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput, MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef, MatDialogTitle } from '@angular/material/dialog';

import { BaseballApiService } from '../../baseball-api.service';
import { Team } from '../../contracts/team';
import { GameMetadata } from '../../contracts/game-metadata';

@Component({
    selector: 'app-import-game-dialog',
    imports: [
    FormsModule,
    ReactiveFormsModule,
    MatLabel,
    MatInput,
    MatFormField,
    MatDialogTitle,
    MatDialogContent,
    MatDialogActions,
    MatDialogClose,
    MatButtonModule,
    MatSelectModule,
    MatInputModule
],
    templateUrl: './import-game-dialog.component.html',
    styleUrl: './import-game-dialog.component.scss'
})
export class ImportGameDialogComponent implements OnInit {

    metadata = new FormGroup({
        scheduledStartDate: new FormControl<string | null>(null, Validators.required),
        scheduledStartTime: new FormControl<string | null>(null, Validators.required),
        actualStartDate: new FormControl<string | null>(null, Validators.required),
        actualStartTime: new FormControl<string | null>(null, Validators.required),
        endDate: new FormControl<string | null>(null, Validators.required),
        endTime: new FormControl<string | null>(null, Validators.required),
        home: new FormControl<Team | null>(null, Validators.required),
        away: new FormControl<Team | null>(null, Validators.required)
    })

    files: File[] = [];
    fileNames: string[] = [];
    get filesUploaded(): boolean {
        return this.fileNames.length > 0;
    }

    teams: Team[] = [];

    get canImport(): boolean {
        return this.filesUploaded
            && this.metadata.valid
    }

    private get metadataValue(): GameMetadata | null {
        if (this.metadata.valid && this.metadata.value.home && this.metadata.value.away) {
            const scheduled = this.metadata.value.scheduledStartDate ? new Date(`${this.metadata.value.scheduledStartDate} ${this.metadata.value.scheduledStartTime}`) : null;
            const actual = this.metadata.value.actualStartDate ? new Date(`${this.metadata.value.actualStartDate} ${this.metadata.value.actualStartTime}`) : null;
            const end = this.metadata.value.endDate ? new Date(`${this.metadata.value.endDate} ${this.metadata.value.endTime}`) : null;
            return {
                home: this.metadata.value.home,
                away: this.metadata.value.away,
                scheduledStart: scheduled,
                actualStart: actual,
                end: end
            }
        } else {
            return null;
        }
    }

    constructor(
        private api: BaseballApiService,
        private dialogRef: MatDialogRef<ImportGameDialogComponent>
    ) { }

    ngOnInit(): void {
        this.loadTeams();
    }

    onFileSelected(event: Event) {
        this.files = Array.from((event.target as HTMLInputElement).files || []);

        if (this.files) {
            this.fileNames = this.files.map(f => f.name);
        }
    }

    import() {
        if (this.canImport) {
            const formData = new FormData();
            this.files.forEach(f => {
                formData.append('files', f);
            });
            formData.append('serializedMetadata', JSON.stringify(this.metadataValue))
            this.api.makeApiPost<{ id: number }>('Games/import', formData).subscribe(result => {
                this.dialogRef.close(result.id);
            });
        }
    }

    loadTeams(): void {
        this.api.makeApiGet<Team[]>('Teams').subscribe(teams => {
            this.teams = teams.sort((a, b) => a.city.localeCompare(b.city));
        })
    }
}
