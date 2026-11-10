using LinguaContext.Infrastructure;
using LinguaContext.Work;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using WorkEntity = LinguaContext.Work.Work;

namespace LinguaContext.Tests.Integration;

public class WorkPersistenceTests
{
    [Fact]
    public async Task Work_CanBeCreatedLoadedAndRenamed()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:17")
            .Build();

        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .LogTo(
                Console.WriteLine,
                new[] { DbLoggerCategory.Database.Command.Name },
                LogLevel.Information)
            .EnableDetailedErrors()
            .Options;

        int workId;

        // Первый DbContext: создание
        await using (var db = new AppDbContext(options))
        {
            await db.Database.MigrateAsync();

            var work = new WorkEntity(
                "El principito",
                WorkType.Book);

            db.Works.Add(work);

            await db.SaveChangesAsync();

            Assert.True(work.Id > 0);

            workId = work.Id;
        }

        // Второй DbContext: загрузка и изменение
        await using (var db = new AppDbContext(options))
        {
            var work = await db.Works.SingleAsync(
                w => w.Id == workId);

            Assert.Equal("El principito", work.Name);
            Assert.Equal(WorkType.Book, work.Type);

            work.Rename("El Principito");

            await db.SaveChangesAsync();
        }

        // Третий DbContext: проверка сохранённого изменения
        await using (var db = new AppDbContext(options))
        {
            var work = await db.Works
                .AsNoTracking()
                .SingleAsync(w => w.Id == workId);

            Assert.Equal("El Principito", work.Name);
        }
    }
}