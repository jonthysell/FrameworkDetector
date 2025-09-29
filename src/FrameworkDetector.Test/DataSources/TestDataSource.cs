// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

using FrameworkDetector.DataSources;
using FrameworkDetector.Models;

namespace FrameworkDetector.Test.DataSources;

public abstract class TestDataSource<TData>(string Id, TData? TypedData) : IDataSource where TData : class
{
    public string GetId() => Id; //// Passthru

    public object? Data => TypedData;

    public Task<bool> LoadAndCacheDataAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}

public class TestProcessDataSource(ProcessMetadata? processes) : TestDataSource<ProcessMetadata>(ProcessDataSource.Id, processes), IProcessDataSource
{
    public ProcessMetadata? ProcessMetadata => processes;
}
