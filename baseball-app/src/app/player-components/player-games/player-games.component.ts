import { Component, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { BaseballTableComponent } from '../../baseball-table-component';
import { PlayerGamesDataSource, PlayerGamesParameters } from './player-games-datasource';
import { PlayerGame } from '../../contracts/player-game';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { BaseballApiFilter, BaseballFilterService } from '../../baseball-filter.service';
import { BaseballApiService } from '../../baseball-api.service';
import { combineLatest, Observable } from 'rxjs';
import { MatTableModule } from '@angular/material/table';
import { TypeSafeMatCellDef } from '../../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../../type-safe-mat-row-def.directive';
import { AsyncPipe, CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { RouterModule } from '@angular/router';
import { StatDefCollection } from '../../contracts/stat-def';
import { Utils } from '../../utils';
import { StatPipe } from '../../stat.pipe';

enum ColumnGroup {
    general = 'general',
    pitching = 'pitching',
    batting = 'batting'
}

@Component({
    selector: 'app-player-games',
    standalone: true,
    imports: [
        MatTableModule,
        TypeSafeMatCellDef,
        TypeSafeMatRowDef,
        MatPaginatorModule,
        MatSortModule,
        AsyncPipe,
        FormsModule,
        MatInputModule,
        MatFormFieldModule,
        MatSelectModule,
        CommonModule,
        RouterModule,
        StatPipe
    ],
    templateUrl: './player-games.component.html',
    styleUrl: './player-games.component.scss'
})
export class PlayerGamesComponent extends BaseballTableComponent<PlayerGamesParameters, PlayerGame> {

    @Input({ required: true })
    public playerId!: number

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    @Output()
    public uniqueIdentifierSet = new EventEmitter<string>();

    dataSource: PlayerGamesDataSource;
    displayedColumns: string[] = [
        'date',
        'awayTeam',
        'homeTeam',
    ];
    displayedColumnGroups: ColumnGroup[] = PlayerGamesComponent.allColumnGroups;
    private static readonly allColumnGroups = [
        ColumnGroup.general,
        ColumnGroup.pitching,
        ColumnGroup.batting
    ];
    private columnGroupToggles = {
        [ColumnGroup.general]: true,
        [ColumnGroup.pitching]: true,
        [ColumnGroup.batting]: true
    }
    protected override get defaultFilters(): BaseballApiFilter {
        return {};
    }
    override defaultPageSize: number = 5;

    public yearOptions$?: Observable<number[]>;

    public get selectedYear(): number | undefined {
        return this.filterService.getFilterValue<PlayerGamesParameters>(this.uniqueIdentifier, 'year');
    }
    public set selectedYear(value: number) {
        this.filterService.setFilterValue<PlayerGamesParameters>(this.uniqueIdentifier, 'year', value);
    }

    battingStats: StatDefCollection = {};
    pitchingStats: StatDefCollection = {};
    get pitchingStatNames(): string[] {
        return Object.keys(this.pitchingStats).filter(n => n !== 'ThirdInningsPitched');
    }
    get battingStatNames(): string[] {
        return Object.keys(this.battingStats);
    }

    public constructor(
        private api: BaseballApiService,
        private filterService: BaseballFilterService
    ) {
        super();
        this.dataSource = new PlayerGamesDataSource(
            this.api,
            this.filterService
        );
        combineLatest([this.dataSource.battingStats$, this.dataSource.pitchingStats$])
            .subscribe(([battingStats, pitchingStats]) => {
                this.battingStats = battingStats;
                this.pitchingStats = pitchingStats;
                this.getDisplayedColumns();
            });
    }

    public override ngOnInit(): void {
        this.filterService.setFilterValue<PlayerGamesParameters>(this.uniqueIdentifier, 'playerId', this.playerId);
        this.yearOptions$ = this.api.makeApiGet<number[]>('player/years', { playerId: this.playerId }, false, false);
        this.uniqueIdentifierSet.emit(this.uniqueIdentifier);
    }

    public getStat(playerGame: PlayerGame, statName: string, pitcher: boolean): number | null {
        if (!pitcher && playerGame.batter && playerGame.batter.stats[statName] != null) {
            return playerGame.batter.stats[statName];
        } else if (playerGame.pitcher && playerGame.pitcher.stats[statName] != null) {
            return playerGame.pitcher.stats[statName];
        } else if (playerGame.fielder && playerGame.fielder.stats[statName] != null) {
            return playerGame.fielder.stats[statName];
        } else {
            return null;
        }
    }

    public fullInningsPitched(playerGame: PlayerGame): string {
        if (playerGame.pitcher) {
            return Utils.fullInningsPitched(playerGame.pitcher.stats);
        } else {
            return '';
        }
    }
    public partialInningsPitched(playerGame: PlayerGame): string {
        if (playerGame.pitcher) {
            return Utils.partialInningsPitched(playerGame.pitcher.stats);
        } else {
            return '';
        }
    }

    private getDisplayedColumns(): void {
        this.displayedColumns = [
            'date',
            'awayTeam',
            'homeTeam',
            'inningsPitched',
            ...this.pitchingStatNames.map(n => `pitching_${n}`),
            ...this.battingStatNames.map(n => `batting_${n}`)
        ];
    }

    public get ColumnGroup() {
        return ColumnGroup;
    }

    public toggleGroup(group: ColumnGroup) {
        this.columnGroupToggles[group] = !this.columnGroupToggles[group];
        this.displayedColumnGroups = PlayerGamesComponent.allColumnGroups.filter(g => this.columnGroupToggles[g]);
    }
}

