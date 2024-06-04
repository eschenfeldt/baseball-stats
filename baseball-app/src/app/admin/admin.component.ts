import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { ImportGameDialogComponent } from '../import-game-dialog/import-game-dialog.component';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [
    MatButtonModule
  ],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.scss'
})
export class AdminViewComponent {

  constructor(public importDialog: MatDialog) { }

  openImportDialog() {
    this.importDialog.open(ImportGameDialogComponent)
  }
}
