using System.Net;
using System.Net.Http.Json;
using System.Text;
using LinguaContext.Work;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LinguaContext.Tests.Integration;

public sealed class WorkEndpointsTests : IClassFixture<WorkApiFactory>, IDisposable
{
    private readonly HttpClient _client;

    public WorkEndpointsTests(WorkApiFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Post_ValidWork_Returns201WithLocation_AndGetReturnsCreatedWork()
    {
        const string name = "El principito";
        using var post = await _client.PostAsJsonAsync("/works", new { name, type = "Book" });

        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        var created = await post.Content.ReadFromJsonAsync<WorkResponse>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal(name, created.Name);
        Assert.Equal("Book", created.Type);

        var location = post.Headers.Location;
        Assert.NotNull(location);
        var resourceUri = new Uri(_client.BaseAddress!, location);
        Assert.Equal(_client.BaseAddress!.GetLeftPart(UriPartial.Authority),
            resourceUri.GetLeftPart(UriPartial.Authority));
        Assert.Equal($"/works/{created.Id}", resourceUri.AbsolutePath);
        Assert.Equal(string.Empty, resourceUri.Query);

        using var get = await _client.GetAsync(resourceUri);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var loaded = await get.Content.ReadFromJsonAsync<WorkResponse>();
        Assert.Equal(created, loaded);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(201)]
    public async Task Post_InvalidName_Returns400WithNameError(int length)
    {
        using var response = await _client.PostAsJsonAsync("/works",
            new { name = new string('a', length), type = "Book" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.True(problem.Errors.TryGetValue("name", out var errors));
        Assert.NotEmpty(errors!);
    }

    [Fact]
    public async Task Post_UnknownEnumString_Returns400()
    {
        using var body = new StringContent(
            """{"name":"El principito","type":"NotAWorkType"}""",
            Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync("/works", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingId_Returns404()
    {
        // Generated positive IDs cannot collide with this value.
        using var response = await _client.GetAsync("/works/-1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose() => _client.Dispose();
}
