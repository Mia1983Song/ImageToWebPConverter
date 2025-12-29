namespace ImageToWebPConverter.Core;

public interface IImageConversionService
{
    Task<ImageConversionSummary> ConvertFolderAsync(
        ImageConversionOptions options,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
