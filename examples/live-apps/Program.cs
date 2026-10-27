// Live apps: the app directory (`client.LiveAdmin`). Creates the app
// `examples_csharp_apps` in the namespace `examples-csharp` if it is missing, reads it
// back, and lists the apps in that namespace. It never touches any other app.
//
//   export LOAMS_ENDPOINT=http://127.0.0.1:8080
//   dotnet run --project examples/live-apps
//
// This example focuses on Live app administration. The SDK also provides
// collection, search, graph, SQL and stream modules.

using Loams;
using Loams.Live.V1;

const string AppName = "examples_csharp_apps";
const string Namespace = "examples-csharp";

var endpoint = Environment.GetEnvironmentVariable("LOAMS_ENDPOINT") ?? "http://127.0.0.1:8080";
var apiKey = Environment.GetEnvironmentVariable("LOAMS_API_KEY");

await using var client = new LoamsClient(new LoamsClientOptions
{
    Endpoint = endpoint,
    Auth = string.IsNullOrEmpty(apiKey) ? null : new ApiKeyTokenSource(apiKey),
});

try
{
    // Feature-detect first: a build without the Live package answers
    // `feature_not_in_variant` here instead of on the first real call.
    await client.System.GuardAsync("live.admin");

    try
    {
        var created = await client.LiveAdmin.CreateAppAsync(new CreateAppRequest
        {
            Name = AppName,
            Namespace = Namespace,
        });
        Console.WriteLine($"created app {created.App.Name} (id {created.App.Id}, keyspace {created.App.Keyspace})");
    }
    catch (FailedPreconditionError e) when (e.Reason == Reason.LiveAppExists)
    {
        Console.WriteLine($"app {AppName} already exists; reusing it");
    }

    var got = await client.LiveAdmin.GetAppAsync(new GetAppRequest { Name = AppName });
    Console.WriteLine($"get: {got.App.Name} state={got.App.State} namespace={got.App.Namespace}");

    var page = await client.LiveAdmin.ListAppsAsync(new ListAppsRequest { PageSize = 100 });
    foreach (var app in page.Apps.Where(a => a.Namespace == Namespace))
    {
        Console.WriteLine($"list: {app.Name} id={app.Id} state={app.State}");
    }
}
catch (FeatureNotInVariantError e)
{
    Console.Error.WriteLine($"this instance does not carry live admin (variant '{e.Variant}')");
    return 1;
}
catch (LoamsError e)
{
    Console.Error.WriteLine($"loams error {e.Code}/{e.Reason}: {e.Message}");
    return 1;
}
return 0;
