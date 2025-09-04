// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.Models;

namespace FrameworkDetector.DataSources;

public class ProcessDataSource : IDataSource
{
    public static Guid Id => new Guid("9C719E0C-2E53-4379-B2F5-C90F47E6C730");

    public Guid GetId() => Id; //// Passthru

    public object? Data => ProcessMetadata;

    public ProcessMetadata? ProcessMetadata { get; private set; } = null;

    internal Process Process { get; private set; }

    public ProcessDataSource(Process process)
    {
        Process = process;
    }

    public async Task<bool> LoadAndCacheDataAsync(CancellationToken cancellationToken)
    {
        ProcessMetadata = await ProcessMetadata.GetMetadataAsync(Process, cancellationToken);

        return true;
    }
}
