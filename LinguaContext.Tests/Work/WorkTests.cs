using LinguaContext.Work;

namespace LinguaContext.Tests.Work;

public class WorkTests
{
    [Fact]
    public void Constructor_ValidArguments_CreatesWork()
    {
        var work = new LinguaContext.Work.Work(
            "El principito",
            WorkType.Book);

        Assert.Equal("El principito", work.Name);
        Assert.Equal(WorkType.Book, work.Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_InvalidName_ThrowsArgumentException(
        string? name)
    {
        Assert.Throws<ArgumentException>(
            () => new LinguaContext.Work.Work(
                name!,
                WorkType.Book));
    }

    [Fact]
    public void Constructor_InvalidType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new LinguaContext.Work.Work(
                "El principito",
                (WorkType)67));
    }

    [Fact]
    public void Constructor_NameWithWhitespace_TrimsName()
    {
        var work = new LinguaContext.Work.Work(
            "  El principito  ",
            WorkType.Book);

        Assert.Equal("El principito", work.Name);
    }

    [Fact]
    public void Rename_ValidName_UpdatesName()
    {
        var work = CreateWork();

        work.Rename("  Nuevo nombre  ");

        Assert.Equal("Nuevo nombre", work.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Rename_InvalidName_ThrowsAndPreservesOriginalName(
        string? name)
    {
        var work = CreateWork();

        Assert.Throws<ArgumentException>(
            () => work.Rename(name!));

        Assert.Equal("El principito", work.Name);
    }

    [Fact]
    public void ChangeType_ValidType_UpdatesType()
    {
        var work = CreateWork();

        work.ChangeType(WorkType.Movie);

        Assert.Equal(WorkType.Movie, work.Type);
    }

    [Fact]
    public void ChangeType_InvalidType_ThrowsAndPreservesOriginalType()
    {
        var work = CreateWork();

        Assert.Throws<ArgumentException>(
            () => work.ChangeType((WorkType)67));

        Assert.Equal(WorkType.Book, work.Type);
    }

    private static LinguaContext.Work.Work CreateWork()
    {
        return new LinguaContext.Work.Work(
            "El principito",
            WorkType.Book);
    }
}