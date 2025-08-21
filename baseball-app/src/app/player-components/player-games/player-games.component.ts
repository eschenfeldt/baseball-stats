import { Component, EventEmitter, Input, OnChanges, Output, signal, ViewChild } from '@angular/core';
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
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatExpansionModule } from '@angular/material/expansion';
import { SortPipe } from '../../sort.pipe';
import { ListFiltersComponent } from '../../util-components/list-filters/list-filters.component';

enum ColumnGroup {
    general = 'general',
    placeholder = 'placeholder',
    pitching = 'pitching',
    batting = 'batting',
    fielding = 'fielding'
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
        MatButtonToggleModule,
        MatExpansionModule,
        CommonModule,
        RouterModule,
        StatPipe,
        SortPipe,
        ListFiltersComponent
    ],
    templateUrl: './player-games.component.html',
    styleUrl: './player-games.component.scss'
})
export class PlayerGamesComponent extends BaseballTableComponent<PlayerGamesParameters, PlayerGame> implements OnChanges {

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
    displayedColumnGroups: ColumnGroup[] = [ColumnGroup.general];
    optionalColumnGroupSelection: ColumnGroup[] = [ColumnGroup.pitching, ColumnGroup.batting];

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
                this.updateColumns();
            });
    }

    public override ngOnInit(): void {
        this.initialize();
    }

    public ngOnChanges(): void {
        if (this.playerId) {
            this.initialize();
        }
    }

    private initialize(): void {
        this.filterService.setFilterValue<PlayerGamesParameters>(this.uniqueIdentifier, 'playerId', this.playerId);
        this.yearOptions$ = this.api.makeApiGet<number[]>('player/years', { playerId: this.playerId }, false, false);
        this.uniqueIdentifierSet.emit(this.uniqueIdentifier);
    }

    public getStat(playerGame: PlayerGame, statName: string, statGroup: ColumnGroup): number | null {
        if (statGroup === ColumnGroup.batting
            && playerGame.batter
            && playerGame.batter.stats[statName] != null) {
            return playerGame.batter.stats[statName];
        } else if (statGroup === ColumnGroup.pitching
            && playerGame.pitcher
            && playerGame.pitcher.stats[statName] != null) {
            return playerGame.pitcher.stats[statName];
        } else if (statGroup === ColumnGroup.fielding
            && playerGame.fielder
            && playerGame.fielder.stats[statName] != null) {
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

    private get shouldShowPitching(): boolean {
        return this.dataSource.hasPitchingStats && this.optionalColumnGroupSelection.includes(ColumnGroup.pitching);
    }
    private get shouldShowHitting(): boolean {
        return this.dataSource.hasBattingStats && this.optionalColumnGroupSelection.includes(ColumnGroup.batting);
    }

    private updateColumns(): void {
        this.getDisplayedColumnGroups();
        this.getDisplayedColumns();
    }

    private getDisplayedColumnGroups(): void {
        const colGroups = [ColumnGroup.general, ColumnGroup.placeholder];
        if (this.shouldShowPitching) {
            colGroups.push(ColumnGroup.pitching);
        }
        if (this.shouldShowHitting) {
            colGroups.push(ColumnGroup.batting);
        }
        this.displayedColumnGroups = colGroups;
    }

    private getDisplayedColumns(): void {
        const cols = [
            'date',
            'awayTeam',
            'homeTeam'
        ];
        if (this.shouldShowPitching) {
            cols.push('inningsPitched');
            cols.push(...this.pitchingStatNames.map(n => `pitching_${n}`));
        }
        if (this.shouldShowHitting) {
            cols.push(...this.battingStatNames.map(n => `batting_${n}`));
        }
        this.displayedColumns = cols;
    }

    public get ColumnGroup() {
        return ColumnGroup;
    }

    public groupHidden(group: ColumnGroup): boolean {
        return !this.optionalColumnGroupSelection.includes(group);
    }

    public setOptionalColumnGroups(optionalGroups: ColumnGroup[]): void {
        this.optionalColumnGroupSelection = optionalGroups;
        this.updateColumns();
    }

    public hideGroup(group: ColumnGroup) {
        this.optionalColumnGroupSelection = this.optionalColumnGroupSelection.filter(g => g !== group);
        this.updateColumns();
    }

    readonly filterOpenState = signal(false);

    public get filterSummary(): string {
        if (this.filterOpenState()) {
            return '';
        } else if (this.selectedYear) {
            return `Year: ${this.selectedYear}`;
        } else {
            return '';
        }
    }

    public gameDate(game: PlayerGame): string {
        return Utils.formatDate(game.game.date);
    }
}

