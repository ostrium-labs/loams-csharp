# minimal-api

An ASP.NET Core minimal API with one shared `LoamsClient`: `GET /instance`, `GET /whoami`, `GET /apps` (the `examples-csharp` namespace only). Loams errors map to HTTP statuses through their stable codes.

```console
export LOAMS_ENDPOINT=http://127.0.0.1:8080
dotnet run --project examples/minimal-api     # listens on http://127.0.0.1:5080 (EXAMPLE_URL to change)
curl localhost:5080/instance
```
