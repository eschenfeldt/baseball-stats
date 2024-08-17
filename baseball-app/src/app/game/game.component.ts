import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { BaseballApiService } from '../baseball-api.service';
import { param } from '../param.decorator';
import { BASEBALL_ROUTES } from '../app.routes';
import { Observable, switchMap } from 'rxjs';
import { GameDetail } from '../contracts/game-detail';
import { AsyncPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { GameBatter } from '../contracts/game-batter';
import { BoxScoreBattersComponent } from '../box-score-batters/box-score-batters.component';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { GameDetailsComponent } from '../game-details/game-details.component';
import { RouterModule } from '@angular/router';
import { GameFielder } from '../contracts/game-fielder';
import { GamePitcher } from '../contracts/game-pitcher';
import { BoxScorePitchersComponent } from '../box-score-pitchers/box-score-pitchers.component';
import { BoxScoreFieldersComponent } from '../box-score-fielders/box-score-fielders.component';
import { ScorecardComponent } from '../scorecard/scorecard.component';
import { LivePhoto } from '../contracts/live-photo';
import { LivePhotoComponent } from '../live-photo/live-photo.component';
import { MediaGalleryComponent } from '../media-gallery/media-gallery.component';

@Component({
    selector: 'app-game',
    standalone: true,
    imports: [
        AsyncPipe,
        RouterModule,
        MatCardModule,
        MatButtonToggleModule,
        FormsModule,
        ReactiveFormsModule,
        MatTabsModule,
        MatProgressSpinnerModule,
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
    game$?: Observable<GameDetail>;

    @ViewChild('livephoto') livePhotoDiv?: ElementRef;

    boxScoreOption: BoxScoreOption = BoxScoreOption.homeBatters;
    abbreviateBoxScoreOptions = false;
    tabIndex: GameTab = GameTab.media;

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

    constructor(
        private breakpointObserver: BreakpointObserver,
        private api: BaseballApiService
    ) { }

    public ngOnInit(): void {

        this.game$ = this.gameId$.pipe(
            switchMap((gameId) => {
                return this.api.makeApiGet<GameDetail>(`games/${gameId}`);
            }));

        this.breakpointObserver.observe([
            Breakpoints.XSmall,
            Breakpoints.TabletPortrait // sidebar menu shows up here
        ]).subscribe(result => {
            if (result.matches) {
                this.abbreviateBoxScoreOptions = true;
            } else {
                this.abbreviateBoxScoreOptions = false;
            }
        });
    }

    public hasMedia(game: GameDetail): boolean {
        return true;
    }

    scorecardUrl(game: GameDetail): string {
        return 'https://eschenfeldt-baseball-media.nyc3.cdn.digitaloceanspaces.com/scorecards/scorecard.pdf';
    }
    get livePhoto(): LivePhoto {
        return {
            photo: {
                url: 'https://eschenfeldt-baseball-media.nyc3.cdn.digitaloceanspaces.com/live-photos/IMG_4316.HEIC'
            },
            video: {
                url: 'https://eschenfeldt-baseball-media.nyc3.cdn.digitaloceanspaces.com/live-photos/IMG_4316.mov'
            }
        }
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
