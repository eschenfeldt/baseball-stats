namespace BaseballApi.Models;

public class Park
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public ICollection<AlternateParkName> AlternateParkNames { get; } = [];
}
