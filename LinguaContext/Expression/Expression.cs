namespace LinguaContext.Expression;

public class Expression(string text, string? translation, string context, int workId)
{
    public int Id { get; private set; }
    public string Text { get; private set; } = ValidateAndNormalizeText(text);
    public string? Translation { get; private set; } = ValidateAndNormalizeTranslation(translation);
    public string Context { get; private set; } = ValidateAndNormalizeContext(context);
    public int WorkId { get; private set; } = ValidateWorkId(workId);

    private static string ValidateAndNormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text is empty", nameof(text));
        
        var normalizedText = text.Trim();
        if (normalizedText.Length > 300)
            throw new ArgumentException("Text must not exceed 300 characters", nameof(text));
        
        return normalizedText;
    }
    
    private static string? ValidateAndNormalizeTranslation(string? translation)
    {
        if (string.IsNullOrWhiteSpace(translation))
            return null;
        
        var normalizedTranslation = translation.Trim();
        if (normalizedTranslation.Length > 1000)
            throw new ArgumentException("Translation must not exceed 1000 characters", nameof(translation));
        
        return normalizedTranslation;
    }

    private static string ValidateAndNormalizeContext(string context)
    {
        if (string.IsNullOrWhiteSpace(context))
            throw new ArgumentException("Context is empty", nameof(context));
        
        var normalizedContext = context.Trim();
        if (normalizedContext.Length > 2000)
            throw new ArgumentException("Context must not exceed 2000 characters", nameof(context));
        
        return normalizedContext;
    }
    
    private static int ValidateWorkId(int workId)
    {
        if (workId < 1)
            throw new ArgumentOutOfRangeException(nameof(workId), "WorkId should be valid positive number");
        
        return workId;
    }
    
    public void ChangeText(string text)
    {
        Text = ValidateAndNormalizeText(text);
    }

    public void ChangeTranslation(string? translation)
    {
        Translation = ValidateAndNormalizeTranslation(translation);
    }
    
    public void ChangeContext(string context)
    {
        Context = ValidateAndNormalizeContext(context);
    }
    
    public void ChangeWorkId(int workId)
    {
        WorkId = ValidateWorkId(workId);
    }
}