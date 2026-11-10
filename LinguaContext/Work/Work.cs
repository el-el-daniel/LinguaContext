namespace LinguaContext.Work;

public class Work
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public WorkType Type { get; private set; }

    public Work(string name, WorkType type)
    {
        Name = ValidateAndNormalizeName(name);
        Type = ValidateType(type);
    }

    private static string ValidateAndNormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is empty", nameof(name));
        
        var nameTrimmed = name.Trim();
        if (nameTrimmed.Length > 200)
            throw new ArgumentException("Name is too long", nameof(name));
        
        return nameTrimmed;
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