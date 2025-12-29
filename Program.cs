using ImageToWebPConverter.Core;

namespace ImageToWebPConverter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== 批量圖片轉WebP工具 ===");

            // 獲取輸入資料夾路徑
            Console.Write("請輸入來源資料夾路徑: ");
            string inputFolder = Console.ReadLine() ?? string.Empty;

            if (!Directory.Exists(inputFolder))
            {
                Console.WriteLine("錯誤: 資料夾不存在!");
                Console.ReadKey();
                return;
            }

            // 獲取輸出資料夾路徑
            Console.Write("請輸入輸出資料夾路徑 (留空則使用來源資料夾): ");
            string outputFolder = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                outputFolder = inputFolder;
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            // 設定品質
            Console.Write("請輸入WebP品質 (1-100, 預設85): ");
            string qualityInput = Console.ReadLine() ?? string.Empty;
            int quality = 85;

            if (!string.IsNullOrWhiteSpace(qualityInput))
            {
                if (!int.TryParse(qualityInput, out quality) || quality < 1 || quality > 100)
                {
                    quality = 85;
                    Console.WriteLine("品質設定無效，使用預設值 85");
                }
            }

            bool includeSubfolders = AskYesNo("是否包含子資料夾? (Y/n): ", defaultYes: true);
            bool overwrite = AskYesNo("若輸出檔案存在是否覆寫? (y/N): ", defaultYes: false);

            int? maxWidth = AskOptionalInt("最大寬度 (像素, 留空不限制): ");
            int? maxHeight = AskOptionalInt("最大高度 (像素, 留空不限制): ");

            // 開始轉換
            var options = new ImageConversionOptions
            {
                InputFolder = inputFolder,
                OutputFolder = outputFolder,
                Quality = quality,
                IncludeSubfolders = includeSubfolders,
                OverwriteExisting = overwrite,
                MaxWidth = maxWidth,
                MaxHeight = maxHeight
            };

            var service = new ImageConversionService();
            var progress = new Progress<ConversionProgress>(p =>
            {
                Console.WriteLine($"{p.InputFileName} -> {p.OutputFileName}: {p.State} - {p.Message}");
            });

            try
            {
                var summary = await service.ConvertFolderAsync(options, progress);
                Console.WriteLine($"\n轉換統計:");
                Console.WriteLine($"總計: {summary.Total}");
                Console.WriteLine($"成功: {summary.Converted}");
                Console.WriteLine($"略過: {summary.Skipped}");
                Console.WriteLine($"失敗: {summary.Failed}");
                Console.WriteLine($"耗時: {summary.Duration}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"轉換失敗: {ex.Message}");
            }

            Console.WriteLine("\n轉換完成! 按任意鍵退出...");
            Console.ReadKey();
        }

        static bool AskYesNo(string prompt, bool defaultYes)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultYes;
            }

            return input.Trim().Equals("y", StringComparison.OrdinalIgnoreCase);
        }

        static int? AskOptionalInt(string prompt)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (int.TryParse(input, out var value) && value > 0)
            {
                return value;
            }

            Console.WriteLine("輸入無效，將不套用限制。");
            return null;
        }
    }
}
