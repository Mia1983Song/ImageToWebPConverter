using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageToWebPConverter.Core;
using ImageToWebPConverter.UI.Services;

namespace ImageToWebPConverter.UI.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IDialogService _dialogService;
    private readonly IImageConversionService _service;
    private CancellationTokenSource? _cancellationTokenSource;

    private string _inputFolder = string.Empty;
    private string _outputFolder = string.Empty;
    private int _quality = 85;
    private bool _includeSubfolders = true;
    private bool _overwriteExisting;
    private string _maxWidthText = string.Empty;
    private string _maxHeightText = string.Empty;
    private bool _isBusy;
    private string _statusMessage = "待命中";
    private SummaryViewModel? _summary;

    public MainViewModel(IDialogService dialogService, IImageConversionService service)
    {
        _dialogService = dialogService;
        _service = service;

        BrowseInputCommand = new RelayCommand(_ => BrowseInput(), _ => !IsBusy);
        BrowseOutputCommand = new RelayCommand(_ => BrowseOutput(), _ => !IsBusy);
        StartCommand = new RelayCommand(async _ => await StartConversionAsync(), _ => CanStart());
        CancelCommand = new RelayCommand(_ => Cancel(), _ => IsBusy);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ConversionItemViewModel> Items { get; } = new();

    public RelayCommand BrowseInputCommand { get; }
    public RelayCommand BrowseOutputCommand { get; }
    public RelayCommand StartCommand { get; }
    public RelayCommand CancelCommand { get; }

    public string InputFolder
    {
        get => _inputFolder;
        set
        {
            if (_inputFolder != value)
            {
                _inputFolder = value;
                OnPropertyChanged(nameof(InputFolder));
                StartCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string OutputFolder
    {
        get => _outputFolder;
        set
        {
            if (_outputFolder != value)
            {
                _outputFolder = value;
                OnPropertyChanged(nameof(OutputFolder));
                StartCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public int Quality
    {
        get => _quality;
        set
        {
            if (_quality != value)
            {
                _quality = value;
                OnPropertyChanged(nameof(Quality));
            }
        }
    }

    public bool IncludeSubfolders
    {
        get => _includeSubfolders;
        set
        {
            if (_includeSubfolders != value)
            {
                _includeSubfolders = value;
                OnPropertyChanged(nameof(IncludeSubfolders));
            }
        }
    }

    public bool OverwriteExisting
    {
        get => _overwriteExisting;
        set
        {
            if (_overwriteExisting != value)
            {
                _overwriteExisting = value;
                OnPropertyChanged(nameof(OverwriteExisting));
            }
        }
    }

    public string MaxWidthText
    {
        get => _maxWidthText;
        set
        {
            if (_maxWidthText != value)
            {
                _maxWidthText = value;
                OnPropertyChanged(nameof(MaxWidthText));
            }
        }
    }

    public string MaxHeightText
    {
        get => _maxHeightText;
        set
        {
            if (_maxHeightText != value)
            {
                _maxHeightText = value;
                OnPropertyChanged(nameof(MaxHeightText));
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                BrowseInputCommand.RaiseCanExecuteChanged();
                BrowseOutputCommand.RaiseCanExecuteChanged();
                StartCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
    }

    public SummaryViewModel? Summary
    {
        get => _summary;
        private set
        {
            if (_summary != value)
            {
                _summary = value;
                OnPropertyChanged(nameof(Summary));
            }
        }
    }

    private bool CanStart() =>
        !IsBusy && Directory.Exists(InputFolder) && !string.IsNullOrWhiteSpace(OutputFolder);

    private void BrowseInput()
    {
        var path = _dialogService.PickFolder("選擇來源資料夾", InputFolder);
        if (!string.IsNullOrWhiteSpace(path))
        {
            InputFolder = path;
            if (string.IsNullOrWhiteSpace(OutputFolder))
            {
                OutputFolder = path;
            }
        }
    }

    private void BrowseOutput()
    {
        var path = _dialogService.PickFolder("選擇輸出資料夾", OutputFolder);
        if (!string.IsNullOrWhiteSpace(path))
        {
            OutputFolder = path;
        }
    }

    private async Task StartConversionAsync()
    {
        if (!Directory.Exists(InputFolder))
        {
            StatusMessage = "來源資料夾不存在。";
            return;
        }

        if (string.IsNullOrWhiteSpace(OutputFolder))
        {
            OutputFolder = InputFolder;
        }

        Directory.CreateDirectory(OutputFolder);

        Items.Clear();
        Summary = null;
        StatusMessage = "轉換中...";
        IsBusy = true;
        _cancellationTokenSource = new CancellationTokenSource();

        var progress = new Progress<ConversionProgress>(ReportProgress);

        try
        {
            var options = new ImageConversionOptions
            {
                InputFolder = InputFolder,
                OutputFolder = OutputFolder,
                Quality = Quality,
                IncludeSubfolders = IncludeSubfolders,
                OverwriteExisting = OverwriteExisting,
                MaxWidth = ParseNullableInt(MaxWidthText),
                MaxHeight = ParseNullableInt(MaxHeightText)
            };

            var summary = await _service.ConvertFolderAsync(options, progress, _cancellationTokenSource.Token);
            Summary = new SummaryViewModel(summary);
            StatusMessage = "轉換完成";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "已取消";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void Cancel()
    {
        if (IsBusy)
        {
            _cancellationTokenSource?.Cancel();
        }
    }

    private void ReportProgress(ConversionProgress progress)
    {
        var existing = Items.FirstOrDefault(item => item.InputFileName == progress.InputFileName);
        if (existing is null)
        {
            existing = new ConversionItemViewModel(progress.InputFileName, progress.OutputFileName);
            Items.Add(existing);
        }

        existing.Update(progress);
    }

    private static int? ParseNullableInt(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (int.TryParse(value, out var number) && number > 0)
        {
            return number;
        }

        return null;
    }

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

