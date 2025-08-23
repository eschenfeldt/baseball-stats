import { Component, Input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { StatPipe } from '../../stat.pipe';
import { Utils } from '../../utils';
import { SummaryStat } from '../../contracts/summary-stat';
import { StatCategory } from '../../contracts/stat-category';
import { ActivatedRoute, Params, RouterModule } from '@angular/router';
import { BASEBALL_ROUTES } from '../../app.routes';

@Component({
    selector: 'app-summary-stats',
    imports: [
        MatCardModule,
        StatPipe,
        RouterModule
    ],
    templateUrl: './summary-stats.component.html',
    styleUrl: './summary-stats.component.scss'
})
export class SummaryStatsComponent {

    @Input({ required: true })
    summaryStats!: SummaryStat[]

    @Input({ required: true })
    category!: StatCategory

    @Input()
    hideGames?: boolean

    @Input()
    hideNull?: boolean

    public get categoryLabel(): string {
        switch (this.category) {
            case StatCategory.batting:
                return 'Batting';
            case StatCategory.pitching:
                return 'Pitching'
            case StatCategory.fielding:
                return 'Fielding'
            case StatCategory.general:
                return 'Overall'
            default:
                return '';
        }
    }

    public get stats() {
        return this.summaryStats.filter(s => s.category === this.category && !(this.hideGames && s.definition.name === 'Games'));
    }
    public get showIP(): boolean {
        return this.category === StatCategory.pitching && this.fullInningsPitched != null;
    }
    public get fullInningsPitched(): string | null {
        return Utils.fullSummaryInningsPitched(this.summaryStats);
    }
    public get partialInningsPitched(): string | null {
        return Utils.partialSummaryInningsPitched(this.summaryStats);
    }

    /**Convert all current path and query parameters into a query parameter array used to filter a target list*/
    get allParams(): Params {
        const params: Params = {}
        // query parameters first so path parameters take priority if we have an overlap
        this.route.snapshot.queryParamMap.keys.forEach(k => {
            params[k] = this.route.snapshot.queryParamMap.get(k)
        })
        this.route.snapshot.paramMap.keys.forEach(k => {
            params[k] = this.route.snapshot.paramMap.get(k)
        })
        return params;
    }

    public constructor(private route: ActivatedRoute) { }

    public statRouterLink(stat: SummaryStat): string[] | null {
        if (this.category !== StatCategory.general) {
            return null;
        } else {
            switch (stat.definition.name) {
                case 'Parks':
                    return ['/', BASEBALL_ROUTES.PARKS]
                case 'Games':
                    return ['/', BASEBALL_ROUTES.GAMES]
                case 'Teams':
                    return ['/', BASEBALL_ROUTES.TEAMS]
                case 'Players':
                    return ['/', BASEBALL_ROUTES.LEADERS]
                default:
                    return null;
            }
        }
    }

}
