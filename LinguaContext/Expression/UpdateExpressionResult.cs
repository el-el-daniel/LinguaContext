namespace LinguaContext.Expression;

public record UpdateExpressionResult(
    ExpressionResponse? Expression,
    UpdateExpressionError? Error);