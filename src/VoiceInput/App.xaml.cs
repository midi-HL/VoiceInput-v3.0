using System;
using Microsoft.UI.Xaml;
using VoiceInput.Services;

namespace VoiceInput;

public partial class App : Application
{
    private MainWindow? _mainWindow;
    private KeyboardHook? _keyboardHook;
    private AudioCapture? _audioCapture;
    private Windows.HudWindow? _hudWindow;

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Helpers.Settings.Instance.Load();

        _mainWindow = new MainWindow();
        _mainWindow.Activate();

        _audioCapture = new AudioCapture();
        _keyboardHook = new KeyboardHook();

        _keyboardHook.RightAltPressed += OnRightAltPressed;
        _keyboardHook.RightAltReleased += OnRightAltReleased;
    }

    private void OnRightAltPressed()
    {
        _mainWindow?.DispatcherQueue.TryEnqueue(() =>
        {
            _hudWindow?.Close();
            _hudWindow = new Windows.HudWindow();
            _hudWindow.Activate();
            _hudWindow.StartRecording();

            _audioCapture?.StartRecording();
        });
    }

    private void OnRightAltReleased()
    {
        _mainWindow?.DispatcherQueue.TryEnqueue(async () =>
        {
            if (_audioCapture == null) return;
            
            _hudWindow?.ShowTranscribing();
            string audioBase64 = _audioCapture.StopRecording();

            if (!string.IsNullOrEmpty(audioBase64))
            {
                try
                {
                    var asrService = new MiMoAsrService();
                    string text = await asrService.RecognizeAsync(audioBase64);

                    if (Helpers.Settings.Instance.Current.LlmCorrection)
                    {
                        var refiner = new LlmRefiner();
                        text = await refiner.RefineTextAsync(text);
                    }

                    _hudWindow?.ShowResult(text);

                    await ClipboardInjector.InjectTextAsync(text, _mainWindow.DispatcherQueue);
                }
                catch (Exception ex)
                {
                    _hudWindow?.ShowError(ex.Message);
                }
            }
            else
            {
                _hudWindow?.Close();
            }
        });
    }
}
