using System;
using System.Diagnostics;
using System.Text.Json;

namespace BaseballApi.Media;

public class VideoConverter
{
    public VideoInfo GetVideoInfo(FileInfo file)
    {
        using Process process = new();
        process.StartInfo.FileName = "ffprobe";
        process.StartInfo.Arguments = $"-v error -show_streams -show_format -print_format json \"{file.FullName}\"";
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
            throw new Exception($"ffprobe failed with exit code {process.ExitCode}: {error}");
        }

        var result = JsonSerializer.Deserialize<VideoInfo>(output);
        if (result != null)
        {
            return result;
        }
        else
        {
            throw new Exception("Failed to parse ffprobe output");
        }
    }

    public FileInfo ConvertVideo(FileInfo original)
    {
        string convertedFileName = Path.ChangeExtension(original.FullName, ".mp4");

        using Process process = new();
        process.StartInfo.FileName = "ffmpeg";
        process.StartInfo.Arguments = $"-y -i \"{original.FullName}\" -v error -c:v libx264 \"{convertedFileName}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;

        string error = string.Empty;
        process.StartInfo.RedirectStandardError = true;
        process.ErrorDataReceived += (sender, e) => error += e.Data;
        process.Start();

        process.BeginErrorReadLine();

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit(TimeSpan.FromMinutes(1));

        if (!process.HasExited)
        {
            process.Kill();
            throw new Exception("ffmpeg took too long to convert the video");
        }
        else if (process.ExitCode != 0)
        {
            throw new Exception($"ffmpeg failed with exit code {process.ExitCode}: {error}");
        }
        return new FileInfo(convertedFileName);
    }
}
