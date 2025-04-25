using System.Diagnostics;
using System.Text.Json;

namespace BaseballApi.Media;

public class ImageConverter
{
    public ImageInfo GetImageInfo(FileInfo file)
    {
        using Process process = new();
        process.StartInfo.FileName = "magick";
        // process.StartInfo.Arguments = $"identify -ping -format \'{{\\\"Extension\\\":\\\"%e\\\",\"Height\\\":%h,\\\"Width\\\":%h}}\' \"{file.FullName}\"";
        process.StartInfo.Arguments = $"identify -ping -format \"%e %w %h\" \"{file.FullName}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;

        string error = string.Empty;
        process.StartInfo.RedirectStandardError = true;
        process.ErrorDataReceived += (sender, e) => error += e.Data;
        process.Start();

        process.BeginErrorReadLine();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"magick failed with exit code {process.ExitCode}: {error}");
        }

        var parts = output.Split(' ');
        if (parts.Length != 3)
        {
            throw new Exception($"Unexpected output format: {output}");
        }
        else
        {
            return new ImageInfo
            {
                Extension = parts[0],
                Width = int.Parse(parts[1]),
                Height = int.Parse(parts[2])
            };
        }
    }

    public FileInfo CreateJpeg(FileInfo file, ThumbnailSize? size)
    {
        string newExtension = size == null ? ".jpeg" : $"_{size.Modifier}.jpeg";
        string resultFileName = Path.Join(Path.GetTempPath(), Path.ChangeExtension(file.Name, newExtension));

        using Process process = new();
        process.StartInfo.FileName = "magick";
        string command = size == null ? "" : $"-thumbnail {size.MaxSize}x{size.MaxSize}\\>";
        process.StartInfo.Arguments = $"\"{file.FullName}\" {command} \"{resultFileName}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;

        string error = string.Empty;
        process.StartInfo.RedirectStandardError = true;
        process.ErrorDataReceived += (sender, e) => error += e.Data;
        process.Start();

        process.BeginErrorReadLine();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"magick failed with exit code {process.ExitCode}: {error}");
        }
        else
        {
            return new FileInfo(resultFileName);
        }
    }

    public ExifInfo GetExifInfo(FileInfo file)
    {
        using Process process = new();
        process.StartInfo.FileName = "exiftool";
        process.StartInfo.Arguments = $"exiftool -json -creationdate -datetimeoriginal -createdate -offsettime -offsettimeoriginal  \"{file.FullName}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;

        string error = string.Empty;
        process.StartInfo.RedirectStandardError = true;
        process.ErrorDataReceived += (sender, e) => error += e.Data;
        process.Start();

        process.BeginErrorReadLine();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        // if (process.ExitCode != 0)
        // {
        //     throw new Exception($"exiftool failed with exit code {process.ExitCode}: {error}");
        // }

        var results = JsonSerializer.Deserialize<RawExif[]>(output);
        if (results == null)
        {
            throw new Exception($"Failed to parse exiftool output: {output}");
        }
        else if (results.Length == 1)
        {
            var result = results[0];
            // CreationDate seems to exist for video and is better when it's there
            string? rawDate = result.CreationDate ?? result.DateTimeOriginal ?? result.CreateDate;
            if (rawDate == null)
            {
                throw new Exception($"No date found in exiftool output: {output}");
            }
            else if (rawDate.Contains('-') || rawDate.Contains('Z'))
            {
                // offset included already
                DateTimeOffset creationDate = DateTimeOffset.ParseExact(rawDate, "yyyy:MM:dd HH:mm:sszzz", null);
                return new ExifInfo
                {
                    CreationDate = creationDate
                };
            }
            else
            {
                var offset = result.OffsetTimeOriginal ?? result.OffsetTime;
                rawDate += offset;
                DateTimeOffset creationDate = DateTimeOffset.ParseExact(rawDate, "yyyy:MM:dd HH:mm:sszzz", null);
                return new ExifInfo
                {
                    CreationDate = creationDate
                };
            }
        }
        else
        {
            throw new Exception($"Unexpected exiftool output: {output}");
        }
    }

    class RawExif
    {
        public required string SourceFile { get; set; }
        public string? DateTimeOriginal { get; set; }
        public string? CreateDate { get; set; }
        public string? CreationDate { get; set; }
        public string? OffsetTime { get; set; }
        public string? OffsetTimeOriginal { get; set; }
    }
}
