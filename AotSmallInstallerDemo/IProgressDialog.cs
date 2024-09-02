using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace AotSmallInstallerDemo;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("EBBC7C04-315E-11d2-B62F-006097DF5BD4")]
internal unsafe partial interface IProgressDialog
{
    public const uint DLG_NORMAL = 0;
    public const uint DLG_MODAL = 1;
    public const uint DLG_AUTOTIME = 0x2;
    public const uint DLG_NOTIME = 0x4;
    public const uint DLG_NOMINIMIZE = 0x8;
    public const uint DLG_NOPROGRESSBAR = 0x10;
    public const uint DLG_MARQUEEPROGRESS = 0x20;
    public const uint DLG_NOCANCEL = 0x40;
    public const uint TIMER_RESET = 0x1;
    public const uint TIMER_PAUSE = 0x2;
    public const uint TIMER_RESUME = 0x3;

    public static Guid GUID  => new(0xEBBC7C04, 0x315E, 0x11D2, 0xB6, 0x2F, 0x00, 0x60, 0x97, 0xDF, 0x5B, 0xD4);
    public static Guid CLSID => new(0xF8383852, 0xFCD3, 0x11D1, 0xA6, 0xB9, 0x00, 0x60, 0x97, 0xDF, 0x5B, 0xD4);

    void StartProgressDialog([Optional] void* hwndParent,
        [Optional] void* punkEnableModless,
        uint dwFlags, void* pvReserved);
    void StopProgressDialog();
    void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pwzTitle);
    void SetAnimation([Optional] void* hInstAnimation, uint idAnimation);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Bool)]
    bool HasUserCancelled();
    void SetProgress(uint dwCompleted, uint dwTotal);
    void SetProgress64(ulong ullCompleted, ulong ullTotal);
    void SetLine(uint dwLineNum, [MarshalAs(UnmanagedType.LPWStr)] string pwzString, [MarshalAs(UnmanagedType.Bool)] bool fCompactPath, void* pvReserved);
    void SetCancelMsg([MarshalAs(UnmanagedType.LPWStr)] string pwzCancelMsg, void* pvReserved);
    void Timer(uint dwTimerAction, void* pvReserved);
}
