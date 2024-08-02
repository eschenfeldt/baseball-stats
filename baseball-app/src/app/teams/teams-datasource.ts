import { BaseballDataSource } from "../baseball-data-source";
import { PagedApiParameters } from "../paged-api-parameters";
import { Team } from "../team";

export interface TeamSummary {
    team: Team;
    games: number;
    wins: number;
    losses: number;
}

export class TeamsDataSource extends BaseballDataSource<PagedApiParameters, TeamSummary> {

    protected override getParameters(): PagedApiParameters {
        return {}
    }

}