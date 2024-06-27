namespace BaseballApi;

public class CsvLoader
{
    string FilePath { get; }
    public bool IsLoaded { get; private set; } = false;
    public List<string>? Headers { get; private set; }
    List<List<string>> Rows { get; } = new List<List<string>>();

    public CsvLoader(string filePath)
    {
        this.FilePath = filePath;
    }

    public void LoadData()
    {
        int lineNum = 0;
        foreach (string line in File.ReadAllLines(this.FilePath))
        {
            List<string> cells = line.Split(',', StringSplitOptions.TrimEntries)
                                    .Select(h => h.Trim('"')).ToList();
            if (lineNum == 0)
            {
                this.Headers = cells;
            }
            else
            {
                this.Rows.Add(cells);
            }
            lineNum++;
        }
        this.IsLoaded = true;
    }
}
