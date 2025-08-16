import { BaseballDataSource } from "../baseball-data-source";
import { PagedApiParameters } from "../paged-api-parameters";
import { Park } from '../contracts/park';

export interface ParkSummary {
    park: Park;
    firstGameDate: string;
    lastGameDate: string;
    games: number;
    wins: number;
    losses: number;
    teams: number;
}

export interface ParkSummaryParameters extends PagedApiParameters {
    teamId?: number | null;
}

export class ParksDataSource extends BaseballDataSource<ParkSummaryParameters, ParkSummary> {

    protected override getParameters(): ParkSummaryParameters {
        return {}
    }
    protected override isInfiniteScrollEnabled = true;
}