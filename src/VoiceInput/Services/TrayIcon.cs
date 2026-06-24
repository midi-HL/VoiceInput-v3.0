using System;
using System.Runtime.InteropServices;
using System.Drawing;

namespace VoiceInput;

public class TrayIcon : IDisposable
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

    private const uint NIM_ADD = 0x00000000;
    private const uint NIM_MODIFY = 0x00000001;
    private const uint NIM_DELETE = 0x00000002;

    private const uint NIF_MESSAGE = 0x00000001;
    private const uint NIF_ICON = 0x00000002;
    private const uint NIF_TIP = 0x00000004;

    private const uint WM_USER = 0x0400;
    public const uint WM_TRAYICON = WM_USER + 101;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public uint dwState;
        public uint dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public uint uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public uint dwInfoFlags;
        public Guid guidItem;
        public IntPtr hBalloonIcon;
    }

    private readonly IntPtr _hwnd;
    private readonly uint _id;
    private bool _added;

    public TrayIcon(IntPtr hwnd, uint id, string tooltip)
    {
        _hwnd = hwnd;
        _id = id;
        CreateTrayIcon(tooltip);
    }

    private void CreateTrayIcon(string tooltip)
    {
        IntPtr hIcon = LoadDefaultIcon();

        var nid = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = _hwnd,
            uID = _id,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = WM_TRAYICON,
            hIcon = hIcon,
            szTip = tooltip
        };

        _added = Shell_NotifyIcon(NIM_ADD, ref nid);
    }

    private IntPtr LoadDefaultIcon()
    {
        try
        {
            using Icon icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location) 
                               ?? SystemIcons.Application;
            return icon.Handle;
        }
        catch
        {
            return SystemIcons.Application.Handle;
        }
    }

    public void UpdateTooltip(string tooltip)
    {
        if (!_added) return;

        var nid = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = _hwnd,
            uID = _id,
            uFlags = NIF_TIP,
            szTip = tooltip
        };

        Shell_NotifyIcon(NIM_MODIFY, ref nid);
    }

    public void Dispose()
    {
        if (_added)
        {
            var nid = new NOTIFYICONDATA
            {
                cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                hWnd = _hwnd,
                uID = _id
            };
            Shell_NotifyIcon(NIM_DELETE, ref nid);
            _added = false;
        }
    }
}
