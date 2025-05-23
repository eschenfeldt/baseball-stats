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
        string convertedFileName = Path.Join(Path.GetTempPath(), Path.ChangeExtension(original.Name, ".mp4"));

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
        process.WaitForExit();

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

    public FileInfo CreateJpeg(FileInfo file)
    {
        string resultFileName = Path.Join(Path.GetTempPath(), Path.ChangeExtension(file.Name, ".jpeg"));

        using Process process = new();
        process.StartInfo.FileName = "ffmpeg";
        process.StartInfo.Arguments = $"-i \"{file.FullName}\" -ss 00:00:00.000 -vframes 1 \"{resultFileName}\" -y -hide_banner -loglevel error";
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
            throw new Exception($"ffmpeg failed with exit code {process.ExitCode}: {error}");
        }
        else
        {
            return new FileInfo(resultFileName);
        }
    }
}
