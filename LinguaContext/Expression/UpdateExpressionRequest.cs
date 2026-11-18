namespace LinguaContext.Expression;

public record UpdateExpressionRequest(
    string Text, 
    string? Translation, 
    string Context, 
    int WorkId);