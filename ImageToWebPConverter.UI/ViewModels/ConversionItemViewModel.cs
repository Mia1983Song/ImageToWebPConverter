using System.ComponentModel;
using ImageToWebPConverter.Core;

namespace ImageToWebPConverter.UI.ViewModels;

public class ConversionItemViewModel : INotifyPropertyChanged
{
    private ConversionState _state;
    private string _message;
    private string _outputFileName;

    public ConversionItemViewModel(string inputFileName, string outputFileName)
    {
        InputFileName = inputFileName;
        _outputFileName = outputFileName;
        _message = "待處理";
    }

    public string InputFileName { get; }

    public string OutputFileName
    {
        get => _outputFileName;
        private set
        {
            if (_outputFileName != value)
            {
                _outputFileName = value;
                OnPropertyChanged(nameof(OutputFileName));
            }
        }
    }

    public ConversionState State
    {
        get => _state;
        private set
        {
            if (_state != value)
            {
                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }
    }

    public string Message
    {
        get => _message;
        private set
        {
            if (_message != value)
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Update(ConversionProgress progress)
    {
        OutputFileName = progress.OutputFileName;
        State = progress.State;
        Message = progress.Message;
    }

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

