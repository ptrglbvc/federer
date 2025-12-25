#!/bin/bash
set -e

REPO="ptrglbvc/federer"
INSTALL_DIR="/usr/local/bin"

# Detect OS and architecture
OS=$(uname -s | tr '[:upper:]' '[:lower:]')
ARCH=$(uname -m)

case "$OS" in
  linux)  PLATFORM="linux-x64" ;;
  darwin)
    case "$ARCH" in
      x86_64) PLATFORM="osx-x64" ;;
      arm64)  PLATFORM="osx-arm64" ;;
    esac
    ;;
  *) echo "Unsupported OS: $OS"; exit 1 ;;
esac

echo "Downloading federer for $PLATFORM..."

LATEST=$(curl -s https://api.github.com/repos/$REPO/releases/latest | grep tag_name | cut -d '"' -f 4)
URL="https://github.com/$REPO/releases/download/$LATEST/federer-$PLATFORM.zip"

TMP_DIR=$(mktemp -d)
curl -L "$URL" -o "$TMP_DIR/federer.zip"
unzip -q "$TMP_DIR/federer.zip" -d "$TMP_DIR"

sudo mv "$TMP_DIR/$PLATFORM/federer" "$INSTALL_DIR/"
sudo chmod +x "$INSTALL_DIR/federer"

rm -rf "$TMP_DIR"

echo "✅ federer installed successfully!"
echo "Run 'federer --help' to get started"
