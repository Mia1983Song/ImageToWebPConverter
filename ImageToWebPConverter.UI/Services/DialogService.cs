using System.IO;
using System.Windows.Forms;

namespace ImageToWebPConverter.UI.Services;

public class DialogService : IDialogService
{
    public string? PickFolder(string title, string initialPath)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = title,
            UseDescriptionForTitle = true
        };

        if (!string.IsNullOrWhiteSpace(initialPath) && Directory.Exists(initialPath))
        {
            dialog.SelectedPath = initialPath;
        }

        return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
    }
}

