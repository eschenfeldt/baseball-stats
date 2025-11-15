using System;
using BaseballApi.Models;

namespace BaseballApi.Integrations;

public class ReferenceManager(BaseballContext context, IMLBAMConnector mlbamConnector)
{
    private IMLBAMConnector MLBAMConnector { get; } = mlbamConnector;
    private BaseballContext Context { get; } = context;

    public async Task<int> UpdateTeamReferences(CancellationToken cancellation)
    {
        // get teams from MLBAM,
        // set MLBAMId on teams in our database
        // Return number of teams updated
        var mlbamTeamsResponse = await MLBAMConnector.GetTeamsAsync(cancellation);
        var updatedCount = 0;
        foreach (var mlbamTeam in mlbamTeamsResponse.Teams)
        {
            var team = Context.Teams.FirstOrDefault(t => t.MLBAMId == mlbamTeam.Id);
            if (team == null)
            {
                team = Context.Teams.FirstOrDefault(t => t.Name == mlbamTeam.TeamName && t.City == mlbamTeam.FranchiseName);
                if (team != null)
                {
                    team.MLBAMId = mlbamTeam.Id;
                    updatedCount++;
                }
            }
        }
        await Context.SaveChangesAsync(cancellation);
        return updatedCount;
    }

    public async Task<PlayerReferenceUpdateResult> UpdatePlayerReferences(DateOnly updatedSince, CancellationToken cancellation)
    {
        // get players from MLBAM updated since the given date
        // update or create ReferencePlayer entries as needed
        // Return number of players updated/created

        // when we have more integrations they can also be handled here
        // unless they require too many api calls
        throw new NotImplementedException();
    }

}
