// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.DataSources;
using FrameworkDetector.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Checks;

//// This is extension/registration information about a check for its definition.

/// <summary>
/// Record of static registration of a Check extension. Provides all the details the engine needs to provide in terms of identify within the check results, as well as the required data sources this check needs to operate. Finally, points to the check function the engine will call to perform the check.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Name"></param>
/// <param name="Description"></param>
/// <param name="DataSourceIds"><see cref="IDataSource.Id"/> static ids to identify the required data source info needed to perform this check (TODO: this probably should be a source generated registry in the future maybe?)</param>
/// <param name="PerformCheckAsync"><see cref="CheckFunction{T}"/> delegate for signature of function called by the detector engine(tbd?) to perform the check against the provided data source.</param>
public record CheckRegistrationInfo<T>(
    string Name,
    string Description,
    Guid[] DataSourceIds,
    CheckFunction<T> PerformCheckAsync
) where T : struct
{
}

// TODO: Maybe have a helper/wrapper class around the datasource dictionary? i.e. have a helper which takes in the ids and returns the strongly typed datasource (or throws error if mismatch).
// TODO: The index of datasources should be a source generator for better type safety and performance.
public delegate Task<DetectorCheckResult> CheckFunction<T>(T info, DataSourceCollection dataSources, CancellationToken ct) where T : struct;