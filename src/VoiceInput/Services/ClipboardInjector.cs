using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Windows.ApplicationModel.DataTransfer;

namespace VoiceInput.Services;

public static class ClipboardInjector
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);

    private const byte VK_CONTROL = 0x11;
    private const byte VK_V = 0x56;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    public static async Task InjectTextAsync(string text, DispatcherQueue dispatcherQueue)
    {
        if (string.IsNullOrEmpty(text)) return;

        IntPtr currentForeground = GetForegroundWindow();

        var tcs = new TaskCompletionSource<bool>();

        dispatcherQueue.TryEnqueue(async () =>
        {
            try
            {
                var dataPackageView = Clipboard.GetContent();
                string? textBackup = null;
                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    textBackup = await dataPackageView.GetTextAsync();
                }

                var dataPackage = new DataPackage();
                dataPackage.SetText(text);
                Clipboard.SetContent(dataPackage);
                Clipboard.Flush();

                if (currentForeground != IntPtr.Zero)
                {
                    SetForegroundWindow(currentForeground);
                    await Task.Delay(50);
                }

                keybd_event(VK_CONTROL, 0, 0, IntPtr.Zero);
                keybd_event(VK_V, 0, 0, IntPtr.Zero);
                keybd_event(VK_V, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, IntPtr.Zero);

                await Task.Delay(150);

                if (textBackup != null)
                {
                    var restorePackage = new DataPackage();
                    restorePackage.SetText(textBackup);
                    Clipboard.SetContent(restorePackage);
                    Clipboard.Flush();
                }
                else
                {
                    Clipboard.Clear();
                }

                tcs.SetResult(true);
            }
            catch (Exception)
            {
                tcs.SetResult(false);
            }
        });

        await tcs.Task;
    }
}
