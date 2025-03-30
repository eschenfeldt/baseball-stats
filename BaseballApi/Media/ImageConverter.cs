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
        string resultFileName = Path.ChangeExtension(file.FullName, newExtension);

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
}
