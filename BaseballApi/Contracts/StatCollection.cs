using System;
using System.Reflection;

namespace BaseballApi.Contracts;

public class StatCollection
{
    public static readonly StatCollection Instance = new();
    public readonly IReadOnlyDictionary<string, Stat> Stats;

    private StatCollection()
    {
        var statProps = typeof(Stat).GetFields(BindingFlags.Static | BindingFlags.Public);
        var stats = new Dictionary<string, Stat>();
        foreach (var statProp in statProps)
        {
            if (statProp.FieldType == typeof(Stat))
            {
                var stat = statProp.GetValue(null);
                if (stat != null)
                {
                    var statVal = (Stat)stat;
                    stats[statVal.Name] = statVal;
                }
            }
        }
        Stats = stats;
    }
}
