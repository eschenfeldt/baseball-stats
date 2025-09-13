import { Component, OnInit } from '@angular/core';
import { BaseballApiService } from '../../baseball-api.service';
import { param } from '../../param.decorator';
import { BASEBALL_ROUTES } from '../../app.routes';
import { Observable, switchMap } from 'rxjs';
import { GameDetail } from '../../contracts/game-detail';
import { AsyncPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonToggleChange, MatButtonToggleModule } from '@angular/material/button-toggle';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatTabChangeEvent, MatTabsModule } from '@angular/material/tabs';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { GameBatter } from '../../contracts/game-batter';
import { BoxScoreBattersComponent } from '../box-score-batters/box-score-batters.component';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { GameDetailsComponent } from '../game-details/game-details.component';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { GameFielder } from '../../contracts/game-fielder';
import { GamePitcher } from '../../contracts/game-pitcher';
import { BoxScorePitchersComponent } from '../box-score-pitchers/box-score-pitchers.component';
import { BoxScoreFieldersComponent } from '../box-score-fielders/box-score-fielders.component';
import { ScorecardComponent } from '../scorecard/scorecard.component';
import { MediaGalleryComponent } from '../../media-components/media-gallery/media-gallery.component';
import { Team } from '../../contracts/team';
import { Utils } from '../../utils';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ImportMediaDialogComponent } from '../import-media-dialog/import-media-dialog.component';
import { MatIcon } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
    selector: 'app-game',
    imports: [
        AsyncPipe,
        RouterModule,
        MatCardModule,
        MatButtonToggleModule,
        FormsModule,
        ReactiveFormsModule,
        MatTabsModule,
        MatProgressSpinnerModule,
        MatDialogModule,
        MatIcon,
        GameDetailsComponent,
        BoxScoreBattersComponent,
        BoxScorePitchersComponent,
        BoxScoreFieldersComponent,
        ScorecardComponent,
        MediaGalleryComponent
    ],
    templateUrl: './game.component.html',
    styleUrl: './game.component.scss'
})
export class GameComponent implements OnInit {

    @param<typeof BASEBALL_ROUTES.GAME>('gameId')
    gameId$!: Observable<number>
    @param<typeof BASEBALL_ROUTES.GAME_TAB>('tab')
    tab$?: Observable<string>
    @param<typeof BASEBALL_ROUTES.GAME_BOX_SCORE>('boxScoreOption')
    boxScoreOption$?: Observable<string>
    game$?: Observable<GameDetail>;

    boxScoreOption: BoxScoreOption = BoxScoreOption.homeBatters;
    abbreviateBoxScoreOptions = false;
    condensedHeader = false;
    tabIndex: GameTab = GameTab.boxScore;

    get boxScoresActive(): boolean {
        return this.tabIndex === GameTab.boxScore;
    }

    public battersDataSource(game: GameDetail): GameBatter[] | null {
        switch (this.boxScoreOption) {
            case BoxScoreOption.homeBatters:
                return game.homeBoxScore.batters;
            case BoxScoreOption.awayBatters:
                return game.awayBoxScore.batters;
            default:
                return null;
        }
    }
    public fieldersDataSource(game: GameDetail): GameFielder[] | null {
        switch (this.boxScoreOption) {
            case BoxScoreOption.homeFielders:
                return game.homeBoxScore.fielders;
            case BoxScoreOption.awayFielders:
                return game.awayBoxScore.fielders;
            default:
                return null;
        }
    }
    public pitchersDataSource(game: GameDetail): GamePitcher[] | null {
        switch (this.boxScoreOption) {
            case BoxScoreOption.homePitchers:
                return game.homeBoxScore.pitchers;
            case BoxScoreOption.awayPitchers:
                return game.awayBoxScore.pitchers;
            default:
                return null;
        }
    }

    public color(team: Team): string {
        return Utils.teamColorOrDefault(team);
    }

    get isLoggedIn$(): Observable<boolean> {
        return this.api.isLoggedIn;
    }

    constructor(
        private breakpointObserver: BreakpointObserver,
        private api: BaseballApiService,
        private importDialog: MatDialog,
        private snackBar: MatSnackBar,
        private route: ActivatedRoute,
        private router: Router
    ) { }

    public ngOnInit(): void {

        this.api.checkLoginStatus();
        this.loadGame();

        this.breakpointObserver.observe([
            Breakpoints.XSmall,
            Breakpoints.HandsetLandscape,
            Breakpoints.TabletPortrait // sidebar menu shows up here
        ]).subscribe(result => {
            if (result.matches) {
                this.abbreviateBoxScoreOptions = true;
            } else {
                this.abbreviateBoxScoreOptions = false;
            }
        });
        this.breakpointObserver.observe([
            Breakpoints.HandsetLandscape
        ]).subscribe(result => {
            if (result.matches) {
                this.condensedHeader = true;
            } else {
                this.condensedHeader = false;
            }
        });
        if (this.tab$ != null) {
            this.tab$.subscribe(tab => {
                this.setTabFromParam(tab);
            });
        }
        if (this.boxScoreOption$ != null) {
            this.boxScoreOption$.subscribe(option => {
                this.setBoxScoreOptionFromParam(option);
            });
        }
    }

