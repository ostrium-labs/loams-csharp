// The 0.1.2 SDK tour: instance/system, namespaces, collections, documents,
// text/vector/hybrid search, graph, SQL and streams. All writes stay in
// examples-csharp; graph and stream names are also scoped to this example.
//
//   LOAMS_ENDPOINT=http://127.0.0.1:8080 dotnet run --project examples/quickstart

using Loams;
using Loams.Collection.V1;
using Fusion = Loams.Fusion;

const string Namespace = "examples-csharp";
const string Collection = "sdk_tour";

var options = LoamsClientOptions.FromEnv() with { SessionConsistency = true, Namespace = Namespace };
await using var loams = new LoamsClient(options);

try
{
    // Instance and system modules: discover the build and check optional features.
    var info = await loams.Instance.GetInstanceAsync();
    var who = await loams.Instance.WhoAmIAsync();
    Console.WriteLine($"connected to {info.Name} {info.ServerVersion} as {who.Principal?.Id}");
    var version = await loams.System.VersionAsync();
    var catalogue = await loams.System.CatalogueAsync();
    Console.WriteLine($"SDK/server compatible: {version.Compatible}; served packages: {catalogue.Served.Count}");

    // Namespace and collection modules. Existing namespace/collection are safe;
    // the same schema is reused and no data is dropped.
    try { await loams.Namespaces.CreateAsync(Namespace); } catch (AlreadyExistsError) { }
    var schema = new Schema().Text("title").Text("body").Keyword("tags").I64("n").Vector("embedding", 4);
    try { await loams.Collections.CreateAsync(Collection, schema); } catch (AlreadyExistsError) { }
    var notes = loams.Collection(Collection);

    // Documents module through the collection handle: upsert, get, patch, count
    // and scroll. Stable IDs make reruns repeatable.
    Doc Note(ulong id, string title, string body, string[] tags, float[] vector) =>
        new(id, new Dictionary<string, object?> { ["title"] = title, ["body"] = body, ["tags"] = tags, ["n"] = (long)id },
            new Dictionary<string, float[]> { ["embedding"] = vector });
    var write = await notes.UpsertAsync([
        Note(1, "Refund policy", "Refunds are issued within 14 days.", ["policy", "billing"], [1f, 0f, 0f, 0f]),
        Note(2, "Shipping", "Orders ship in two business days.", ["shipping"], [0f, 1f, 0f, 0f]),
        Note(3, "Refund window", "The refund window closes after 30 days.", ["policy"], [0.9f, 0.1f, 0f, 0f]),
    ]);
    Console.WriteLine($"upsert token: {write.Token}");
    Console.WriteLine($"get returned {(await notes.GetAsync([1, 3])).Documents.Count} documents");
    await notes.PatchAsync(2, set: new { title = "Shipping times" });
    Console.WriteLine($"policy count: {await notes.CountAsync(Filter.Eq("tags", "policy"))}");
    await foreach (var doc in notes.ScrollAllAsync(limit: 10))
        Console.WriteLine($"doc {doc.Id.Text}: {doc.Source.Fields["title"].StringValue}");

    // Search and vector modules: filtered text, standalone vector, and hybrid.
    var text = await loams.Search.QueryAsync(Search.On(Collection).Text("refund", "body")
        .Filter(Filter.Eq("tags", "policy")).Select(include: ["title"]).Sort(Sort.Score(), Sort.Pk()).Limit(3));
    var vector = await loams.Vector.SearchAsync(Collection, [1f, 0f, 0f, 0f], field: "embedding", limit: 3);
    var hybrid = await loams.Search.QueryAsync(Search.On(Collection).Text("refund", "body")
        .Vector("embedding", [1f, 0f, 0f, 0f]).Fuse(Fusion.Rrf(60)).Limit(3));
    Console.WriteLine($"search hits: {text.Hits.Count}; vector hits: {vector.Hits.Count}; hybrid hits: {hybrid.Hits.Count}");

    // SQL module over the same collection, with a bound parameter.
    var sql = await loams.Sql.QueryAsync("SELECT n, title FROM sdk_tour WHERE n > $1 ORDER BY n", [1]);
    Console.WriteLine($"SQL rows: {sql.Records().Count}");

    // Streams module with a producer identity/sequence: rerunning will not append
    // the same logical batch twice. Read back only this batch's offsets.
    await loams.Streams.CreateAsync("sdk-tour-events", partitions: 1);
    var batch = await loams.Streams.ProduceAsync("sdk-tour-events", 0,
        [new ProducerRecord("1", new Dictionary<string, object?> { ["kind"] = "note-upserted", ["id"] = 1 })],
        producerId: "examples-csharp-sdk-tour", producerSeq: 1);
    await foreach (var record in loams.Streams.Subscribe("sdk-tour-events", 0, fromOffset: batch.BaseOffset))
    {
        Console.WriteLine($"stream event at {record.Offset}: {record.Text}");
        if (record.Offset >= batch.LastOffset) break;
    }

    // Graph is experimental and optional. Only create a namespaced demo graph and
    // run a read-only query; do not delete user data on exit.
    if (await loams.System.AvailableAsync("graph"))
    {
        try { await loams.Graph.CreateAsync("examples_csharp_sdk_tour"); } catch (AlreadyExistsError) { }
        var graph = await loams.Graph.QueryAsync("examples_csharp_sdk_tour", "RETURN 1 AS ready", readOnly: true);
        Console.WriteLine($"graph rows: {graph.Rows.Rows.Count}");
    }
    else Console.WriteLine("graph: not in this build");
}
catch (FeatureNotInVariantError e)
{
    Console.Error.WriteLine($"optional SDK feature unavailable ({e.Variant}): {e.Message}");
    return 1;
}
catch (LoamsError e)
{
    Console.Error.WriteLine($"loams error {e.Code}/{e.Reason}: {e.Message}");
    return 1;
}

return 0;
