import { AfterViewInit, Component, Input, OnInit, ViewChild } from '@angular/core';
import { MatTable, MatTableModule } from '@angular/material/table';
import { GameBatter } from '../contracts/game-batter';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { TypeSafeMatCellDef } from '../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../type-safe-mat-row-def.directive';
import { RouterModule } from '@angular/router';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { NgClass } from '@angular/common';

@Component({
    selector: 'app-box-score-batters',
    standalone: true,
    imports: [
        MatTableModule,
        TypeSafeMatCellDef,
        TypeSafeMatRowDef,
        MatSortModule,
        RouterModule,
        NgClass
    ],
    templateUrl: './box-score-batters.component.html',
    styleUrl: './box-score-batters.component.scss'
})
export class BoxScoreBattersComponent implements OnInit, AfterViewInit {

    @ViewChild(MatSort) sort!: MatSort;
    @ViewChild(MatTable) table!: MatTable<GameBatter>;

    @Input({ required: true })
    dataSource!: GameBatter[]
    displayedColumns = BoxScoreBattersComponent.fullSizeDisplayedColumns;

    constructor(
        private breakPointObserver: BreakpointObserver
    ) { }

    ngOnInit(): void {
        this.breakPointObserver.observe([
            Breakpoints.XSmall,
            Breakpoints.TabletPortrait
        ]).subscribe(result => {
            if (result.matches) {
                this.displayedColumns = BoxScoreBattersComponent.compactDisplayedColumns;
            } else {
                this.displayedColumns = BoxScoreBattersComponent.fullSizeDisplayedColumns;
            }
        });
    }

    ngAfterViewInit(): void {
        this.sort.sortChange.subscribe(() => {
            const basicSort = this.sort.active as (keyof GameBatter);
            if (basicSort && basicSort != 'player') {
                this.dataSource.sort((a, b) => a[basicSort] - b[basicSort]);
                if (this.sort.direction === 'desc') {
                    this.dataSource.reverse();
                }
                this.table.renderRows();
            }
        });
    }

    private static readonly fullSizeDisplayedColumns: string[] = [
        'name',
        'plateAppearances',
        'atBats',
        'hits',
        'walks',
        'singles',
        'doubles',
        'triples',
        'homeruns',
        'strikeouts',
        'strikeoutsCalled',
        'strikeoutsSwinging',
        'runs',
        'runsBattedIn',
        'stolenBases',
        'caughtStealing'
    ];
    private static readonly compactDisplayedColumns: string[] = [
        'name',
        'plateAppearances',
        'atBats',
        'hits',
        'walks',
        'homeruns',
        'strikeouts',
        'runs',
        'runsBattedIn',
        'stolenBases'
    ]

    basicColumns: (keyof GameBatter)[] = [
        'games',
        'atBats',
        'plateAppearances',
        'hits',
        'singles',
        'doubles',
        'triples',
        'homeruns',
        'runs',
        'runsBattedIn',
        'stolenBases',
        'caughtStealing',
        'walks',
        'strikeouts',
        'strikeoutsCalled',
        'strikeoutsSwinging',
        'hitByPitch',
    ];

    basicHeaders: { [key in keyof GameBatter]: string } = {
        player: '',
        number: '',
        games: 'G',
        plateAppearances: 'PA',
        atBats: 'AB',
        runs: 'R',
        hits: 'H',
        buntSingles: 'Bunt 1B',
        singles: '1B',
        doubles: '2B',
        triples: '3B',
        homeruns: 'HR',
        runsBattedIn: 'RBI',
        walks: 'BB',
        strikeouts: 'K',
        strikeoutsCalled: 'Kc',
        strikeoutsSwinging: 'Ks',
        hitByPitch: 'HBP',
        stolenBases: 'SB',
        caughtStealing: 'CS',
        sacrificeBunts: 'Sac Bunt',
        sacrificeFlies: 'SF',
        sacrifices: 'SAC',
        reachedOnError: 'E',
        fieldersChoices: 'FC',
        catchersInterference: 'CI',
        groundedIntoDoublePlay: 'GIDP',
        groundedIntoTriplePlay: 'GITP',
        atBatsWithRunnersInScoringPosition: 'AB RISP',
        hitsWithRunnersInScoringPosition: 'H RISP'
    };
}
