using System.IO.Compression;
using System.Runtime.InteropServices;
using TerraFX.Interop;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;

unsafe
{
    var hr = CoInitializeEx(null, (uint)COINIT.COINIT_MULTITHREADED);
    if (hr < 0) return (int)hr;
    var comInitialized = hr == 0;

    try
    {
        string targetDir;

        using (var fileDialog = CreateInstance<FileOpenDialog, IFileOpenDialog>())
        {
            FILEOPENDIALOGOPTIONS opts;
            hr = fileDialog.Get()->GetOptions((uint*)&opts);
            Marshal.ThrowExceptionForHR(hr);

            opts &= ~FILEOPENDIALOGOPTIONS.FOS_OVERWRITEPROMPT;
            opts |= FILEOPENDIALOGOPTIONS.FOS_FORCEFILESYSTEM;
            opts |= FILEOPENDIALOGOPTIONS.FOS_PICKFOLDERS;
            opts |= FILEOPENDIALOGOPTIONS.FOS_PATHMUSTEXIST;
            opts |= FILEOPENDIALOGOPTIONS.FOS_DONTADDTORECENT;

            hr = fileDialog.Get()->SetOptions((uint)opts);
            Marshal.ThrowExceptionForHR(hr);

            using ComPtr<IShellItem> shitem = default;
            fixed (char* pPath = Environment.CurrentDirectory)
                hr = SHCreateItemFromParsingName(pPath, null,
                    __uuidof(shitem.Get()), (void**)shitem.ReleaseAndGetAddressOf());
            Marshal.ThrowExceptionForHR(hr);

            hr = fileDialog.Get()->SetDefaultFolder(shitem);
            Marshal.ThrowExceptionForHR(hr);

            fixed (char* pTitle = "Select desintation folder")
                hr = fileDialog.Get()->SetTitle(pTitle);
            Marshal.ThrowExceptionForHR(hr);

            hr = fileDialog.Get()->Show(HWND.NULL);
            if (hr == HRESULT_FROM_WIN32(TerraFX.Interop.Windows.ERROR.ERROR_CANCELLED))
            {
                // cancelled, show message box to that effect then exit
                fixed (char* lpTitle = "Install cancelled")
                fixed (char* lpText = "Installation cancelled")
                    MessageBoxW(HWND.NULL, lpText, lpTitle, MB.MB_OK | MB.MB_ICONWARNING);
                return 0;
            }
            Marshal.ThrowExceptionForHR(hr);

            hr = fileDialog.Get()->GetResult(shitem.ReleaseAndGetAddressOf());
            Marshal.ThrowExceptionForHR(hr);

            char* fsName;
            hr = shitem.Get()->GetDisplayName(SIGDN.SIGDN_FILESYSPATH, &fsName);
            Marshal.ThrowExceptionForHR(hr);

            targetDir = new string(fsName);
            CoTaskMemFree(fsName);
        }

        using var dialog = CreateInstanceFromGuid<IProgressDialog>(CLSID.CLSID_ProgressDialog);

        fixed (char* str = "Extracting...") dialog.Get()->SetTitle(str);
        fixed (char* str = "Cancelling...") dialog.Get()->SetCancelMsg(str, null);
        dialog.Get()->StartProgressDialog(HWND.NULL, null, PROGDLG_AUTOTIME, null);

        Span<char> span = new char[0x4000];
        fixed (char* ptr = span)
        {
            var len = GetModuleFileNameW(HMODULE.NULL, ptr, (uint)span.Length + 1);
            span = span.Slice(0, (int)len);
        }
        var filename = span.ToString();

        using (var zipfile = ZipFile.OpenRead(filename))
        {
            var entryCount = zipfile.Entries.Count;

            for (var i = 0; i < entryCount; i++)
            {
                if (dialog.Get()->HasUserCancelled())
                {
                    // handle cancellation...
                    fixed (char* str = "Cancelling...") dialog.Get()->SetLine(1, str, false, null);
                    dialog.Get()->StartProgressDialog(HWND.NULL, null, PROGDLG_MARQUEEPROGRESS, null);
                    break;
                }

                var source = zipfile.Entries[i];

                // Note that this will give us a good DirectoryInfo even if destinationDirectoryName exists:
                var di = Directory.CreateDirectory(targetDir);
                var destinationDirectoryFullPath = di.FullName;
                if (!destinationDirectoryFullPath.EndsWith(Path.DirectorySeparatorChar))
                {
                    char sep = Path.DirectorySeparatorChar;
                    destinationDirectoryFullPath = string.Concat(destinationDirectoryFullPath, new ReadOnlySpan<char>(in sep));
                }

                var fileDestinationPath = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, source.FullName.Replace('\0', '_')));

                if (!fileDestinationPath.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    fixed (char* lpTitle = "Error")
                    fixed (char* lpText = "Archive is broken...")
                        MessageBoxW(HWND.NULL, lpText, lpTitle, MB.MB_OK | MB.MB_ICONERROR);
                    continue;
                }

                fixed (char* str = $"Extracting {destinationDirectoryFullPath}")
                    dialog.Get()->SetLine(1, str, true, null);
                fixed (char* str = fileDestinationPath)
                    dialog.Get()->SetLine(2, str, true, null);
                dialog.Get()->SetProgress((uint)i, (uint)entryCount);

                if (Path.GetFileName(fileDestinationPath).Length == 0)
                {
                    // If it is a directory:
                    if (source.Length != 0)
                        throw new IOException("Directory has data");

                    Directory.CreateDirectory(fileDestinationPath);
                }
                else
                {
                    // If it is a file:
                    // Create containing directory:
                    Directory.CreateDirectory(Path.GetDirectoryName(fileDestinationPath)!);
                    source.ExtractToFile(fileDestinationPath, overwrite: true);
                }
            }

            dialog.Get()->SetProgress((uint)entryCount, (uint)entryCount);
        }

        dialog.Get()->StopProgressDialog();
        dialog.Dispose();

        fixed (char* lpTitle = "Extraction complete")
        fixed (char* lpText = "Done!")
            MessageBoxW(HWND.NULL, lpText, lpTitle, MB.MB_OK | MB.MB_ICONINFORMATION);
    }
    catch (Exception e)
    {
        fixed (char* lpTitle = "An exception occurred")
        fixed (char* lpText = e.ToString())
            MessageBoxW(HWND.NULL, lpText, lpTitle, MB.MB_OK | MB.MB_ICONERROR);
        throw;
    }
    finally
    {
        if (comInitialized)
        {
            CoUninitialize();
        }
    }
}

return 0;

static unsafe ComPtr<T> CreateInstanceFromGuid<T>(Guid clsid)
    where T : unmanaged, INativeGuid, IUnknown.Interface
{
    ComPtr<T> ptr = default;
    var hr = CoCreateInstance(
        &clsid, null,
        (uint)CLSCTX.CLSCTX_INPROC_SERVER,
        Windows.__uuidof<T>(), (void**)ptr.ReleaseAndGetAddressOf());
    Marshal.ThrowExceptionForHR(hr.Value);
    return ptr;
}

static ComPtr<TI> CreateInstance<T, TI>()
    where TI : unmanaged, INativeGuid, IUnknown.Interface
    where T : unmanaged, INativeGuid
{
    return CreateInstanceFromGuid<TI>(Windows.__uuidof<T>());
}