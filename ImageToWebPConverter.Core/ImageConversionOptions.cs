namespace ImageToWebPConverter.Core;

public class ImageConversionOptions
{
    public string InputFolder { get; set; } = string.Empty;
    public string OutputFolder { get; set; } = string.Empty;
    public int Quality { get; set; } = 85;
    public bool OverwriteExisting { get; set; }
    public bool IncludeSubfolders { get; set; } = true;
    public int? MaxWidth { get; set; }
    public int? MaxHeight { get; set; }
}

