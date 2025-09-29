// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.Models;

namespace FrameworkDetector.DataSources;

public interface IProcessDataSource : IDataSource
{
    ProcessMetadata? ProcessMetadata { get; }
}

public class ProcessDataSource : IProcessDataSource
{
    public static string Id => "processes";

    public string GetId() => Id; //// Passthru

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
