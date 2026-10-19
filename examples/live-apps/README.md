# live-apps

Uses `client.LiveAdmin` to create the Live app `examples_csharp_apps` (namespace `examples-csharp`) if it is missing, read it back and list the apps in that namespace. Re-running is safe: an existing app is reused. It never touches other apps.

```console
export LOAMS_ENDPOINT=http://127.0.0.1:8080
dotnet run --project examples/live-apps
```
