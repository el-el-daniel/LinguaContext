using LinguaContext.Common.Pagination;
using LinguaContext.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LinguaContext.Work;

public class WorkService(AppDbContext dbContext)
{
    private static WorkResponse ToWorkResponse(Work work) => 
        new(work.Id, work.Name, work.Type.ToString());
    
    private static string EscapeLikePattern(string value) =>
        value
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    
    public async Task<WorkResponse> CreateAsync(
        CreateWorkRequest createWorkRequest,
        CancellationToken cancellationToken = default)
    {
        var work = new Work(createWorkRequest.Name, createWorkRequest.Type);
        dbContext.Works.Add(work);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return ToWorkResponse(work);
    }

    public async Task<WorkResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await dbContext.Works
            .Where(w => w.Id == id)
            .Select(w => new WorkResponse(
                w.Id, 
                w.Name, 
                w.Type.ToString()))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<PagedResponse<WorkResponse>> GetAllAsync(
        int page, 
        int pageSize, 
        WorkType? type,
        string? search,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than zero");
        
        if (pageSize is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100");
        
        if (type is not null && !Enum.IsDefined(type.Value))
            throw new ArgumentOutOfRangeException(nameof(type), "Invalid work type");
        
        var offset = (long)(page - 1) * pageSize;

        if (offset > int.MaxValue)
            throw new ArgumentOutOfRangeException(
                nameof(page), "Page number is too large");

        IQueryable<Work> query = dbContext.Works;
        
        if (type is not null)
            query = query.Where(w => w.Type == type);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            if (normalizedSearch.Length > 100)
                throw new ArgumentOutOfRangeException(
                    nameof(search), "Search must not exceed 100 characters");
            
            var pattern = $"%{EscapeLikePattern(normalizedSearch)}%";
            query = query.Where(w => 
                EF.Functions.ILike(w.Name, pattern, "\\"));
        }

        var count = await query.CountAsync(cancellationToken);
        
        var works = await query
            .OrderBy(w => w.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .Select(w => new WorkResponse(
                w.Id,
                w.Name,
                w.Type.ToString()))
            .ToListAsync(cancellationToken);
        
        return new PagedResponse<WorkResponse>(works, page, pageSize, count);
    }

    public async Task<WorkResponse?> UpdateAsync(
        int id,
        UpdateWorkRequest updateWorkRequest,
        CancellationToken cancellationToken = default)
    {
        var work = await dbContext.Works.FindAsync([id], cancellationToken);
        if (work is null)
            return null;

        if (updateWorkRequest.Name is not null)
            work.Rename(updateWorkRequest.Name);

        if (updateWorkRequest.Type is not null)
            work.ChangeType(updateWorkRequest.Type.Value);

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToWorkResponse(work);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var work = await dbContext.Works.FindAsync([id], cancellationToken);
        if (work is null)
            return false;
        
        dbContext.Works.Remove(work);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}