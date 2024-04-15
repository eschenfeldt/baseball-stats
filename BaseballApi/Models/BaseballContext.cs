using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Models;

public class BaseballContext : DbContext
{
    public BaseballContext(DbContextOptions<BaseballContext> options) : base(options) {}

    public DbSet<Game> Games { get; set; } = null!;
}