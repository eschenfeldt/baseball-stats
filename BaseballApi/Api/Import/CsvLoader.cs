using BaseballApi.Models;

namespace BaseballApi.Import;

public class CsvLoader
{
    string FilePath { get; }
    public bool IsLoaded { get; private set; } = false;
    public List<string>? Headers { get; private set; }
    public Dictionary<string, int> HeaderIndices { get; } = [];
    public List<List<string>> Rows { get; } = [];

    public CsvLoader(string filePath)
    {
        this.FilePath = filePath;
    }

    public IEnumerable<Batter> GetBatters(BoxScore boxScore)
    {
        if (!this.IsLoaded)
        {
            this.LoadData();
        }
        foreach (var rawRow in this.Rows)
        {
            var name = rawRow[HeaderIndices["Name"]];
            if (!name.Equals("TOTALS", StringComparison.OrdinalIgnoreCase))
            {
                yield return new Batter
                {
                    BoxScore = boxScore,
                    Player = new Player
                    {
                        Name = name,
                    },
                    Number = this.GetInt(rawRow, "#"),
                    Games = this.GetInt(rawRow, "G"),
                    PlateAppearances = this.GetInt(rawRow, "PA"),
                    AtBats = this.GetInt(rawRow, "AB"),
                    Runs = this.GetInt(rawRow, "R"),
                    Hits = this.GetInt(rawRow, "H"),
                    BuntSingles = this.GetInt(rawRow, "B"),
                    Singles = this.GetInt(rawRow, "1B"),
                    Doubles = this.GetInt(rawRow, "2B"),
                    Triples = this.GetInt(rawRow, "3B"),
                    Homeruns = this.GetInt(rawRow, "HR"),
                    RunsBattedIn = this.GetInt(rawRow, "RBI"),
                    Walks = this.GetInt(rawRow, "BB"),
                    Strikeouts = this.GetInt(rawRow, "SO"),
                    StrikeoutsCalled = this.GetInt(rawRow, "Kc"),
                    StrikeoutsSwinging = this.GetInt(rawRow, "Ks"),
                    HitByPitch = this.GetInt(rawRow, "HBP"),
                    StolenBases = this.GetInt(rawRow, "SB"),
                    CaughtStealing = this.GetInt(rawRow, "CS"),
                    SacrificeBunts = this.GetInt(rawRow, "SCB"),
                    SacrificeFlies = this.GetInt(rawRow, "SF"),
                    Sacrifices = this.GetInt(rawRow, "SAC"),
                    ReachedOnError = this.GetInt(rawRow, "ROE"),
                    FieldersChoices = this.GetInt(rawRow, "FC"),
                    CatchersInterference = this.GetInt(rawRow, "CI"),
                    GroundedIntoDoublePlay = this.GetInt(rawRow, "GDP"),
                    GroundedIntoTriplePlay = this.GetInt(rawRow, "GTP"),
                    AtBatsWithRunnersInScoringPosition = this.GetInt(rawRow, "AB/RSP"),
                    HitsWithRunnersInScoringPosition = this.GetInt(rawRow, "H/RSP")
                };
            }
        }
    }

    public IEnumerable<Pitcher> GetPitchers(BoxScore boxScore)
    {
        if (!this.IsLoaded)
        {
            this.LoadData();
        }
        foreach (var rawRow in this.Rows)
        {
            var name = rawRow[HeaderIndices["Name"]];
            if (!name.Equals("TOTALS", StringComparison.OrdinalIgnoreCase))
            {
                yield return new Pitcher
                {
                    BoxScore = boxScore,
                    Player = new Player
                    {
                        Name = name,
                    },
                    Number = this.GetInt(rawRow, "#"),
                    Games = this.GetInt(rawRow, "G"),
                    Wins = GetInt(rawRow, "W"),
                    Losses = GetInt(rawRow, "L"),
                    Saves = GetInt(rawRow, "SV"),
                    ThirdInningsPitched = GetThirdInningsPitched(rawRow),
                    BattersFaced = GetInt(rawRow, "BF"),
                    Balls = GetInt(rawRow, "Ball"),
                    Strikes = GetInt(rawRow, "Str"),
                    Pitches = GetInt(rawRow, "PIT"),
                    Runs = GetInt(rawRow, "R"),
                    EarnedRuns = GetInt(rawRow, "ER"),
                    Hits = GetInt(rawRow, "H"),
                    Walks = GetInt(rawRow, "BB"),
                    IntentionalWalks = GetInt(rawRow, "IBB"),
                    Strikeouts = GetInt(rawRow, "K"),
                    StrikeoutsCalled = GetInt(rawRow, "Kc"),
                    StrikeoutsSwinging = GetInt(rawRow, "Ks"),
                    HitByPitch = GetInt(rawRow, "HB"),
                    Balks = GetInt(rawRow, "BK"),
                    WildPitches = GetInt(rawRow, "WP"),
                    Homeruns = GetInt(rawRow, "HR"),
                    GroundOuts = GetInt(rawRow, "GO"),
                    AirOuts = GetInt(rawRow, "AO"),
                    FirstPitchStrikes = GetInt(rawRow, "FPS"),
                    FirstPitchBalls = GetInt(rawRow, "FPB")
                };
            }
        }
    }

