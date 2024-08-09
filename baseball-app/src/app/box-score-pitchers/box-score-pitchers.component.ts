import { Component, Input, ViewChild } from '@angular/core';
import { GamePitcher } from '../contracts/game-pitcher';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTable, MatTableModule } from '@angular/material/table';
import { RouterModule } from '@angular/router';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { TypeSafeMatCellDef } from '../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../type-safe-mat-row-def.directive';

@Component({
    selector: 'app-box-score-pitchers',
    standalone: true,
    imports: [
        RouterModule,
        MatTableModule,
        TypeSafeMatCellDef,
        TypeSafeMatRowDef,
        MatSortModule
    ],
    templateUrl: './box-score-pitchers.component.html',
    styleUrl: './box-score-pitchers.component.scss'
})
export class BoxScorePitchersComponent {

    @ViewChild(MatSort) sort!: MatSort;
    @ViewChild(MatTable) table!: MatTable<GamePitcher>;

    @Input({ required: true })
    dataSource!: GamePitcher[]

    displayedColumns = BoxScorePitchersComponent.fullSizeDisplayedColumns;

    constructor(
        private breakPointObserver: BreakpointObserver
    ) { }

    ngOnInit(): void {
        this.breakPointObserver.observe([
            Breakpoints.XSmall,
            Breakpoints.TabletPortrait
        ]).subscribe(result => {
            if (result.matches) {
                this.displayedColumns = BoxScorePitchersComponent.compactDisplayedColumns;
            } else {
                this.displayedColumns = BoxScorePitchersComponent.fullSizeDisplayedColumns;
            }
        });
    }

    ngAfterViewInit(): void {
        this.sort.sortChange.subscribe(() => {
            const basicSort = this.sort.active as (keyof GamePitcher);
            if (basicSort && basicSort != 'player') {
                this.dataSource.sort((a, b) => a[basicSort] - b[basicSort]);
                if (this.sort.direction === 'desc') {
                    this.dataSource.reverse();
                }
                this.table.renderRows();
            }
        });
    }

    fullInningsPitched(pitcher: GamePitcher): string {
        const number = Math.floor(pitcher.thirdInningsPitched / 3);
        if (number > 0) {
            return number.toString();
        } else {
            return '';
        }
    }
    partialInningsPitched(pitcher: GamePitcher): string {
        const numerator = pitcher.thirdInningsPitched % 3;
        if (numerator == 1) {
            return '&frac13;';
        } else if (numerator == 2) {
            return '&frac23;';
        } else {
            return '';
        }
    }

    private static readonly fullSizeDisplayedColumns: string[] = [
        'name',
        'inningsPitched',
        'battersFaced',
        'pitches',
        'balls',
        'strikes',
        'runs',
        'earnedRuns',
        'strikeouts',
        'strikeoutsCalled',
        'strikeoutsSwinging',
        'hits',
        'walks',
        'intentionalWalks',
        'balks',
        'wildPitches'
    ];
    private static readonly compactDisplayedColumns: string[] = [
        'name',
        'inningsPitched',
        'battersFaced',
        'pitches',
        'runs',
        'earnedRuns',
        'strikeouts',
        'hits',
        'walks'
    ]

    basicColumns: (keyof GamePitcher)[] = [
        'games',
        'number',
        'wins',
        'losses',
        'saves',
        'battersFaced',
        'balls',
        'strikes',
        'pitches',
        'runs',
        'earnedRuns',
        'hits',
        'walks',
        'intentionalWalks',
        'strikeouts',
        'strikeoutsCalled',
        'strikeoutsSwinging',
        'hitByPitch',
        'balks',
        'wildPitches',
        'homeruns',
        'groundOuts',
        'airOuts',
        'firstPitchStrikes',
        'firstPitchBalls',
    ];

    basicHeaders: { [key in keyof GamePitcher]: string } = {
        player: '',
        number: '',
        games: 'G',
        wins: 'W',
        losses: 'L',
        saves: 'S',
        thirdInningsPitched: '',
        battersFaced: 'BF',
        balls: 'B',
        strikes: 'S',
        pitches: 'P',
        runs: 'R',
        earnedRuns: 'ER',
        hits: 'H',
        walks: 'BB',
        intentionalWalks: 'IBB',
        strikeouts: 'K',
        strikeoutsCalled: 'Kc',
        strikeoutsSwinging: 'Ks',
        hitByPitch: 'HBP',
        balks: 'Balks',
        wildPitches: 'WP',
        homeruns: 'HR',
        groundOuts: 'GO',
        airOuts: 'AO',
        firstPitchStrikes: 'FPS',
        firstPitchBalls: 'FPB'
    };
}

interface InningsPitched {
    full: number;
    thirds: number;
}