    private loadGame(): void {
        this.game$ = this.gameId$.pipe(
            switchMap((gameId) => {
                return this.api.makeApiGet<GameDetail>(`games/${gameId}`);
            }));
    }

    private setTabFromParam(tab: string): void {
        if (tab == null) {
            this.tabIndex = GameTab.boxScore;
            return;
        }
        switch (tab.toLowerCase()) {
            case 'details':
                this.tabIndex = GameTab.details;
                break;
            case 'boxscore':
                this.tabIndex = GameTab.boxScore;
                break;
            case 'scorecard':
                this.tabIndex = GameTab.scoreCard;
                break;
            case 'media':
                this.tabIndex = GameTab.media;
                break;
            default:
                this.tabIndex = GameTab.boxScore;
        }
    }

    private setBoxScoreOptionFromParam(option: string): void {
        if (option == null) {
            this.boxScoreOption = BoxScoreOption.homeBatters;
            return;
        }
        switch (option.toLowerCase()) {
            case 'awaybatters':
                this.boxScoreOption = BoxScoreOption.awayBatters;
                break;
            case 'awaypitchers':
                this.boxScoreOption = BoxScoreOption.awayPitchers;
                break;
            case 'awayfielders':
                this.boxScoreOption = BoxScoreOption.awayFielders;
                break;
            case 'homebatters':
                this.boxScoreOption = BoxScoreOption.homeBatters;
                break;
            case 'homepitchers':
                this.boxScoreOption = BoxScoreOption.homePitchers;
                break;
            case 'homefielders':
                this.boxScoreOption = BoxScoreOption.homeFielders;
                break;
            default:
                this.boxScoreOption = BoxScoreOption.homeBatters;
        }
    }

    public setRouteFromTab(event: MatTabChangeEvent): void {
        let tab = '';
        switch (event.index) {
            case GameTab.details:
                tab = 'details';
                break;
            case GameTab.boxScore:
                tab = 'boxscore';
                break;
            case GameTab.scoreCard:
                tab = 'scorecard';
                break;
            case GameTab.media:
                tab = 'media';
                break;
            default:
                tab = 'boxscore';
        }
        this.router.navigate(['/', 'game', this.route.snapshot.params['gameId'], tab]);
    }

    public setRouteFromBoxScoreOption(event: MatButtonToggleChange): void {
        let option = '';
        switch (event.value) {
            case BoxScoreOption.awayBatters:
                option = 'awayBatters';
                break;
            case BoxScoreOption.awayPitchers:
                option = 'awayPitchers';
                break;
            case BoxScoreOption.awayFielders:
                option = 'awayFielders';
                break;
            case BoxScoreOption.homeBatters:
                option = 'homeBatters';
                break;
            case BoxScoreOption.homePitchers:
                option = 'homePitchers';
                break;
            case BoxScoreOption.homeFielders:
                option = 'homeFielders';
                break;
            default:
                option = 'homeBatters';
        }
        this.router.navigate(['/', 'game', this.route.snapshot.params['gameId'], 'boxscore', option]);
    }

    public hasMedia(game: GameDetail): boolean {
        return game.hasMedia;
    }

    public get batterLabel(): string {
        return this.abbreviateBoxScoreOptions ? 'B' : 'Batters';
    }
    public get pitcherLabel(): string {
        return this.abbreviateBoxScoreOptions ? 'P' : 'Pitchers';
    }
    public get fielderLabel(): string {
        return this.abbreviateBoxScoreOptions ? 'F' : 'Fielders';
    }

    public gameDate(game: GameDetail): string {
        return Utils.formatDate(game.date);
    }

    openImportMediaDialog(game: GameDetail) {
        this.importDialog.open(ImportMediaDialogComponent, { data: game }).afterClosed().subscribe(result => {
            if (result) {
                this.snackBar.open(result, 'Cool', {
                    duration: 5000
                });
                this.loadGame();
            }
        })
    }
}

enum GameTab {
    details,
    boxScore,
    scoreCard,
    media
}

enum BoxScoreOption {
    awayBatters = 'awayBatters',
    awayPitchers = 'awayPitchers',
    awayFielders = 'awayFielders',
    homeBatters = 'homeBatters',
    homePitchers = 'homePitchers',
    homeFielders = 'homeFielders'
}
