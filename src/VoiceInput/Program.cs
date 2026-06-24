using System;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace VoiceInput;

internal static class Program
{
    private static Mutex? _mutex;

    [STAThread]
    static void Main(string[] args)
    {
        // Single instance check using a named Mutex
        _mutex = new Mutex(true, "Global\\VoiceInputSingleInstanceMutex", out bool isNewInstance);
        if (!isNewInstance)
        {
            return;
        }

        WinRT.ComWrappers.InitializeComWrappers();
        Application.Start((p) =>
        {
            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            var context = new DispatcherQueueSynchronizationContext(dispatcherQueue);
            SynchronizationContext.SetSynchronizationContext(context);
            new App();
        });
    }
}
