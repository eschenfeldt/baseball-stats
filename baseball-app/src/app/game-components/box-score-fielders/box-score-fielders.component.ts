import { AfterViewInit, Component, Input, ViewChild } from '@angular/core';
import { GameFielder } from '../../contracts/game-fielder';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTable, MatTableModule } from '@angular/material/table';
import { RouterModule } from '@angular/router';
import { TypeSafeMatCellDef } from '../../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../../type-safe-mat-row-def.directive';
import { StatDefCollection } from '../../contracts/stat-def';

@Component({
    selector: 'app-box-score-fielders',
    standalone: true,
    imports: [
        RouterModule,
        MatTableModule,
        TypeSafeMatCellDef,
        TypeSafeMatRowDef,
        MatSortModule
    ],
    templateUrl: './box-score-fielders.component.html',
    styleUrl: './box-score-fielders.component.scss'
})
export class BoxScoreFieldersComponent implements AfterViewInit {

    @ViewChild(MatSort) sort!: MatSort;
    @ViewChild(MatTable) table!: MatTable<GameFielder>;

    @Input({ required: true })
    dataSource!: GameFielder[]
    @Input({ required: true })
    stats!: StatDefCollection
    displayedColumns = [
        'name',
        'Games',
        'Putouts',
        'Assists',
        'Errors',
        'ErrorsThrowing',
        'ErrorsFielding',
        'StolenBaseAttempts',
        'CaughtStealing'
    ]

    get statNames(): string[] {
        return Object.keys(this.stats);
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
}
