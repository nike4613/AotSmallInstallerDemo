using AotSmallInstallerDemo;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.Marshalling;

unsafe
{
    var hr = Interop.CoInitializeEx(null, Interop.COINIT_MULTITHREADED);
    if (hr < 0) return (int)hr;
    var comInitialized = hr == 0;

    try
    {
        var cw = new StrategyBasedComWrappers();
        hr = Interop.CoCreateInstance(IProgressDialog.CLSID, null, 0x1, IProgressDialog.GUID, out var comPtr);
        if (hr != 0) return (int)hr;
        var dialog = (IProgressDialog)cw.GetOrCreateObjectForComInstance((nint)comPtr, System.Runtime.InteropServices.CreateObjectFlags.None);

        dialog.SetTitle("Extracting...");
        dialog.SetCancelMsg("Cancelling...", null);
        dialog.StartProgressDialog(null, null, IProgressDialog.DLG_AUTOTIME, null);

        Span<char> span = new char[0x4000];
        fixed (char* ptr = span)
        {
            var len = Interop.GetModuleFileNameW(null, ptr, (uint)span.Length + 1);
            span = span.Slice(0, (int)len);
        }

        var filename = span.ToString();
        var targetDir = Path.Combine(Environment.CurrentDirectory, Path.GetFileNameWithoutExtension(filename));

        using (var zipfile = ZipFile.OpenRead(filename))
        {
            var entryCount = zipfile.Entries.Count;

            for (var i = 0; i < entryCount; i++)
            {
                if (dialog.HasUserCancelled())
                {
                    // handle cancellation...
                    dialog.SetLine(1, "Cancelling...", false, null);
                    dialog.StartProgressDialog(null, null, IProgressDialog.DLG_MARQUEEPROGRESS, null);
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
                    Interop.MessageBoxW(null, "Archive is broken...", "Error", Interop.MB_OK | Interop.MB_ICONERROR);
                    continue;
                }

                dialog.SetLine(1, $"Extracting {destinationDirectoryFullPath}", true, null);
                dialog.SetLine(2, fileDestinationPath, true, null);
                dialog.SetProgress((uint)i, (uint)entryCount);

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

            dialog.SetProgress((uint)entryCount, (uint)entryCount);
        }

        dialog.StopProgressDialog();

        Interop.MessageBoxW(null, "Done!", "Extraction complete", Interop.MB_OK | Interop.MB_INFORMATION);
    }
    catch (Exception e)
    {
        Interop.MessageBoxW(null, e.ToString(), "An exception occurred", Interop.MB_OK | Interop.MB_ICONERROR);
        throw;
    }
    finally
    {
        if (comInitialized)
        {
            Interop.CoUninitialize();
        }
    }
}

return 0;