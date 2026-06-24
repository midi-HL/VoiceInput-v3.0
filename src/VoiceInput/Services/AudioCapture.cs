using System;
using System.IO;
using NAudio.Wave;

namespace VoiceInput.Services;

public class AudioCapture : IDisposable
{
    private WaveInEvent? _waveIn;
    private MemoryStream? _outputStream;
    private WaveFileWriter? _waveWriter;
    private readonly int _sampleRate = 16000;
    private readonly int _channels = 1;

    public event Action<float>? RmsCalculated;

    public void StartRecording()
    {
        StopRecording();

        _outputStream = new MemoryStream();
        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(_sampleRate, 16, _channels)
        };

        _waveWriter = new WaveFileWriter(_outputStream, _waveIn.WaveFormat);
        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.StartRecording();
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (_waveWriter == null) return;
        _waveWriter.Write(e.Buffer, 0, e.BytesRecorded);

        float sum = 0;
        int sampleCount = e.BytesRecorded / 2;
        if (sampleCount == 0) return;

        for (int i = 0; i < e.BytesRecorded; i += 2)
        {
            if (i + 1 < e.BytesRecorded)
            {
                short sample = BitConverter.ToInt16(e.Buffer, i);
                sum += sample * sample;
            }
        }

        float rms = (float)Math.Sqrt(sum / sampleCount);
        float normalizedRms = rms / 32768.0f;
        RmsCalculated?.Invoke(normalizedRms);
    }

    public string StopRecording()
    {
        if (_waveIn != null)
        {
            _waveIn.StopRecording();
            _waveIn.DataAvailable -= OnDataAvailable;
            _waveIn.Dispose();
            _waveIn = null;
        }

        if (_waveWriter != null)
        {
            _waveWriter.Flush();
            _waveWriter.Dispose();
            _waveWriter = null;
        }

        if (_outputStream != null)
        {
            byte[] bytes = _outputStream.ToArray();
            _outputStream.Dispose();
            _outputStream = null;
            return Convert.ToBase64String(bytes);
        }

        return string.Empty;
    }

    public void Dispose()
    {
        StopRecording();
    }
}
