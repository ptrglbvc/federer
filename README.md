# 🎾 Federer

An elegant, small HTTP file server with streaming and range request support.

## Installation

### Via .NET Tool (Recommended)

```bash
dotnet tool install --global federer
```

### Via Homebrew (macOS/Linux)

```bash
brew tap ptrglbvc/tap
brew install federer
```

### Manual Download

Download the latest release from [GitHub Releases](https://github.com/ptrglbvc/federer/releases).

### Installation Script

```bash
curl -fsSL https://raw.githubusercontent.com/ptrglbvc/federer/main/install.sh | bash
```

## Usage

```bash
# Basic usage
federer /video=/path/to/video.mp4

# Multiple routes
federer /video=/path/to/video.mp4 /music=/path/to/song.mp3

# Custom port
federer -p 8080 /video=/path/to/video.mp4

# Show help
federer --help
```

## Features

- 🚀 Lightweight and fast
- 📁 Stream large files without loading into memory
- 📍 Range request support (video seeking, resume downloads)
- 🎯 Simple route-based configuration

## Examples

Serve a video file:
```bash
federer -p 3000 /movie=/home/user/Movies/movie.mp4
# Access at http://localhost:3000/movie
```

Serve multiple files:
```bash
federer /doc=/path/to/manual.pdf /vid=/path/to/tutorial.mp4
```

## License

MIT
