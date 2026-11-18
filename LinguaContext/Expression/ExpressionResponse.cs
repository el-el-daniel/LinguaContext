using LinguaContext.Work;

namespace LinguaContext.Expression;

public record ExpressionResponse(
    int Id,
    string Text, 
    string? Translation, 
    string Context, 
    WorkResponse Work);