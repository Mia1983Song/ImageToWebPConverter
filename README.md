# ImageToWebPConverter

A Windows application for batch converting images (PNG, JPG, JPEG, BMP, TIFF) to WebP format with optional resizing support.

## Features

- Batch convert multiple images to WebP format
- Support for PNG, JPG, JPEG, BMP, TIFF input formats
- Adjustable quality settings (1-100)
- Optional image resizing with aspect ratio preservation
- Recursive subfolder processing
- Progress tracking with detailed status
- Both GUI (WPF) and CLI interfaces available

## Project Structure

```
ImageToWebPConverter/
├── ImageToWebPConverter.Core/    # Core conversion logic
├── ImageToWebPConverter.UI/      # WPF GUI application
├── ImageToWebPConverter.Tests/   # Unit tests
└── ImageToWebPConverter/         # Console application
```

## Tech Stack

- .NET 6.0 / .NET 8.0
- [ImageSharp](https://github.com/SixLabors/ImageSharp) - Cross-platform image processing
- WPF (Windows Presentation Foundation) - Desktop UI
- xUnit - Unit testing framework
- MVVM pattern for UI architecture

## Getting Started

### Prerequisites

- .NET 6.0 SDK or later
- Windows OS (for WPF UI)

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Run GUI Application

```bash
dotnet run --project ImageToWebPConverter.UI
```

### Run Console Application

```bash
dotnet run --project ImageToWebPConverter
```

## Usage

See [USAGE.md](USAGE.md) for detailed usage instructions.

### Quick Start (GUI)

1. Launch `ImageToWebPConverter.UI.exe`
2. Select source folder containing images
3. Choose output folder (defaults to source folder)
4. Adjust quality and resize settings as needed
5. Click "Start" to begin conversion

## License

MIT License

## Author

[Your Name]
