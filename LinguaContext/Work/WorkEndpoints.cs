using System.Text.Json;
using LinguaContext.Common.Pagination;

namespace LinguaContext.Work;

using Microsoft.AspNetCore.Http.HttpResults;

public static class WorkEndpoints
{
    private const string GetByIdRouteName = "GetWorkById";

    private static ValidationProblem ToValidationProblem(ArgumentException e)
    {
        var field = e.ParamName is { Length: > 0 }
            ? JsonNamingPolicy.CamelCase.ConvertName(e.ParamName)
            : "general";
        
        return TypedResults.ValidationProblem(
            new Dictionary<string, string[]>
            {
                [field] = [e.Message]
            });
    }
    
    private static async Task<
        Results<
            Ok<WorkResponse>,
            NotFound>> GetWorkByIdAsync(
        int id, 
        WorkService workService,
        CancellationToken cancellationToken)
    {
        var work = await workService.GetByIdAsync(id, cancellationToken);
        return work is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(work);
    }
    
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
            return ToValidationProblem(e);
        }
    }
    
    private static async Task<
        Results<
            Ok<PagedResponse<WorkResponse>>,
            ValidationProblem>> GetWorksAsync(
        WorkService workService,
        CancellationToken cancellationToken, 
        int page = 1,
        int pageSize = 20,
        WorkType? type = null,
        string? search = null)
    {
        try
        {
            var works = await workService.GetAllAsync(
                page, pageSize, type, search, cancellationToken);
            return TypedResults.Ok(works);
        }
        catch (ArgumentException e)
        {
            return ToValidationProblem(e);
        }
    }

    private static async Task<
        Results<
            Ok<WorkResponse>,
            ValidationProblem,
            NotFound>> UpdateWorkAsync(
        int id,
        UpdateWorkRequest updateWorkRequest,
        WorkService workService,
        CancellationToken cancellationToken)
    {
        try
        {
            var work = await workService.UpdateAsync(id, updateWorkRequest, cancellationToken);
            if (work is null)
                return TypedResults.NotFound();
            return TypedResults.Ok(work);
        }
        catch (ArgumentException e)
        {
            return ToValidationProblem(e);
        }
    }

    private static async Task<
        Results<
            NoContent,
            NotFound,
            ProblemHttpResult>> DeleteWorkAsync(int id, WorkService workService, CancellationToken cancellationToken)
    {
        var result = await workService.DeleteAsync(id, cancellationToken);
        
        if (result is DeleteWorkError.WorkNotFound)
            return TypedResults.NotFound();
        if (result is DeleteWorkError.ExpressionsFound)
            return TypedResults.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Work cannot be deleted",
                detail: "This work has associated expressions.");
        return TypedResults.NoContent();
    }
    
    public static RouteGroupBuilder MapWorkEndpoints(this IEndpointRouteBuilder app)
    {
        var workGroup = app.MapGroup("/works");

        workGroup.MapGet("/{id:int}", GetWorkByIdAsync)
            .WithName(GetByIdRouteName);
        
        workGroup.MapGet("", GetWorksAsync);
        workGroup.MapPost("", CreateWorkAsync);
        workGroup.MapPatch("/{id:int}", UpdateWorkAsync);
        
        workGroup.MapDelete("/{id:int}", DeleteWorkAsync)
            .WithName("DeleteWork")
            .Produces(StatusCodes.Status409Conflict);
        
        return workGroup;
    }
}