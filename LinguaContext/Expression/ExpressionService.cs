using LinguaContext.Common.Pagination;
using LinguaContext.Infrastructure;
using LinguaContext.Work;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LinguaContext.Expression;

public class ExpressionService(AppDbContext dbContext)
{
    private static ExpressionResponse ToExpressionResponse(Expression expression, Work.Work work)
    {
        var workResponse = new WorkResponse(work.Id, work.Name, work.Type.ToString());
        return new(
            expression.Id, 
            expression.Text, 
            expression.Translation, 
            expression.Context, 
            workResponse);
    }
    
    public async Task<ExpressionResponse?> CreateAsync(
        CreateExpressionRequest request,
        CancellationToken cancellationToken = default)
    {
        var work = await dbContext.Works
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == request.WorkId, cancellationToken);

        if (work is null)
            return null;

        var expression = new Expression(
            request.Text, request.Translation, request.Context, request.WorkId);
        dbContext.Expressions.Add(expression);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
                  {
                      SqlState: PostgresErrorCodes.ForeignKeyViolation,
                      ConstraintName: DbConstraints.ExpressionWorkForeignKey
                  })
        {
            return null;
        }
        
        return ToExpressionResponse(expression, work);
    }

    public async Task<ExpressionResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await dbContext.Expressions
            .Where(e => e.Id == id)
            .Join(
                dbContext.Works,
                expression => expression.WorkId,
                work => work.Id,
                (expression, work) => new ExpressionResponse(
                    expression.Id,
                    expression.Text,
                    expression.Translation,
                    expression.Context,
                    new WorkResponse(
                        work.Id,
                        work.Name,
                        work.Type.ToString())))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<PagedResponse<ExpressionResponse>> GetAllAsync(
        int page,
        int pageSize,
        int? workId,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than zero");
        
        if (pageSize is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100");
        
        var offset = (long)(page - 1) * pageSize;
        
        if (offset > int.MaxValue)
            throw new ArgumentOutOfRangeException(
                nameof(page), "Page number is too large");

        IQueryable<Expression> query = dbContext.Expressions;

        if (workId is not null)
        {
            if (workId is <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(workId), "Work ID must be greater than zero");
            query = query.Where(e => e.WorkId == workId);
        }

        var count = await query.CountAsync(cancellationToken);

        var expressions = await query
            .OrderBy(e => e.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .Join(
                dbContext.Works,
                expression => expression.WorkId,
                work => work.Id,
                (expression, work) => new { Expression = expression, Work = work})
            .OrderBy(x => x.Expression.Id)
            .Select(x => new ExpressionResponse(
                x.Expression.Id,
                x.Expression.Text,
                x.Expression.Translation,
                x.Expression.Context,
                new WorkResponse(
                    x.Work.Id,
                    x.Work.Name,
                    x.Work.Type.ToString())))
            .ToListAsync(cancellationToken);
        
        return new PagedResponse<ExpressionResponse>(expressions, page, pageSize, count);
    }

    public async Task<UpdateExpressionResult> UpdateAsync(
        int id,
        UpdateExpressionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.WorkId < 1)
            throw new ArgumentOutOfRangeException(
                nameof(request.WorkId),
                "Work ID must be greater than zero");
        
        var expression = await dbContext.Expressions
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (expression is null)
            return new(null, UpdateExpressionError.ExpressionNotFound);

        var work = await dbContext.Works
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == request.WorkId, cancellationToken);

        if (work is null)
            return new(null, UpdateExpressionError.WorkNotFound);
        
        expression.ChangeText(request.Text);
        expression.ChangeTranslation(request.Translation);
        expression.ChangeContext(request.Context);
        expression.ChangeWorkId(request.WorkId);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new(null, UpdateExpressionError.ExpressionNotFound);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
                  {
                      SqlState: PostgresErrorCodes.ForeignKeyViolation,
                      ConstraintName: DbConstraints.ExpressionWorkForeignKey
                  })
        {
            return new(null, UpdateExpressionError.WorkNotFound);
        }

        return new(ToExpressionResponse(expression, work), null);
    }
    
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var expression = await dbContext.Expressions.FindAsync([id], cancellationToken);
        if (expression is null)
            return false;
        
        dbContext.Expressions.Remove(expression);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
        
        return true;
    }
}