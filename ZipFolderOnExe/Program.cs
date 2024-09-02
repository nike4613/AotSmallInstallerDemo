using System.IO.Compression;

if (args is not [{ }  exe, { } dir, { } outExe, ..])
{
    Console.Error.WriteLine("Usage: ZipFolderOnExe <sourceExe> <zipDir> <outExe>");
    return 1;
}

using var outStream = File.OpenWrite(outExe);

using (var exeStream = File.OpenRead(exe))
{
    exeStream.CopyTo(outStream);
}

using var zip = new ZipArchive(outStream, ZipArchiveMode.Create);
foreach (var entry in Directory.EnumerateFileSystemEntries(dir, "*", SearchOption.AllDirectories))
{
    var relativePath = Path.GetRelativePath(dir, entry);
    relativePath = relativePath.Replace(Path.PathSeparator, '/');

    if (File.Exists(entry))
    {
        // this is a file
        zip.CreateEntryFromFile(entry, relativePath, CompressionLevel.SmallestSize);
    }
    else if (Directory.Exists(entry))
    {
        if (!relativePath.EndsWith("/"))
        {
            relativePath += "/";
        }

        var di = new DirectoryInfo(entry);
        if (!di.EnumerateFileSystemInfos().Any())
        {
            // empty directory, add an entry
            _ = zip.CreateEntry(relativePath, CompressionLevel.SmallestSize);
        }
    }
}

return 0;