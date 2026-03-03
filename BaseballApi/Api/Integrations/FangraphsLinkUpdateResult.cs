using System;

namespace BaseballApi.Integrations;

public struct FangraphsLinkUpdateResult
{
    public int PlayersUpdated { get; set; }
    public int ReferencePlayersUpdated { get; set; }
}
