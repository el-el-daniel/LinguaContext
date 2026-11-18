namespace LinguaContext.Work;

public record CreateWorkRequest
{
    public required string Name { get; init; }
    public required WorkType Type { get; init; }
}