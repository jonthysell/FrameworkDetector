// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.DataSources;
using FrameworkDetector.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Checks;

//// This is runtime/definition information about checks to be performed for a detector.

/// <summary>
/// Base interface for <see cref="CheckDefinition{T}"/> for common information about a performed check.
/// </summary>
public interface ICheckDefinition
{
    // TODO: Wondering if this is actually required to define by the check as currently we're just passing through all the data sources we have anyway... The check author has to grab the specific one they need anyway which they do through the IDataSource.Id right now anyway, so this is just more of a goodfaith declaration.
    public Guid[] DataSourceIds { get; }

    public string Description { get; }

    public string Name { get; }

    /// <summary>
    /// Performs the defined check against the provided <see cref="DataSourceCollection"/>.
    /// </summary>
    /// <param name="dataSources">Complete <see cref="DataSourceCollection"/> for an application.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<DetectorCheckResult> PerformCheckAsync(DataSourceCollection dataSources, CancellationToken cancellationToken);
}

/// <summary>
/// Runtime record created by a detector which links the specific check extension info (through its extension method to DetectorCheckList) with the specific metadata to be checked against by a particular detector.
/// e.g. WPF needs to look for a specific dll.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="CheckRegistration">Reference to the specific registration of the check creating this entry.</param>
/// <param name="Metadata">Additional metadata provided by a detector for this check to be passed in when executed.</param>
public record CheckDefinition<T>(
    CheckRegistrationInfo<T> CheckRegistration,
    T Metadata
) : ICheckDefinition where T : struct
{
    public string Name => CheckRegistration.Name;

    /// <summary>
    /// Description is used as a ToString format with the Metadata.ToString() as a parameter.
    /// </summary>
    public string Description => CheckRegistration.Description;

    public Guid[] DataSourceIds => CheckRegistration.DataSourceIds;

    private CheckFunction<T> PerformCheckAsync => CheckRegistration.PerformCheckAsync;

    //// Used to translate between the strongly-typed definition written by check extension author passed in as a delegate and the concreate generalized version the engine will call on the check.
    /// <inheritdoc/>
    Task<DetectorCheckResult> ICheckDefinition.PerformCheckAsync(DataSourceCollection dataSources, CancellationToken cancellationToken) => PerformCheckAsync.Invoke(this, dataSources, cancellationToken);

    // TODO: IsRequired and Result here too? Or do we aggregate results separately?

    public override string ToString()
    {
        return string.Format(Description, Metadata.ToString());
    }
}