using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using BaseballApi.Contracts;
using BaseballApi.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;

namespace BaseballApi.Controllers;

internal class StatCalculator
{
    private BaseballContext Context { get; }

    public int? Year { get; set; }
    public long? PlayerId { get; set; }
    public long? TeamId { get; set; }

    public bool GroupByPlayer { get; set; } = true;
    public bool GroupByYear { get; set; } = false;
    public bool GroupByTeam { get; set; } = false;

    public string? PlayerSearch { get; set; }

    public int MinPlateAppearances { get; set; }

    public string OrderBy { get; set; } = Stat.Games.Name;
    public bool OrderAscending { get; set; } = false;

    internal StatCalculator(BaseballContext context)
    {
        Context = context;
    }

    private static readonly Dictionary<string, Expression<Func<BattingStat, decimal?>>> BattingStatSelectors = new() {
        {Stat.Games.Name, b => b.Games},
        {Stat.PlateAppearances.Name, b => b.PlateAppearances},
        {Stat.BattingAverage.Name, b => b.AVG},
        {Stat.OnBasePercentage.Name, b => b.OBP},
        {Stat.WeightedOnBaseAverage.Name, b => b.WOBA }
    };

    public static Dictionary<string, Stat> GetBattingStats()
    {
        return StatCollection.Instance.Stats
            .Where(kvp => BattingStatSelectors.ContainsKey(kvp.Key))
            .ToDictionary();
    }

    internal record BattingStat
    {
        public int? Year { get; set; }
        public long? PlayerId { get; set; }
        public long? TeamId { get; set; }
        public int Games { get; set; }
        public int PlateAppearances { get; set; }
        public decimal? WOBA { get; set; }
        public decimal? OBP { get; set; }
        public decimal? AVG { get; set; }
    }

    internal IQueryable<BattingStat> GetBattingStats(BatterLeaderboardOrder order = BatterLeaderboardOrder.Games)
    {
        string? yearCol = GroupByYear ? "c.\"Year\"" : null;
        string? teamCol = GroupByTeam ? "bs.\"TeamId\"" : null;
        string? playerCol = GroupByPlayer ? "b.\"PlayerId\"" : null;

        string baseQuery = @$"
            SELECT
                {yearCol ?? "NULL"} AS ""Year"",
                {teamCol ?? "NULL"} AS ""TeamId"",
                {playerCol ?? "NULL"} AS ""PlayerId"",
                SUM(b.""Games"") AS ""Games"",
                SUM(b.""PlateAppearances"") AS ""PlateAppearances"",
                SUM(b.""Hits"" + b.""Walks"" + b.""HitByPitch"")::decimal
                    / NULLIF(SUM(b.""AtBats"" + b.""Walks"" + b.""HitByPitch"" + b.""SacrificeFlies""), 0) AS ""OBP"",
                SUM(c.""WBB"" * b.""Walks"" + c.""WHBP"" * b.""HitByPitch"" + c.""W1B"" * b.""Singles""
                    + c.""W2B"" * b.""Doubles"" + c.""W3B"" * b.""Triples"" + c.""WHR"" * b.""Homeruns"")
                    / NULLIF(SUM(b.""AtBats"" + b.""Walks"" + b.""SacrificeFlies"" + b.""HitByPitch""), 0) AS ""WOBA"",
                SUM(b.""Hits"")::decimal / NULLIF(SUM(b.""AtBats""), 0) AS ""AVG""
            FROM ""Batters"" b
            JOIN ""Players"" p ON b.""PlayerId"" = p.""Id""
            JOIN ""BoxScores"" bs ON b.""BoxScoreId"" = bs.""Id""
            JOIN ""Games"" g ON bs.""GameId"" = g.""Id""
            JOIN ""Constants"" c ON DATE_PART('Year', g.""Date"") = c.""Year""
            ";

        string where = "WHERE 1=1";
        List<NpgsqlParameter> parameters = [
            new NpgsqlParameter("minPlateAppearances", MinPlateAppearances)
        ];
        if (PlayerId.HasValue)
        {
            where += " AND b.\"PlayerId\" = @playerId";
            parameters.Add(new NpgsqlParameter("playerId", PlayerId));
        }
        if (Year.HasValue)
        {
            where += " AND c.\"Year\" = @year";
            parameters.Add(new NpgsqlParameter("year", Year));
        }
        if (TeamId.HasValue)
        {
            where += " AND bs.\"TeamId\" = @teamId";
            parameters.Add(new NpgsqlParameter("teamId", TeamId));
        }
        if (!string.IsNullOrEmpty(PlayerSearch))
        {
            where += " AND p.\"Name\" LIKE CONCAT('%', @playerSearch, '%')";
            parameters.Add(new NpgsqlParameter("playerSearch", PlayerSearch));
        }

        List<string> groupByCols = [
            yearCol,
            teamCol,
            playerCol
        ];
        string groupBy = $"GROUP BY {string.Join(", ", groupByCols.Where(c => !string.IsNullOrEmpty(c)))}";

        var query = @$"
        {baseQuery}
        {where}
        {groupBy}
        HAVING SUM(b.""PlateAppearances"") > @minPlateAppearances
        {GetBattingOrderBy()}
        ";

#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
        return Context.Database.SqlQueryRaw<BattingStat>(query, parameters.ToArray());
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.
    }

    private string GetBattingOrderBy()
    {
        if (!BattingStatSelectors.TryGetValue(OrderBy, out Expression<Func<BattingStat, decimal?>>? selector))
        {
            throw new ArgumentException($"Stat {OrderBy} is not configured for batting leaders");
        }
        var name = selector.GetMemberName();
        var order = OrderAscending ? "ASC" : "DESC";
        return $"ORDER BY \"{name}\" {order}";
    }
}
