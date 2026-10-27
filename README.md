# Loams C# examples

These examples target the published **Loams SDK 0.1.2** release, available as
the `Loams` NuGet package.

Example apps for the [Loams SDK for .NET](https://github.com/ostrium-labs/loams/tree/v0.1.2/sdks/csharp) (NuGet `Loams`). Loams is an open-source, Rust data platform: one engine and one Connect API for documents, search, graph and live queries. The SDK talks to it with one `LoamsClient` and namespaced modules.

| Example | What it shows |
|---|---|
| [`examples/quickstart`](examples/quickstart) | Instance/system, namespaces, collections, documents, text/vector/hybrid search, optional graph, SDK SQL and streams |
| [`examples/live-apps`](examples/live-apps) | The Live app directory (`client.LiveAdmin`): create, get, list |
| [`examples/minimal-api`](examples/minimal-api) | An ASP.NET Core minimal API with one shared `LoamsClient` |

The quickstart exercises the published app modules; `live-apps` and the minimal API retain Live administration and instance/system coverage. The examples do not construct raw Connect or SQL HTTP requests. See the [SDK notes example](https://github.com/ostrium-labs/loams/blob/v0.1.2/sdks/csharp/examples/Notes/Program.cs) for the same 0.1.2 signatures.

## Install

```console
dotnet add package Loams --version 0.1.2
```

The examples take the SDK from NuGet at `$(LoamsVersion)` (default exact version `[0.1.2]`, see `Directory.Build.props`). For explicit local SDK development, build against a matching source checkout (CI uses NuGet):

```console
git clone --branch v0.1.2 --depth 1 https://github.com/ostrium-labs/loams
dotnet build examples/quickstart -p:LoamsSource=monorepo -p:LoamsProject=$PWD/loams/sdks/csharp/src/Loams/Loams.csproj
```

## Run against a local engine

Start an engine (`cargo run -p loams -- dev` in the monorepo, or the compose stack), then:

```console
export LOAMS_ENDPOINT=http://127.0.0.1:8080   # the engine's address
export LOAMS_API_KEY=...                      # only if auth is on; a dev engine needs none
dotnet run --project examples/quickstart
dotnet run --project examples/live-apps
dotnet run --project examples/minimal-api
```

The examples use the `examples-csharp` namespace. Quickstart writes only the `sdk_tour` collection and its `sdk-tour-events` stream; `live-apps` only creates/reuses `examples_csharp_apps`. Minimal API endpoints are read-only.

## More

- Docs: <https://loams.dev/docs>
- Running Loams: [docs/release/preview.md](https://github.com/ostrium-labs/loams/blob/main/docs/release/preview.md)

Apache-2.0.
