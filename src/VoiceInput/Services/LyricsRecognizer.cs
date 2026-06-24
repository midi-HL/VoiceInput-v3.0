using System;
using System.IO;
using System.Threading.Tasks;

namespace VoiceInput.Services;

public class LyricsRecognizer
{
    private readonly MiMoAsrService _asrService;

    public LyricsRecognizer()
    {
        _asrService = new MiMoAsrService();
    }

    public async Task<string> RecognizeLyricsAsync(string filePath, string language = "auto", string? customApiKey = null)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Audio file not found.", filePath);
        }

        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > 10 * 1024 * 1024)
        {
            throw new InvalidOperationException("File size exceeds 10MB limit.");
        }

        byte[] bytes = await File.ReadAllBytesAsync(filePath);
        string base64 = Convert.ToBase64String(bytes);
        string mimeType = filePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ? "audio/wav" : "audio/mpeg";

        string transcript = await _asrService.RecognizeAsync(base64, mimeType, language, customApiKey);

        if (transcript.Contains("[00:") || transcript.Contains("[01:"))
        {
            return transcript;
        }

        return FormatAsLrc(transcript);
    }

    private string FormatAsLrc(string rawText)
    {
        var lines = rawText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[ti:Recognized Lyrics]");
        sb.AppendLine("[ar:VoiceInput App]");
        sb.AppendLine("[al:MiMo ASR Transcript]");
        sb.AppendLine("[by:VoiceInput]");

        double currentTimeSec = 1.5;
        foreach (var line in lines)
        {
            int min = (int)(currentTimeSec / 60);
            double sec = currentTimeSec % 60;
            sb.AppendLine($"[{min:00}:{sec:00.00}]{line.Trim()}");
            currentTimeSec += Math.Max(2.5, line.Length * 0.3); 
        }

        return sb.ToString();
    }
}
