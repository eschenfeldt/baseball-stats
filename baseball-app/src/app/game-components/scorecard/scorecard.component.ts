import { Component, Input, OnInit } from '@angular/core';
import { PdfViewerModule } from 'ng2-pdf-viewer';
import { GameDetail } from '../../contracts/game-detail';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Utils } from '../../utils';
import { BaseballApiService } from '../../baseball-api.service';
import { Observable } from 'rxjs';
import { AsyncPipe } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { ImportScorecardDialogComponent } from '../import-scorecard-dialog/import-scorecard-dialog.component';

@Component({
    selector: 'app-scorecard',
    imports: [
        PdfViewerModule,
        FormsModule,
        MatSelectModule,
        MatIconModule,
        MatButtonModule,
        AsyncPipe
    ],
    templateUrl: './scorecard.component.html',
    styleUrl: './scorecard.component.scss'
})
export class ScorecardComponent implements OnInit {

    @Input({ required: true })
    game!: GameDetail;

    pageOptions: PageOption[] = [];
    selectedPage?: PageOption;

    zoomed: boolean = false;
    get zoomScale() {
        return this.zoomed ? 'page-width' : 'page-height';
    }

    get hasScorecard(): boolean {
        return this.game.scorecard != null;
    }

    get url(): string | null {
        if (this.game.scorecard) {
            return Utils.keyToUrl(this.game.scorecard.file.key);
        } else {
            return null;
        }
    }

    get urlWithOrigin(): string {
        return `${this.url}?origin=${window.location.host}`;
    }

    get isLoggedIn$(): Observable<boolean> {
        return this.api.isLoggedIn;
    }

    constructor(
        private api: BaseballApiService,
        private importDialog: MatDialog
    ) { }

    ngOnInit(): void {
        this.pageOptions = [
            {
                index: 1,
                label: this.game.awayTeamName
            },
            {
                index: 2,
                label: this.game.homeTeamName
            },
            {
                index: 3,
                label: 'Pitchers'
            }
        ]
        this.api.checkLoginStatus();
    }

    openImportDialog() {
        this.importDialog.open(ImportScorecardDialogComponent, { data: this.game }).afterClosed()
            .subscribe(newScorecard => {
                this.game.scorecard = newScorecard;
            });
    }
}

interface PageOption {
    index: number
    label: string
}