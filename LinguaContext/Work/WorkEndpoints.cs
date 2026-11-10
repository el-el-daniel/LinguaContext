namespace LinguaContext.Work;

using Microsoft.AspNetCore.Http.HttpResults;

public static class WorkEndpoints
{
    private const string GetByIdRouteName = "GetWorkById";
    
    private static async Task<
        Results<
            CreatedAtRoute<WorkResponse>,
            ValidationProblem>> CreateWorkAsync(
        CreateWorkRequest createWorkRequest,
        WorkService workService,
        CancellationToken cancellationToken)
    {
        try
        {
            var work = await workService.CreateAsync(createWorkRequest, cancellationToken);
            return TypedResults.CreatedAtRoute(work, GetByIdRouteName, new { id = work.Id });
        }
        catch (ArgumentException e)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>()
                {
                    [e.ParamName ?? "general"] = [e.Message]
                });
        }
    }
    
    public static RouteGroupBuilder MapWorkEndpoints(this IEndpointRouteBuilder app)
    {
        var workGroup = app.MapGroup("/works");

        workGroup.MapGet("/{id:int}", async (
                int id, 
                WorkService workService, 
                CancellationToken cancellationToken) =>
            {
                var work = await workService.GetByIdAsync(id, cancellationToken);
                return work is null
                    ? Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Work not found",
                        detail: $"Work with id {id} was not found")
                    : Results.Ok(work);
            })
            .WithName(GetByIdRouteName);
        
        workGroup.MapPost("", CreateWorkAsync);
        
        return workGroup;
    }
}