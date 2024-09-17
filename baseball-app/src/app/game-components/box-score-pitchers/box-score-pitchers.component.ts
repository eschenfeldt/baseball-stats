import { Component, Input, ViewChild } from '@angular/core';
import { GamePitcher } from '../../contracts/game-pitcher';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTable, MatTableModule } from '@angular/material/table';
import { RouterModule } from '@angular/router';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { TypeSafeMatCellDef } from '../../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../../type-safe-mat-row-def.directive';
import { StatDefCollection } from '../../contracts/stat-def';
import { Utils } from '../../utils';

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

    @Input({ required: true })
    stats!: StatDefCollection

    displayedColumns = BoxScorePitchersComponent.fullSizeDisplayedColumns;

    get statNames(): string[] {
        return Object.keys(this.stats);
    }

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
            const basicSort = this.sort.active as (keyof StatDefCollection);
            if (basicSort && basicSort != 'player') {
                this.dataSource.sort((a, b) => a.stats[basicSort] - b.stats[basicSort]);
                if (this.sort.direction === 'desc') {
                    this.dataSource.reverse();
                }
                this.table.renderRows();
            }
        });
    }

    fullInningsPitched(pitcher: GamePitcher): string {
        return Utils.fullInningsPitched(pitcher.stats);
    }
    partialInningsPitched(pitcher: GamePitcher): string {
        return Utils.partialInningsPitched(pitcher.stats);
    }

    private static readonly fullSizeDisplayedColumns: string[] = [
        'name',
        'inningsPitched',
        'BattersFaced',
        'Pitches',
        'Balls',
        'Strikes',
        'Runs',
        'EarnedRuns',
        'Strikeouts',
        'StrikeoutsCalled',
        'StrikeoutsSwinging',
        'Hits',
        'Walks',
        'IntentionalWalks',
        'Balks',
        'WildPitches'
    ];
    private static readonly compactDisplayedColumns: string[] = [
        'name',
        'inningsPitched',
        'BattersFaced',
        'Pitches',
        'Runs',
        'EarnedRuns',
        'Strikeouts',
        'Hits',
        'Walks'
    ]
}
