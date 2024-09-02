using System.Runtime.InteropServices;

namespace AotSmallInstallerDemo;

internal static unsafe partial class Interop
{
    public const string Ole32 = "ole32";
    public const string Kernel32 = "kernel32";
    public const string User32 = "User32";

    [LibraryImport(Kernel32, EntryPoint = "GetModuleFileNameW", SetLastError = true)]
    public static partial uint GetModuleFileNameW(void* hModule, char* lpFilename, uint nSize);

    public const uint MB_ABORTRETRYIGNORE = 0x2;
    public const uint MB_CANCELTRYCONTINUE = 0x6;
    public const uint MB_HELP = 0x4000;
    public const uint MB_OK = 0x0;
    public const uint MB_OKCANCEL = 0x1;
    public const uint MB_RETRYCANCEL = 0x5;
    public const uint MB_YESNO = 0x4;
    public const uint MB_YESNOCANCEL = 0x3;

    public const uint MB_ICONWARNING = 0x30;
    public const uint MB_INFORMATION = 0x40;
    public const uint MB_ICONQUESTION = 0x20;
    public const uint MB_ICONERROR = 0x10;

    public const int IDABORT = 3;
    public const int IDCANCEL = 2;
    public const int IDCONTINUE = 11;
    public const int IDIGNORE = 5;
    public const int IDNO = 7;
    public const int IDOK = 1;
    public const int IDRETRY = 4;
    public const int IDTRYAGAIN = 10;
    public const int IDYES = 6;

    [LibraryImport(Kernel32, EntryPoint = "MessageBoxW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial int MessageBoxW(void* hwnd,
        [MarshalAs(UnmanagedType.LPWStr)] string? lpText,
        [MarshalAs(UnmanagedType.LPWStr)] string? lpCaption,
        uint uType);

    public const uint COINIT_APARTMENTTHREADED = 0x2;
    public const uint COINIT_MULTITHREADED = 0x0;
    public const uint COINIT_DISABLE_OLE1DDE = 0x4;
    public const uint COINIT_SPEED_OVER_MEMORY = 0x8;

    [LibraryImport(Ole32, EntryPoint = "CoInitializeEx")]
    public static partial uint CoInitializeEx([Optional] void* pvReserved, uint dwCoInit);
    [LibraryImport(Ole32, EntryPoint = "CoUninitialize")]
    public static partial void CoUninitialize();

    [LibraryImport(Ole32, EntryPoint = "CoCreateInstance")]
    public static partial uint CoCreateInstance(in Guid rclsid, void* pUnkOuter, uint dwClsContext, in Guid riid, out void* ppv);

}
