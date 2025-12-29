namespace ImageToWebPConverter.Core;

public enum ConversionState
{
    Pending,
    Processing,
    Succeeded,
    Skipped,
    Failed
}

public record ConversionProgress(string InputFileName, string OutputFileName, ConversionState State, string Message);

public class ImageConversionSummary
{
    public int Total { get; internal set; }
    public int Converted { get; internal set; }
    public int Skipped { get; internal set; }
    public int Failed { get; internal set; }
    public DateTimeOffset StartedAt { get; internal set; }
    public DateTimeOffset CompletedAt { get; internal set; }
    public TimeSpan Duration => CompletedAt - StartedAt;
}

