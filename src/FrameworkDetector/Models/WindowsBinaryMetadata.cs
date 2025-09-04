// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Models;

public record WindowsBinaryMetadata(string Filename, 
                                    string? OriginalFilename, 
                                    string? FileVersion, 
                                    string? ProductName, 
                                    string? ProductVersion) : FileMetadata(Filename)
{
    public static new async Task<WindowsBinaryMetadata?> GetMetadataAsync(string? filename, CancellationToken cancellationToken)
    {
        if (filename is null)
        {
            throw new ArgumentNullException(nameof(filename));
        }

        await Task.Yield();

        if (cancellationToken.IsCancellationRequested)
        {
            return await Task.FromCanceled<WindowsBinaryMetadata?>(cancellationToken);
        }

        var fileVersionInfo = FileVersionInfo.GetVersionInfo(filename);

        return new WindowsBinaryMetadata(Path.GetFileName(fileVersionInfo.FileName), 
            fileVersionInfo.OriginalFilename, 
            fileVersionInfo.FileVersion, 
            fileVersionInfo.ProductName, 
            fileVersionInfo.ProductVersion);
    }
}
