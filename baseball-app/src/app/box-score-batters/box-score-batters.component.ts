import { Component, Input, ViewChild } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { GameBatter } from '../contracts/game-batter';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { TypeSafeMatCellDef } from '../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../type-safe-mat-row-def.directive';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'app-box-score-batters',
    standalone: true,
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
export class BoxScoreBattersComponent {

    @ViewChild(MatSort) sort!: MatSort;

    @Input({ required: true })
    dataSource!: GameBatter[]
    displayedColumns: string[] = [
        'name',
        'games',
        'atBats',
        'hits',
    ];

}
