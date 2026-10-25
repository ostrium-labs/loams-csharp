# quickstart

Connects, then prints the instance (`GetInstance`), the calling principal (`WhoAmI`), the SDK/server version check and the package catalogue.

```console
export LOAMS_ENDPOINT=http://127.0.0.1:8080
dotnet run --project examples/quickstart
```

Expected output against a dev engine:

```
Loams 0.1.2 (Oss)
serves: loams.instance.v1, loams.collection.v1, ...
principal: dev-loopback (Local developer)
```
