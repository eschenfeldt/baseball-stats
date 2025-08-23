import { AfterViewInit, Component, Input, OnInit, ViewChild } from '@angular/core';
import { MatTable, MatTableModule } from '@angular/material/table';
import { GameBatter } from '../../contracts/game-batter';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { TypeSafeMatCellDef } from '../../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../../type-safe-mat-row-def.directive';
import { RouterModule } from '@angular/router';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { StatDefCollection } from '../../contracts/stat-def';

@Component({
    selector: 'app-box-score-batters',
    imports: [
        MatTableModule,
        TypeSafeMatCellDef,
        TypeSafeMatRowDef,
        MatSortModule,
        RouterModule
    ],
    templateUrl: './box-score-batters.component.html',
    styleUrl: './box-score-batters.component.scss'
})
export class BoxScoreBattersComponent implements OnInit, AfterViewInit {

    @ViewChild(MatSort) sort!: MatSort;
    @ViewChild(MatTable) table!: MatTable<GameBatter>;

    @Input({ required: true })
    dataSource!: GameBatter[]

    @Input({ required: true })
    stats!: StatDefCollection
    displayedColumns = BoxScoreBattersComponent.fullSizeDisplayedColumns;

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
                this.displayedColumns = BoxScoreBattersComponent.compactDisplayedColumns;
            } else {
                this.displayedColumns = BoxScoreBattersComponent.fullSizeDisplayedColumns;
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

    private static readonly fullSizeDisplayedColumns: string[] = [
        'name',
        'PlateAppearances',
        'AtBats',
        'Hits',
        'Walks',
        'Singles',
        'Doubles',
        'Triples',
        'Homeruns',
        'Strikeouts',
        'StrikeoutsCalled',
        'StrikeoutsSwinging',
        'Runs',
        'RunsBattedIn',
        'StolenBases',
        'CaughtStealing'
    ];
    private static readonly compactDisplayedColumns: string[] = [
        'name',
        'PlateAppearances',
        'AtBats',
        'Hits',
        'Walks',
        'Homeruns',
        'Strikeouts',
        'Runs',
        'RunsBattedIn',
        'StolenBases'
    ]
}
