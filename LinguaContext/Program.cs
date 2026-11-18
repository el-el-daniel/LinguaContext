using LinguaContext.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using LinguaContext.Expression;
using LinguaContext.Work;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention();
});

builder.Services.AddScoped<WorkService>();
builder.Services.AddScoped<ExpressionService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter<WorkType>(allowIntegerValues: false));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapWorkEndpoints();

app.Run();