    public IEnumerable<Fielder> GetFielders(BoxScore boxScore)
    {
        if (!this.IsLoaded)
        {
            this.LoadData();
        }
        foreach (var rawRow in this.Rows)
        {
            var name = rawRow[HeaderIndices["Name"]];
            if (!name.Equals("TOTALS", StringComparison.OrdinalIgnoreCase))
            {
                yield return new Fielder
                {
                    BoxScore = boxScore,
                    Player = new Player
                    {
                        Name = name,
                    },
                    Number = GetInt(rawRow, "#"),
                    Games = GetInt(rawRow, "G"),
                    Errors = GetInt(rawRow, "ERR"),
                    ErrorsThrowing = GetInt(rawRow, "Et"),
                    ErrorsFielding = GetInt(rawRow, "Ef"),
                    Putouts = GetInt(rawRow, "PO"),
                    Assists = GetInt(rawRow, "A"),
                    StolenBaseAttempts = GetInt(rawRow, "SBA"),
                    CaughtStealing = GetInt(rawRow, "CS"),
                    DoublePlays = GetInt(rawRow, "DP"),
                    TriplePlays = GetInt(rawRow, "TP"),
                    PassedBalls = GetInt(rawRow, "PB"),
                    PickoffFailed = GetInt(rawRow, "PKF"),
                    PickoffSuccess = GetInt(rawRow, "PK")
                };
            }
        }
    }

    public IEnumerable<FangraphsConstants> GetFangraphsConstants()
    {
        if (!this.IsLoaded)
        {
            this.LoadData();
        }
        foreach (var rawRow in this.Rows)
        {
            yield return new FangraphsConstants
            {

                Year = GetInt(rawRow, "Season"),
                WOBA = GetDecimal(rawRow, "wOBA"),
                WOBAScale = GetDecimal(rawRow, "wOBAScale"),
                WBB = GetDecimal(rawRow, "wBB"),
                WHBP = GetDecimal(rawRow, "wHBP"),
                W1B = GetDecimal(rawRow, "w1B"),
                W2B = GetDecimal(rawRow, "w2B"),
                W3B = GetDecimal(rawRow, "w3B"),
                WHR = GetDecimal(rawRow, "wHR"),
                RunSB = GetDecimal(rawRow, "runSB"),
                RunCS = GetDecimal(rawRow, "runCS"),
                RPA = GetDecimal(rawRow, "R/PA"),
                RW = GetDecimal(rawRow, "R/W"),
                CFIP = GetDecimal(rawRow, "cFIP")
            };
        }
    }

    private int GetInt(List<string> row, string colName)
    {
        string rawVal = row[HeaderIndices[colName]];
        if (string.IsNullOrEmpty(rawVal))
        {
            return 0;
        }
        else
        {
            return Convert.ToInt32(rawVal);
        }
    }
    private decimal GetDecimal(List<string> row, string colName)
    {
        string rawVal = row[HeaderIndices[colName]];
        if (string.IsNullOrEmpty(rawVal))
        {
            return 0;
        }
        else
        {
            return Convert.ToDecimal(rawVal);
        }
    }

    private int GetThirdInningsPitched(List<string> row)
    {
        string rawVal = row[HeaderIndices["IP"]];
        decimal ip = Convert.ToDecimal(rawVal);
        return Convert.ToInt32(Math.Round(ip * 3, 0));
    }

    public void LoadData()
    {
        int lineNum = 0;
        foreach (string line in File.ReadAllLines(this.FilePath))
        {
            List<string> cells = line.Split(',', StringSplitOptions.TrimEntries)
                                    .Select(h => h.Trim('"')).ToList();
            if (lineNum == 0)
            {
                this.Headers = cells;
                int i = 0;
                foreach (string header in this.Headers)
                {
                    this.HeaderIndices[header] = i;
                    i++;
                }
            }
            else
            {
                this.Rows.Add(cells);
            }
            lineNum++;
        }
        this.IsLoaded = true;
    }
}
