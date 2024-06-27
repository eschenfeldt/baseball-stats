using BaseballApi.Models;

namespace BaseballApi;

public class GameImportManager
{
    GameImportData Data { get; }
    Dictionary<ImportFileType, CsvLoader> Files { get; }
    public GameImportManager(GameImportData data)
    {
        this.Data = data;
        this.Files = new Dictionary<ImportFileType, CsvLoader>();
    }

    public Game GetGame()
    {
        throw new NotImplementedException();
    }

    private CsvLoader GetOrLoadFile(ImportFileType fileType)
    {
        if (this.Files.TryGetValue(fileType, out CsvLoader? file))
        {
            return file;
        }
        else if (this.Data.FilePaths.TryGetValue(fileType.ExpectedFileName(), out string? filePath))
        {
            var loader = new CsvLoader(filePath);
            loader.LoadData();
            this.Files[fileType] = loader;
            return loader;
        }
        else
        {
            throw new ArgumentException($"No '{fileType.ExpectedFileName()}' file found");
        }
    }
}
