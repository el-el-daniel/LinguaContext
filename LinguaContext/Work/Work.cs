namespace LinguaContext.Work;

public class Work(string name, WorkType type)
{
    public int Id { get; private set; }
    public string Name { get; private set; } = ValidateAndNormalizeName(name);
    public WorkType Type { get; private set; } = ValidateType(type);

    private static string ValidateAndNormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is empty", nameof(name));
        
        var normalizedName = name.Trim();
        if (normalizedName.Length > 200)
            throw new ArgumentException("Name must not exceed 200 characters", nameof(name));
        
        return normalizedName;
    }

    private static WorkType ValidateType(WorkType type)
    {
        return !Enum.IsDefined(type) 
            ? throw new ArgumentException("Type not defined", nameof(type)) 
            : type;
    }

    public void Rename(string newName)
    {
        Name = ValidateAndNormalizeName(newName);
    }

    public void ChangeType(WorkType newType)
    {
        Type = ValidateType(newType);
    }
}