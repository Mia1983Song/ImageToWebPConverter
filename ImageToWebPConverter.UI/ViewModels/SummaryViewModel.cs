using System;
using ImageToWebPConverter.Core;

namespace ImageToWebPConverter.UI.ViewModels;

public class SummaryViewModel
{
    public SummaryViewModel(ImageConversionSummary summary)
    {
        Total = summary.Total;
        Converted = summary.Converted;
        Skipped = summary.Skipped;
        Failed = summary.Failed;
        Duration = summary.Duration;
    }

    public int Total { get; }
    public int Converted { get; }
    public int Skipped { get; }
    public int Failed { get; }
    public TimeSpan Duration { get; }
}

