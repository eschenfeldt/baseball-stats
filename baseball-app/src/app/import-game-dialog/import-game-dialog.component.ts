import { Component, OnInit } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { MatFormField, MatFormFieldModule, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatInput, MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogActions, MatDialogClose, MatDialogContent, MatDialogTitle } from '@angular/material/dialog';
import { MtxDatetimepicker, MtxDatetimepickerInput, MtxDatetimepickerToggle } from '@ng-matero/extensions/datetimepicker';
import { provideMomentDatetimeAdapter } from '@ng-matero/extensions-moment-adapter';
import { NgFor, NgIf } from '@angular/common';
import { BaseballApiService } from '../baseball-api.service';
import { Team } from '../team';

@Component({
    selector: 'app-import-game-dialog',
    standalone: true,
    imports: [
        NgIf,
        NgFor,
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
        MatSuffix,
        MtxDatetimepicker,
        MtxDatetimepickerInput,
        MtxDatetimepickerToggle,
        MatSelectModule,
        MatInputModule
    ],
    providers: [
        provideMomentDatetimeAdapter({
            parse: {
                datetimeInput: 'YYYY-MM-DD hh:mm A',
            },
            display: {
                dateInput: 'YYYY-MM-DD',
                monthInput: 'MMMM',
                yearInput: 'YYYY',
                timeInput: 'HH:mm',
                datetimeInput: 'YYYY-MM-DD hh:mm A',
                monthYearLabel: 'YYYY MMMM',
                dateA11yLabel: 'LL',
                monthYearA11yLabel: 'MMMM YYYY',
                popupHeaderDateLabel: 'MMM DD, ddd',
            },
        })
    ],
    templateUrl: './import-game-dialog.component.html',
    styleUrl: './import-game-dialog.component.scss'
})
export class ImportGameDialogComponent implements OnInit {

    scheduledDateTime = new UntypedFormControl();
    startDateTime = new UntypedFormControl();
    endDateTime = new UntypedFormControl();
    homeTeam = new FormControl<Team | null>(null);
    awayTeam = new FormControl<Team | null>(null);

    files: File[] = [];
    fileNames: string[] = [];
    get filesUploaded(): boolean {
        return this.fileNames.length > 0;
    }

    teams: Team[] = [];

    constructor(
        private api: BaseballApiService
    ) { }

    ngOnInit(): void {
        this.loadTeams();
    }

    onFileSelected(event: Event) {
        console.log(event);

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
            this.api.makeApiPost('Games/import', formData).subscribe();
        }
    }

    loadTeams(): void {
        this.api.makeApiGet<Team[]>('Teams').subscribe(teams => {
            this.teams = teams;
        })
    }
}
