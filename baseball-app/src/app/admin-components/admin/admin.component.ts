import { Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { ImportGameDialogComponent } from '../import-game-dialog/import-game-dialog.component';
import { BaseballApiService } from '../../baseball-api.service';
import { MatFormField, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { EditTeamDialogComponent } from '../edit-team-dialog/edit-team-dialog.component';
import { Router } from '@angular/router';
import { ImportConstantsDialogComponent } from '../import-constants-dialog/import-constants-dialog.component';

@Component({
    selector: 'app-admin',
    standalone: true,
    imports: [
        MatButtonModule,
        MatFormField,
        MatLabel,
        MatInput,
        MatButtonModule,
        MatSuffix,
        ReactiveFormsModule
    ],
    templateUrl: './admin.component.html',
    styleUrl: './admin.component.scss'
})
export class AdminViewComponent implements OnInit {

    public isLoggedIn: boolean = false;
    loginInfo: FormGroup;

    constructor(
        public importDialog: MatDialog,
        private router: Router,
        private api: BaseballApiService
    ) {
        const formBuilder = new FormBuilder();
        this.loginInfo = formBuilder.nonNullable.group({
            email: new FormControl('', [Validators.required]),
            password: new FormControl('', [Validators.required])
        });
    }

    ngOnInit(): void {
        this.api.isLoggedIn.subscribe((result) => {
            this.isLoggedIn = result;
        });
    }

    openImportDialog() {
        this.importDialog.open(ImportGameDialogComponent).afterClosed().subscribe(newGameId => {
            if (newGameId) {
                this.router.navigate(['game', newGameId]);
            }
        });
    }

    openTeamDialog() {
        this.importDialog.open(EditTeamDialogComponent);
    }

    openConstantsRefreshDialog() {
        this.importDialog.open(ImportConstantsDialogComponent);
    }

    login() {
        if (this.loginInfo.valid) {
            this.api.logIn(this.loginInfo.value.email, this.loginInfo.value.password);
        }
    }
}
