using System.Diagnostics;

namespace Utils;

public class FfmpegHelper
{
    public static void ConvertTs2Mp4(string inputFile, string outputFile)
    {
        var ffmpegPath = "ffmpeg"; // 如果 ffmpeg 加入了系统 PATH
        // 或者你也可以写绝对路径，如："C:\\Tools\\ffmpeg.exe"

        var arguments = $"-i \"{inputFile}\"  -c copy -movflags faststart \"{outputFile}\"";

        using (var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true, // ffmpeg 会把日志输出到 stderr
                UseShellExecute = false,
                CreateNoWindow = true
            }
        })
        {

            process.Start();

            string stderr = process.StandardError.ReadToEnd(); // 可用于日志输出
            process.WaitForExit();

            Console.WriteLine("FFmpeg执行完成！");
            Console.WriteLine(stderr); // 打印 FFmpeg 日志
        }
    }
    public static void ExportTs(string inputFile, string outputDir)
    {

        if (!System.IO.Directory.Exists(outputDir))
        {
            System.IO.Directory.CreateDirectory(outputDir);
        }


        var ffmpegPath = "ffmpeg"; // 如果 ffmpeg 加入了系统 PATH
        // 或者你也可以写绝对路径，如："C:\\Tools\\ffmpeg.exe"

        var arguments = $"-i \"{inputFile}\"   -codec: copy" +
            $" -start_number 0" +
            $" -hls_time 10" +
            $" -hls_list_size 0 " +
           // $" -hls_segment_filename \"index_%03d.ts\"" +
            $" \"{outputDir}/index.m3u8\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true, // ffmpeg 会把日志输出到 stderr
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        string stderr = process.StandardError.ReadToEnd(); // 可用于日志输出
        process.WaitForExit();

        Console.WriteLine("FFmpeg执行完成！");
        Console.WriteLine(stderr); // 打印 FFmpeg 日志
    }
}
