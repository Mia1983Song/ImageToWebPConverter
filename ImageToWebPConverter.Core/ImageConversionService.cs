using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ImageToWebPConverter.Core;

public class ImageConversionService : IImageConversionService
{
    private static readonly string[] SupportedExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".tiff" };

    public Task<ImageConversionSummary> ConvertFolderAsync(
        ImageConversionOptions options,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ValidateOptions(options);

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var summary = new ImageConversionSummary
            {
                StartedAt = DateTimeOffset.Now
            };

            var searchOption = options.IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = SupportedExtensions
                .SelectMany(ext => Directory.GetFiles(options.InputFolder, $"*{ext}", searchOption))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            summary.Total = files.Count;

            Directory.CreateDirectory(options.OutputFolder);

            foreach (var filePath in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var relativePath = Path.GetRelativePath(options.InputFolder, filePath);
                var inputDisplay = relativePath.Replace(Path.DirectorySeparatorChar, '/');
                var outputRelativePath = Path.ChangeExtension(relativePath, ".webp");
                var outputDisplay = outputRelativePath.Replace(Path.DirectorySeparatorChar, '/');
                var outputPath = Path.Combine(options.OutputFolder, outputRelativePath);
                var outputDirectory = Path.GetDirectoryName(outputPath);

                if (!string.IsNullOrEmpty(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                if (!options.OverwriteExisting && File.Exists(outputPath))
                {
                    summary.Skipped++;
                    progress?.Report(new ConversionProgress(inputDisplay, outputDisplay, ConversionState.Skipped, "檔案已存在，略過"));
                    continue;
                }

                progress?.Report(new ConversionProgress(inputDisplay, outputDisplay, ConversionState.Processing, "轉換中"));

                try
                {
                    ConvertFile(filePath, outputPath, options);
                    summary.Converted++;
                    progress?.Report(new ConversionProgress(inputDisplay, outputDisplay, ConversionState.Succeeded, "完成"));
                }
                catch (Exception ex)
                {
                    summary.Failed++;
                    progress?.Report(new ConversionProgress(inputDisplay, outputDisplay, ConversionState.Failed, ex.Message));
                }
            }

            summary.CompletedAt = DateTimeOffset.Now;
            return summary;
        }, cancellationToken);
    }

    private static void ValidateOptions(ImageConversionOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.InputFolder) || !Directory.Exists(options.InputFolder))
        {
            throw new DirectoryNotFoundException("來源資料夾不存在。");
        }

        if (string.IsNullOrWhiteSpace(options.OutputFolder))
        {
            throw new ArgumentException("輸出資料夾路徑不可為空白。", nameof(options));
        }

        if (options.Quality is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(options.Quality), "品質必須介於 1 到 100。");
        }
    }

    private static void ConvertFile(string inputPath, string outputPath, ImageConversionOptions options)
    {
        using var image = Image.Load(inputPath);

        ResizeIfNeeded(image, options);

        var encoder = new WebpEncoder
        {
            Quality = options.Quality,
            Method = WebpEncodingMethod.Default
        };

        image.Save(outputPath, encoder);
    }

    private static void ResizeIfNeeded(Image image, ImageConversionOptions options)
    {
        if (options.MaxWidth is null && options.MaxHeight is null)
        {
            return;
        }

        var maxWidth = options.MaxWidth ?? image.Width;
        var maxHeight = options.MaxHeight ?? image.Height;

        var ratioX = (double)maxWidth / image.Width;
        var ratioY = (double)maxHeight / image.Height;
        var ratio = Math.Min(ratioX, ratioY);

        if (ratio >= 1)
        {
            return;
        }

        var targetWidth = Math.Max(1, (int)Math.Round(image.Width * ratio));
        var targetHeight = Math.Max(1, (int)Math.Round(image.Height * ratio));

        image.Mutate(ctx => ctx.Resize(targetWidth, targetHeight));
    }
}

