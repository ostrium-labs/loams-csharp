// Quickstart: connect, ask the instance what it is, who you are, and whether this
// SDK and the server speak the same packages.
//
//   export LOAMS_ENDPOINT=http://127.0.0.1:8080   # `loams dev`
//   export LOAMS_API_KEY=...                      # a loopback dev engine needs none
//   dotnet run --project examples/quickstart

using Loams;
using Loams.Instance.V1;

var endpoint = System.Environment.GetEnvironmentVariable("LOAMS_ENDPOINT") ?? "http://127.0.0.1:8080";
var apiKey = System.Environment.GetEnvironmentVariable("LOAMS_API_KEY");

await using var client = new LoamsClient(new LoamsClientOptions
{
    Endpoint = endpoint,
    // No key is fine against a loopback `loams dev`; GetInstance needs none anyway.
    Auth = string.IsNullOrEmpty(apiKey) ? null : new ApiKeyTokenSource(apiKey),
});

try
{
    // The first call any client makes: what is this instance? Needs no credentials.
    var info = await client.Instance.GetInstanceAsync(new GetInstanceRequest());
    Console.WriteLine($"{info.Name} {info.ServerVersion} ({info.Edition})");
    Console.WriteLine($"serves: {string.Join(", ", info.ApiVersions)}");

    // Who the server thinks you are. A dev engine with auth off admits you as the
    // `dev-loopback` principal.
    var who = await client.Instance.WhoAmIAsync(new WhoAmIRequest());
    Console.WriteLine($"principal: {who.Principal?.Id} ({who.Principal?.DisplayName})");

    // R9 version check: a package this SDK speaks that the server does not serve is
    // reported, not thrown.
    var version = await client.System.VersionAsync();
    Console.WriteLine($"proto rev {version.ProtoRev}, compatible: {version.Compatible}");
    if (version.Missing.Count > 0)
    {
        Console.WriteLine($"server does not serve: {string.Join(", ", version.Missing)}");
    }

    // Feature detection without calling the module: one cached GetInstance.
    var catalogue = await client.System.CatalogueAsync();
    Console.WriteLine($"available packages: {catalogue.Served.Count}, unavailable: {catalogue.Unavailable.Count}");
}
catch (FeatureNotInVariantError e)
{
    Console.Error.WriteLine($"this instance does not carry that package (variant '{e.Variant}'): {e.Message}");
    return 1;
}
catch (LoamsError e)
{
    // A reason is a stable code; the message is for people.
    Console.Error.WriteLine($"loams error {e.Code}/{e.Reason}: {e.Message}");
    return 1;
}
return 0;
