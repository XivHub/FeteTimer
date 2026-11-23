# Development

## Build

Requires the .NET SDK and an extracted Dalamud dev bundle. Shared helpers come from
[XivHubPluginKit](https://github.com/XivHub/XivHubPluginKit), linked by source: clone it next to this
repo, so `../XivHubPluginKit` exists.

```bash
DALAMUD_HOME=~/.cache/dalamud-dev DOTNET_ROOT=~/.dotnet \
  dotnet build FeteTimer/FeteTimer.csproj -c Release -p:Platform=x64
```

The build writes `FeteTimer.dll` and a packaged `latest.zip` under `FeteTimer/bin/x64/Release/`.

## Publish

Bump `<Version>` in `FeteTimer/FeteTimer.csproj` so Dalamud sees an update, then run `publish-plugin` from the repo
root. It builds Release and replaces FeteTimer's entry in the XIV Hub `pluginmaster.json`, leaving the
other plugins alone.
