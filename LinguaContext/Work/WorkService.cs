using LinguaContext.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LinguaContext.Work;

public class WorkService(AppDbContext dbContext)
{
    private static WorkResponse ToWorkResponse(Work work) => 
        new(work.Id, work.Name, work.Type.ToString());
    
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
}