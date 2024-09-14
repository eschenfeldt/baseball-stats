using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using BaseballApi.Contracts;
using BaseballApi.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    public int MinInningsPitched { get; set; }

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
        {Stat.SluggingPercentage.Name, b => b.SLG},
        {Stat.OnBasePlusSlugging.Name, b => b.OPS},
        {Stat.WeightedOnBaseAverage.Name, b => b.WOBA }
    };
    private static readonly Dictionary<string, Func<BattingStat, decimal?>> CompiledBattingStatSelectors = BattingStatSelectors.ToDictionary(
        kvp => kvp.Key,
        kvp => kvp.Value.Compile()
    );

    public static Dictionary<string, Stat> GetBattingStatDefs()
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
        public decimal? OPS { get; set; }
        public decimal? SLG { get; set; }
        public decimal? OBP { get; set; }
        public decimal? AVG { get; set; }

        public Dictionary<string, decimal?> ToDictionary()
        {
            return CompiledBattingStatSelectors.ToDictionary(sel => sel.Key, sel => sel.Value(this));
        }
    }

    internal IQueryable<BattingStat> GetBattingStats()
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
                SUM(b.""Hits"")::decimal / NULLIF(SUM(b.""AtBats""), 0) AS ""AVG"",
                SUM(b.""Singles"" + 2 * b.""Doubles"" + 3 * b.""Triples"" + 4 * b.""Homeruns"")::decimal 
                    / NULLIF(SUM(b.""AtBats""), 0) AS ""SLG"",
                SUM(b.""Hits"" + b.""Walks"" + b.""HitByPitch"")::decimal
                    / NULLIF(SUM(b.""AtBats"" + b.""Walks"" + b.""HitByPitch"" + b.""SacrificeFlies""), 0)
                + SUM(b.""Singles"" + 2 * b.""Doubles"" + 3 * b.""Triples"" + 4 * b.""Homeruns"")::decimal 
                    / NULLIF(SUM(b.""AtBats""), 0) AS ""OPS""
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
            where += " AND p.\"Name\" ILIKE CONCAT('%', @playerSearch, '%')";
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
        HAVING SUM(b.""PlateAppearances"") >= @minPlateAppearances
        {GetBattingOrderBy()}
        ";

