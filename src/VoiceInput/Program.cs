using System;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace VoiceInput;

internal static class Program
{
    private static Mutex? _mutex;

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    [STAThread]
    static void Main(string[] args)
    {
        // Single instance check using a named Mutex
        _mutex = new Mutex(true, "Global\\VoiceInputSingleInstanceMutex", out bool isNewInstance);
        if (!isNewInstance)
        {
            return;
        }

        // Setup AppDomain-wide unhandled exception handlers
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            HandleCrash(e.ExceptionObject as Exception);
        };

        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            HandleCrash(e.Exception);
            e.SetObserved();
        };

        try
        {
            global::WinRT.ComWrappersSupport.InitializeComWrappers();
            Application.Start((p) =>
            {
                try
                {
                    var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                    var context = new DispatcherQueueSynchronizationContext(dispatcherQueue);
                    SynchronizationContext.SetSynchronizationContext(context);
                    new App();
                }
                catch (Exception ex)
                {
                    HandleCrash(ex);
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            HandleCrash(ex);
        }
    }

    public static void HandleCrash(Exception? ex)
    {
        if (ex == null) return;

        string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");
        string errorDetails = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] UNHANDLED EXCEPTION\n" +
                              $"Exception Type: {ex.GetType().FullName}\n" +
                              $"Message: {ex.Message}\n" +
                              $"Source: {ex.Source}\n" +
                              $"Stack Trace:\n{ex.StackTrace}\n";

        if (ex.InnerException != null)
        {
            errorDetails += $"Inner Exception: {ex.InnerException.GetType().FullName}\n" +
                            $"Inner Message: {ex.InnerException.Message}\n" +
                            $"Inner Stack Trace:\n{ex.InnerException.StackTrace}\n";
        }
        errorDetails += new string('-', 80) + "\n";

        try
        {
            System.IO.File.AppendAllText(logPath, errorDetails);
        }
        catch
        {
            // Ignore failure to write log
        }

        string userMessage = $"程序发生未捕获的严重错误，无法继续运行。\n\n" +
                             $"错误类型: {ex.GetType().Name}\n" +
                             $"错误原因: {ex.Message}\n\n" +
                             $"详细日志已保存至: {logPath}\n\n" +
                             $"请将此日志提供给开发人员以进行排查。";

        MessageBox(IntPtr.Zero, userMessage, "VoiceInput 严重错误", 0x00000010 /* MB_ICONERROR */);
    }
}
