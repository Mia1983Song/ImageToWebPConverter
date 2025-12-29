using ImageToWebPConverter.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageToWebPConverter.Tests;

public class ImageConversionServiceTests : IDisposable
{
    private readonly string _testInputFolder;
    private readonly string _testOutputFolder;
    private readonly ImageConversionService _service;

    public ImageConversionServiceTests()
    {
        _testInputFolder = Path.Combine(Path.GetTempPath(), $"ImageConversionTest_Input_{Guid.NewGuid()}");
        _testOutputFolder = Path.Combine(Path.GetTempPath(), $"ImageConversionTest_Output_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testInputFolder);
        _service = new ImageConversionService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testInputFolder))
            Directory.Delete(_testInputFolder, true);
        if (Directory.Exists(_testOutputFolder))
            Directory.Delete(_testOutputFolder, true);
    }

    private void CreateTestImage(string fileName, int width = 100, int height = 100)
    {
        var filePath = Path.Combine(_testInputFolder, fileName);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        using var image = new Image<Rgba32>(width, height, Color.Red);
        image.Save(filePath);
    }

    [Fact]
    public async Task ConvertFolderAsync_WithValidPngImage_ConvertsToWebp()
    {
        // Arrange
        CreateTestImage("test.png");
        var options = new ImageConversionOptions
        {
            InputFolder = _testInputFolder,
            OutputFolder = _testOutputFolder,
            Quality = 85
        };

        // Act
        var summary = await _service.ConvertFolderAsync(options);

        // Assert
        Assert.Equal(1, summary.Total);
        Assert.Equal(1, summary.Converted);
        Assert.Equal(0, summary.Failed);
        Assert.Equal(0, summary.Skipped);
        Assert.True(File.Exists(Path.Combine(_testOutputFolder, "test.webp")));
    }

    [Fact]
    public async Task ConvertFolderAsync_WithValidJpgImage_ConvertsToWebp()
    {
        // Arrange
        CreateTestImage("test.jpg");
        var options = new ImageConversionOptions
        {
            InputFolder = _testInputFolder,
            OutputFolder = _testOutputFolder,
            Quality = 85
        };

        // Act
        var summary = await _service.ConvertFolderAsync(options);

        // Assert
        Assert.Equal(1, summary.Total);
        Assert.Equal(1, summary.Converted);
        Assert.True(File.Exists(Path.Combine(_testOutputFolder, "test.webp")));
    }

    [Fact]
    public async Task ConvertFolderAsync_WithMultipleImages_ConvertsAll()
    {
        // Arrange
        CreateTestImage("image1.png");
        CreateTestImage("image2.jpg");
        CreateTestImage("image3.jpeg");
        var options = new ImageConversionOptions
        {
            InputFolder = _testInputFolder,
            OutputFolder = _testOutputFolder,
            Quality = 85
        };

        // Act
        var summary = await _service.ConvertFolderAsync(options);

        // Assert
        Assert.Equal(3, summary.Total);
        Assert.Equal(3, summary.Converted);
        Assert.True(File.Exists(Path.Combine(_testOutputFolder, "image1.webp")));
        Assert.True(File.Exists(Path.Combine(_testOutputFolder, "image2.webp")));
        Assert.True(File.Exists(Path.Combine(_testOutputFolder, "image3.webp")));
    }

    [Fact]
    public async Task ConvertFolderAsync_WithSubfolders_ConvertsAllWhenIncludeSubfoldersIsTrue()
    {
        // Arrange
        CreateTestImage("root.png");
        CreateTestImage("subfolder/nested.png");
        var options = new ImageConversionOptions
        {
            InputFolder = _testInputFolder,
            OutputFolder = _testOutputFolder,
            Quality = 85,
            IncludeSubfolders = true
        };

        // Act
        var summary = await _service.ConvertFolderAsync(options);

        // Assert
        Assert.Equal(2, summary.Total);
        Assert.Equal(2, summary.Converted);
        Assert.True(File.Exists(Path.Combine(_testOutputFolder, "root.webp")));
        Assert.True(File.Exists(Path.Combine(_testOutputFolder, "subfolder", "nested.webp")));
    }

    [Fact]
    public async Task ConvertFolderAsync_WithSubfolders_OnlyConvertsRootWhenIncludeSubfoldersIsFalse()
    {
        // Arrange
        CreateTestImage("root.png");
        CreateTestImage("subfolder/nested.png");
        var options = new ImageConversionOptions
        {
            InputFolder = _testInputFolder,
            OutputFolder = _testOutputFolder,
            Quality = 85,
            IncludeSubfolders = false
        };

        // Act
        var summary = await _service.ConvertFolderAsync(options);

        // Assert
        Assert.Equal(1, summary.Total);
        Assert.Equal(1, summary.Converted);
        Assert.True(File.Exists(Path.Combine(_testOutputFolder, "root.webp")));
        Assert.False(File.Exists(Path.Combine(_testOutputFolder, "subfolder", "nested.webp")));
    }

    [Fact]
    public async Task ConvertFolderAsync_WithExistingFile_SkipsWhenOverwriteIsFalse()
    {
        // Arrange
        CreateTestImage("test.png");
        Directory.CreateDirectory(_testOutputFolder);
        File.WriteAllText(Path.Combine(_testOutputFolder, "test.webp"), "existing");
        var options = new ImageConversionOptions
        {
            InputFolder = _testInputFolder,
            OutputFolder = _testOutputFolder,
            Quality = 85,
            OverwriteExisting = false
        };

        // Act
        var summary = await _service.ConvertFolderAsync(options);

        // Assert
        Assert.Equal(1, summary.Total);
        Assert.Equal(0, summary.Converted);
        Assert.Equal(1, summary.Skipped);
    }

    [Fact]
    public async Task ConvertFolderAsync_WithExistingFile_OverwritesWhenOverwriteIsTrue()
    {
        // Arrange
        CreateTestImage("test.png");
        Directory.CreateDirectory(_testOutputFolder);
        var existingPath = Path.Combine(_testOutputFolder, "test.webp");
        File.WriteAllText(existingPath, "existing");
        var originalSize = new FileInfo(existingPath).Length;
        var options = new ImageConversionOptions
        {
            InputFolder = _testInputFolder,
            OutputFolder = _testOutputFolder,
            Quality = 85,
            OverwriteExisting = true
        };

        // Act
        var summary = await _service.ConvertFolderAsync(options);

        // Assert
        Assert.Equal(1, summary.Total);
        Assert.Equal(1, summary.Converted);
        Assert.Equal(0, summary.Skipped);
        var newSize = new FileInfo(existingPath).Length;
        Assert.NotEqual(originalSize, newSize);
    }

    [Fact]
    public async Task ConvertFolderAsync_WithMaxWidth_ResizesImage()
    {
        // Arrange
        CreateTestImage("large.png", 500, 300);
        var options = new ImageConversionOptions
        {
            InputFolder = _testInputFolder,
            OutputFolder = _testOutputFolder,
            Quality = 85,
            MaxWidth = 200
        };

        // Act
        var summary = await _service.ConvertFolderAsync(options);

        // Assert
        Assert.Equal(1, summary.Converted);
        var outputPath = Path.Combine(_testOutputFolder, "large.webp");
        using var outputImage = Image.Load(outputPath);
        Assert.True(outputImage.Width <= 200);
    }

    [Fact]
    public async Task ConvertFolderAsync_WithMaxHeight_ResizesImage()
    {
        // Arrange
        CreateTestImage("large.png", 300, 500);
        var options = new ImageConversionOptions
        {
            InputFolder = _testInputFolder,
            OutputFolder = _testOutputFolder,
            Quality = 85,
            MaxHeight = 200
        };

        // Act
        var summary = await _service.ConvertFolderAsync(options);

        // Assert
        Assert.Equal(1, summary.Converted);
        var outputPath = Path.Combine(_testOutputFolder, "large.webp");
        using var outputImage = Image.Load(outputPath);
        Assert.True(outputImage.Height <= 200);
    }

    [Fact]
    public async Task ConvertFolderAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        CreateTestImage("test.png");
        var options = new ImageConversionOptions
        {
            InputFolder = _testInputFolder,
            OutputFolder = _testOutputFolder,
            Quality = 85
        };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _service.ConvertFolderAsync(options, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task ConvertFolderAsync_ReportsProgress()
    {
        // Arrange
        CreateTestImage("test.png");
        var options = new ImageConversionOptions
        {
            InputFolder = _testInputFolder,
            OutputFolder = _testOutputFolder,
            Quality = 85
        };
        var progressReports = new List<ConversionProgress>();
        var progress = new Progress<ConversionProgress>(p => progressReports.Add(p));

        // Act
        await _service.ConvertFolderAsync(options, progress);
        await Task.Delay(100); // Allow progress events to be processed

        // Assert
        Assert.True(progressReports.Count > 0);
        Assert.Contains(progressReports, p => p.State == ConversionState.Succeeded);
    }

    [Fact]
    public async Task ConvertFolderAsync_WithInvalidInputFolder_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var options = new ImageConversionOptions
        {
            InputFolder = Path.Combine(Path.GetTempPath(), "NonExistentFolder_" + Guid.NewGuid()),
            OutputFolder = _testOutputFolder,
            Quality = 85
        };

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => _service.ConvertFolderAsync(options));
    }

    [Fact]
    public async Task ConvertFolderAsync_WithEmptyOutputFolder_ThrowsArgumentException()
    {
        // Arrange
        var options = new ImageConversionOptions
        {
            InputFolder = _testInputFolder,
            OutputFolder = "",
            Quality = 85
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ConvertFolderAsync(options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task ConvertFolderAsync_WithInvalidQuality_ThrowsArgumentOutOfRangeException(int quality)
    {
        // Arrange
        var options = new ImageConversionOptions
        {
            InputFolder = _testInputFolder,
            OutputFolder = _testOutputFolder,
            Quality = quality
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _service.ConvertFolderAsync(options));
    }

    [Fact]
    public async Task ConvertFolderAsync_WithEmptyFolder_ReturnsZeroTotal()
    {
        // Arrange
        var options = new ImageConversionOptions
        {
            InputFolder = _testInputFolder,
            OutputFolder = _testOutputFolder,
            Quality = 85
        };

        // Act
        var summary = await _service.ConvertFolderAsync(options);

        // Assert
        Assert.Equal(0, summary.Total);
        Assert.Equal(0, summary.Converted);
    }

    [Fact]
    public async Task ConvertFolderAsync_SummaryContainsCorrectTimings()
    {
        // Arrange
        CreateTestImage("test.png");
        var options = new ImageConversionOptions
        {
            InputFolder = _testInputFolder,
            OutputFolder = _testOutputFolder,
            Quality = 85
        };

        // Act
        var beforeStart = DateTimeOffset.Now;
        var summary = await _service.ConvertFolderAsync(options);
        var afterComplete = DateTimeOffset.Now;

        // Assert
        Assert.True(summary.StartedAt >= beforeStart);
        Assert.True(summary.CompletedAt <= afterComplete);
        Assert.True(summary.Duration >= TimeSpan.Zero);
    }
}
