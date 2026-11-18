using Microsoft.EntityFrameworkCore;
using ExpressionEntity = LinguaContext.Expression.Expression;
using WorkEntity = LinguaContext.Work.Work;

namespace LinguaContext.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<WorkEntity> Works => Set<WorkEntity>();
    public DbSet<ExpressionEntity> Expressions => Set<ExpressionEntity>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}