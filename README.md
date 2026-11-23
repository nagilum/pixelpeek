# PixelPeek

A simple, cross-platform image slideshow.

Point PixelPeek at one or more folders and it cycles through the images with optional fade transitions, shuffling, and fullscreen mode. It's built with [.NET](https://dotnet.microsoft.com/) and [Avalonia UI](https://avaloniaui.net/), so it runs on Windows, macOS, and Linux.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

## Features

- Slideshow from one or more folders, optionally including subfolders
- Filter files with one or more wildcard patterns (`*.jpg`, `IMG_*`, …)
- Configurable interval between images
- Optional fade-out/fade-in transitions
- Shuffle mode
- Start in fullscreen, or toggle it at any time
- Pause and resume with a single key
- Images scale to fit the window, keeping their aspect ratio
- The mouse cursor hides after 2 seconds without movement
- The title bar shows the current file name and its pixel dimensions
- Files that can't be decoded as images are skipped automatically. If none of them can be opened, PixelPeek prints a message and exits

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (to build)
- Windows 10+, macOS, or Linux with a desktop environment

## Building

Clone the repository and build the solution:

```sh
git clone https://github.com/nagilum/pixelpeek.git
cd pixelpeek
dotnet build
```

To run it straight from the source tree, pass PixelPeek's arguments after `--`:

```sh
dotnet run --project PixelPeekApp -- ~/Pictures --shuffle
```

To produce a standalone build you can put on your `PATH`:

```sh
dotnet publish PixelPeekApp -c Release -o publish
```

The `publish` folder then contains the `PixelPeek` executable (`PixelPeek.exe` on Windows). This build needs the .NET 10 runtime installed. Add `-r <runtime-id> --self-contained` (for example `-r linux-x64` or `-r win-x64`) to bundle the runtime instead.

## Usage

```
PixelPeek [path-to-images] [<options>]
```

You can give several paths, and they're shown in the order you list them. Running PixelPeek without arguments prints the help text.

### Options

| Option | Description | Default |
| --- | --- | --- |
| `-i`, `--interval <ms>` | Milliseconds between image changes. Must be at least 1. | `5000` |
| `-f`, `--fade <ms>` | Fade duration in milliseconds. Used for both the fade-out and the fade-in, so a full transition takes twice as long. | `0` (no fade) |
| `-s`, `--shuffle` | Show the images in random order. | Off |
| `-r`, `--recursive` | Include images in subfolders. | Off |
| `-p`, `--pattern <ptn>` | Only include files matching this pattern. Can be repeated. | `*` |
| `-fs`, `--fullscreen` | Start in fullscreen. | Off |
| `-h`, `--help` | Show the help text and exit. | |

### Keyboard shortcuts

| Key | Action |
| --- | --- |
| `Space` or `P` | Pause / resume |
| `F` | Toggle fullscreen |
| `Esc` | Close PixelPeek |

### Examples

Show all images in a folder:

```sh
PixelPeek ~/Pictures/Holiday
```

Shuffle every JPEG and PNG under a folder tree, fullscreen, with a new image every 10 seconds:

```sh
PixelPeek ~/Pictures -r -s -fs -i 10000 -p "*.jpg" -p "*.png"
```

Combine several folders with a soft 300 ms fade between images:

```sh
PixelPeek ~/Pictures/2024 ~/Pictures/2025 --fade 300
```

### Tips

- **Quote your patterns.** Without quotes, the shell expands `*.jpg` against the current directory before PixelPeek sees it.
- **Patterns are case-sensitive on Linux.** `*.jpg` won't match `PHOTO.JPG`, so add `-p "*.JPG"` as well if your files use upper-case extensions.
- **Ordering:** without `--shuffle`, files are sorted by path (ignoring case) within each folder you pass, and folders are shown in the order you listed them. After the last image, the slideshow loops back to the first.
- Arguments that aren't recognised as options are treated as folder paths, so a mistyped option shows up as a "folder does not exist" error.

## Running the tests

The `PixelPeekTests` project contains xUnit tests for command-line parsing and file discovery:

```sh
dotnet test
```

## Project structure

```
PixelPeek.slnx
├── PixelPeekApp/          The application
│   ├── Program.cs           Entry point, argument parsing and file discovery
│   ├── ProgramOptions.cs    Parsed command-line options
│   ├── App.cs               Avalonia application setup
│   └── SlideshowWindow.cs   The slideshow window: image loading, fades, keys and cursor hiding
└── PixelPeekTests/        xUnit tests
```

## License

PixelPeek is released under the [MIT License](LICENSE).
