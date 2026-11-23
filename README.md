# FeteTimer

Counts down to the next Ishgardian Restoration Fête (Skyrise Celebration) and alerts you before it
starts.

## How it works

The game only shows the fête countdown inside the Firmament. Visit the Firmament once and FeteTimer
remembers the start time for your world. After that it counts down anywhere.

Each visit covers the next fête on that world. Once it has passed, stop by the Firmament again.

Works on the English and Japanese clients.

## Features

- Alerts by chat, notification and sound; turn each one on or off.
- Choose when to be alerted. By default: 5 minutes before, 1 minute before, and at the start.
- A countdown in the server info bar.
- Remembers each world separately.

## Install

In `/xlsettings`, under Experimental, add this to Custom Plugin Repositories:

```
https://plugins.xivhub.net/pluginmaster.json
```

Then install FeteTimer from the plugin installer.

## Commands

| Command | What it does |
|---|---|
| `/fetetimer` | Open the main window |

Building from source: see [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md).

## License

AGPL-3.0-or-later. See `LICENSE`.
