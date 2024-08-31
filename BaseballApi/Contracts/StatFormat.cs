namespace BaseballApi.Contracts;

public interface IStatFormat
{
    public string Name { get; }
}

public record Integer : IStatFormat
{
    public static readonly Integer Singleton = new("Integer");
    public string Name { get; }
    private Integer(string name)
    {
        Name = name;
    }
}
public record Decimal(int DecimalPoints) : IStatFormat
{
    public string Name { get; } = "Decimal";
    public int DecimalPoints { get; } = DecimalPoints;
}
