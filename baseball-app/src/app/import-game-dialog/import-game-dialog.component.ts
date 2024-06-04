import { Component } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { MatFormField, MatFormFieldModule, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogActions, MatDialogClose, MatDialogContent, MatDialogTitle } from '@angular/material/dialog';
import { MtxDatetimepicker, MtxDatetimepickerInput, MtxDatetimepickerToggle } from '@ng-matero/extensions/datetimepicker';
import { provideMomentDatetimeAdapter } from '@ng-matero/extensions-moment-adapter';
import { NgFor, NgIf } from '@angular/common';

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
    MtxDatetimepickerToggle
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
export class ImportGameDialogComponent {

  scheduledDateTime = new UntypedFormControl();
  startDateTime = new UntypedFormControl();
  endDateTime = new UntypedFormControl();

  fileNames: string[] = [];
  get filesUploaded(): boolean {
    return this.fileNames.length > 0;
  }

  onFileSelected(event: Event) {
    console.log(event);

    const files = Array.from((event.target as HTMLInputElement).files || []);

    if (files) {
      this.fileNames = files.map(f => f.name);
    }
  }
}
