import { AfterViewInit, Component, Input, ViewChild } from '@angular/core';
import { GameFielder } from '../contracts/game-fielder';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTable, MatTableModule } from '@angular/material/table';
import { RouterModule } from '@angular/router';
import { TypeSafeMatCellDef } from '../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../type-safe-mat-row-def.directive';

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
    displayedColumns = [
        'name',
        'games',
        'putouts',
        'assists',
        'errors',
        'errorsThrowing',
        'errorsFielding',
        'stolenBaseAttempts',
        'caughtStealing'
    ]

    ngAfterViewInit(): void {
        this.sort.sortChange.subscribe(() => {
            const basicSort = this.sort.active as (keyof GameFielder);
            if (basicSort && basicSort != 'player') {
                this.dataSource.sort((a, b) => a[basicSort] - b[basicSort]);
                if (this.sort.direction === 'desc') {
                    this.dataSource.reverse();
                }
                this.table.renderRows();
            }
        });
    }
    basicColumns: (keyof GameFielder)[] = [
        'games',
        'errors',
        'errorsThrowing',
        'errorsFielding',
        'putouts',
        'assists',
        'stolenBaseAttempts',
        'caughtStealing',
        'doublePlays',
        'triplePlays',
        'passedBalls',
        'pickoffFailed',
        'pickoffSuccess',
    ];

    basicHeaders: { [key in keyof GameFielder]: string } = {
        player: '',
        number: '',
        games: 'G',
        errors: 'E',
        errorsThrowing: 'Throwing E',
        errorsFielding: 'Fielding E',
        putouts: 'PO',
        assists: 'A',
        stolenBaseAttempts: 'SBA',
        caughtStealing: 'CS',
        doublePlays: 'DP',
        triplePlays: 'TP',
        passedBalls: 'PB',
        pickoffFailed: 'Pickoff Failed',
        pickoffSuccess: 'Pickoff Success'
    };
}
