// A minimal ASP.NET Core API in front of a Loams instance: one shared
// LoamsClient (it is thread-safe and holds the connection pool) behind three
// read-only endpoints. `/apps` only lists the `examples-csharp` namespace.
//
//   export LOAMS_ENDPOINT=http://127.0.0.1:8080
//   dotnet run --project examples/minimal-api
//   curl localhost:5080/instance
//   curl localhost:5080/whoami
//   curl localhost:5080/apps

using Loams;
using Loams.Instance.V1;
using Loams.Live.V1;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(System.Environment.GetEnvironmentVariable("EXAMPLE_URL") ?? "http://127.0.0.1:5080");

var apiKey = System.Environment.GetEnvironmentVariable("LOAMS_API_KEY");
builder.Services.AddSingleton(new LoamsClient(new LoamsClientOptions
{
    Endpoint = System.Environment.GetEnvironmentVariable("LOAMS_ENDPOINT") ?? "http://127.0.0.1:8080",
    Auth = string.IsNullOrEmpty(apiKey) ? null : new ApiKeyTokenSource(apiKey),
}));

var app = builder.Build();

app.MapGet("/instance", async (LoamsClient loams, CancellationToken ct) =>
{
    var info = await loams.Instance.GetInstanceAsync(new GetInstanceRequest(), cancellationToken: ct);
    return Results.Ok(new { info.Name, info.ServerVersion, info.ApiVersions });
});

app.MapGet("/whoami", async (LoamsClient loams, CancellationToken ct) =>
{
    var who = await loams.Instance.WhoAmIAsync(new WhoAmIRequest(), cancellationToken: ct);
    return Results.Ok(new { who.Principal?.Id, who.Principal?.DisplayName, Org = who.Org?.Name });
});

app.MapGet("/apps", async (LoamsClient loams, CancellationToken ct) =>
{
    var page = await loams.LiveAdmin.ListAppsAsync(new ListAppsRequest { PageSize = 100 }, cancellationToken: ct);
    return Results.Ok(page.Apps.Where(a => a.Namespace == "examples-csharp").Select(a => new { a.Name, a.Namespace, State = a.State.ToString() }));
});

// A Loams error maps to an HTTP status through its stable code, not its message.
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (LoamsError e)
    {
        context.Response.StatusCode = e switch
        {
            NotFoundError => 404,
            UnauthenticatedError => 401,
            PermissionDeniedError => 403,
            UnimplementedError => 501,
            UnavailableError => 503,
            _ => 502,
        };
        await context.Response.WriteAsJsonAsync(new { error = e.Reason.ToString(), message = e.Message });
    }
});

app.Run();