#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
        return Context.Database.SqlQueryRaw<BattingStat>(query, parameters.ToArray());
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.
    }

    private string GetBattingOrderBy()
    {
        string name;
        if (string.Equals(OrderBy, "year", StringComparison.OrdinalIgnoreCase))
        {
            name = "Year";
        }
        else if (!BattingStatSelectors.TryGetValue(OrderBy, out Expression<Func<BattingStat, decimal?>>? selector))
        {
            throw new ArgumentException($"Stat {OrderBy} is not configured for batting leaders");
        }
        else
        {
            name = selector.GetMemberName();
        }
        var order = OrderAscending ? "ASC" : "DESC";
        return $"ORDER BY \"{name}\" {order}";
    }

    internal record PitchingStat
    {
        public int? Year { get; set; }
        public long? PlayerId { get; set; }
        public long? TeamId { get; set; }
        public int Games { get; set; }
        public int ThirdInningsPitched { get; set; }
        public decimal? ERA { get; set; }
        public decimal? FIP { get; set; }
        public decimal? HR { get; set; }
        public decimal? KRate { get; set; }
        public decimal? BBRate { get; set; }

        public Dictionary<string, decimal?> ToDictionary()
        {
            return CompiledPitchingStatSelectors.ToDictionary(sel => sel.Key, sel => sel.Value(this));
        }
    }

    private static readonly Dictionary<string, Expression<Func<PitchingStat, decimal?>>> PitchingStatSelectors = new() {
        {Stat.Games.Name, p => p.Games},
        {Stat.ThirdInningsPitched.Name, p => p.ThirdInningsPitched},
        {Stat.EarnedRunAverage.Name, p => p.ERA},
        {Stat.FieldingIndependentPitching.Name, p => p.FIP},
        {Stat.HomerunsAllowed.Name, p => p.HR},
        {Stat.StrikeoutRate.Name, p => p.KRate},
        {Stat.WalkRate.Name, p => p.BBRate}
    };
    private static readonly Dictionary<string, Func<PitchingStat, decimal?>> CompiledPitchingStatSelectors = PitchingStatSelectors.ToDictionary(
        kvp => kvp.Key,
        kvp => kvp.Value.Compile()
    );

    public static Dictionary<string, Stat> GetPitchingStatDefs()
    {
        return StatCollection.Instance.Stats
            .Where(kvp => PitchingStatSelectors.ContainsKey(kvp.Key))
            .ToDictionary();
    }

    internal IQueryable<PitchingStat> GetPitchingStats()
    {
        string? yearCol = GroupByYear ? "c.\"Year\"" : null;
        string? teamCol = GroupByTeam ? "bs.\"TeamId\"" : null;
        string? playerCol = GroupByPlayer ? "pi.\"PlayerId\"" : null;

        string baseQuery = @$"
            SELECT
                {yearCol ?? "NULL"} AS ""Year"",
                {teamCol ?? "NULL"} AS ""TeamId"",
                {playerCol ?? "NULL"} AS ""PlayerId"",
                SUM(pi.""Games"") AS ""Games"",
                SUM(pi.""Homeruns"") AS ""HR"",
                SUM(pi.""ThirdInningsPitched"") AS ""ThirdInningsPitched"",
                9 * SUM(pi.""EarnedRuns"")::decimal
                    / NULLIF(SUM(pi.""ThirdInningsPitched"") / 3.0, 0) AS ""ERA"",
                SUM(c.""CFIP"" * pi.""ThirdInningsPitched"") / NULLIF(SUM(pi.""ThirdInningsPitched""), 0)
                + SUM(13 * pi.""Homeruns"" + 3 * pi.""HitByPitch"" + 3 * pi.""Walks""
                    - 2 * pi.""Strikeouts"")
                    / NULLIF(SUM(pi.""ThirdInningsPitched"") / 3.0, 0) AS ""FIP"",
                SUM(pi.""Strikeouts"")::decimal / NULLIF(SUM(pi.""BattersFaced""), 0) AS ""KRate"",
                SUM(pi.""Walks"")::decimal / NULLIF(SUM(pi.""BattersFaced""), 0) AS ""BBRate""
            FROM ""Pitchers"" pi
            JOIN ""Players"" p ON pi.""PlayerId"" = p.""Id""
            JOIN ""BoxScores"" bs ON pi.""BoxScoreId"" = bs.""Id""
            JOIN ""Games"" g ON bs.""GameId"" = g.""Id""
            JOIN ""Constants"" c ON DATE_PART('Year', g.""Date"") = c.""Year""
            ";

        string where = "WHERE 1=1";
        List<NpgsqlParameter> parameters = [
            new NpgsqlParameter("minThirdInningsPitched", MinInningsPitched * 3)
        ];
        if (PlayerId.HasValue)
        {
            where += " AND pi.\"PlayerId\" = @playerId";
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
            where += " AND p.\"Name\" ILIKE CONCAT('%', @playerSearch, '%')";
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
        HAVING SUM(pi.""ThirdInningsPitched"") >= @minThirdInningsPitched
        {GetPitchingOrderBy()}
        ";

#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
        return Context.Database.SqlQueryRaw<PitchingStat>(query, parameters.ToArray());
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.
    }

    private string GetPitchingOrderBy()
    {
        if (!PitchingStatSelectors.TryGetValue(OrderBy, out Expression<Func<PitchingStat, decimal?>>? selector))
        {
            throw new ArgumentException($"Stat {OrderBy} is not configured for pitching leaders");
        }
        var name = selector.GetMemberName();
        var order = OrderAscending ? "ASC" : "DESC";
        return $"ORDER BY \"{name}\" {order}";
    }
}
