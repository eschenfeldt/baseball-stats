import { BaseballDataSource } from "../baseball-data-source";
import { PagedApiParameters } from "../paged-api-parameters";
import { Team } from "../contracts/team";

export interface TeamSummary {
    team: Team;
    lastGameDate: string;
    games: number;
    wins: number;
    losses: number;
    parks: number;
}

interface TeamSummaryParameters extends PagedApiParameters {

}

export class TeamsDataSource extends BaseballDataSource<TeamSummaryParameters, TeamSummary> {

    protected override getParameters(): TeamSummaryParameters {
        return {}
    }
    protected override isInfiniteScrollEnabled = true;
}