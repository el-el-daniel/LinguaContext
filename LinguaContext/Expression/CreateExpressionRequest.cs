namespace LinguaContext.Expression;

public record CreateExpressionRequest(
    string Text, 
    string? Translation, 
    string Context, 
    int WorkId);