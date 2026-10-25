# Loams C# examples

These examples target **Loams SDK 0.1.2**. Installation and CI require the matching
0.1.2 packages and release tags to be published.

Example apps for the [Loams SDK for .NET](https://github.com/ostrium-labs/loams/tree/v0.1.2/sdks/csharp) (NuGet `Loams`). Loams is an open-source, Rust data platform: one engine and one Connect API for documents, search, graph and live queries. The SDK talks to it with one `LoamsClient` and namespaced modules.

| Example | What it shows |
|---|---|
| [`examples/quickstart`](examples/quickstart) | Connect, `GetInstance`, `WhoAmI`, the version check and feature detection |
| [`examples/live-apps`](examples/live-apps) | The Live app directory (`client.LiveAdmin`): create, get, list |
| [`examples/minimal-api`](examples/minimal-api) | An ASP.NET Core minimal API with one shared `LoamsClient` |

The SDK also provides namespaces, collections, documents, search, vector, graph, SQL and streams. These examples focus on instance discovery, Live app administration and ASP.NET integration. See the [SDK app example](https://github.com/ostrium-labs/loams/tree/v0.1.2/sdks/csharp/examples) for the app modules.

## Install

```console
dotnet add package Loams --version 0.1.2
```

The examples take the SDK from NuGet at `$(LoamsVersion)` (default `0.1.2`, see `Directory.Build.props`). For explicit local SDK development, build against a matching source checkout (CI uses NuGet):

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

The examples only write to the Live app `examples_csharp_apps` in the namespace `examples-csharp`.

## More

- Docs: <https://loams.dev/docs>
- Release notes: [docs/release/preview.md](https://github.com/ostrium-labs/loams/blob/dev/docs/release/preview.md)

Apache-2.0.
