// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using FrameworkDetector.DataSources;
using FrameworkDetector.Engine;
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
    public Task<IDetectorCheckResult> PerformCheckAsync(IDetector detector, DataSourceCollection dataSources, CancellationToken cancellationToken);
}

/// <summary>
/// Runtime record created by a detector which links the specific check extension info (through its extension method to DetectorCheckList) with the specific metadata to be checked against by a particular detector.
/// e.g. WPF needs to look for a specific dll.
/// </summary>
/// <typeparam name="T">Type of additional information struct for storing information provided within the detector and needed to know which data in the datasource to look for. e.g. the specific module to search for.</typeparam>
/// <param name="CheckRegistration">Reference to the specific registration of the check creating this entry.</param>
/// <param name="Metadata">Additional metadata provided by a detector for this check to be passed in when executed. Included automatically within the <see cref="DetectorCheckResult{T}"/></param>
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
    async Task<IDetectorCheckResult> ICheckDefinition.PerformCheckAsync(IDetector detector, DataSourceCollection dataSources, CancellationToken cancellationToken)
    {
        // Create initial result holder linking the detector to this check being performed.
        // Auto includes the additional metadata required by the check defined by the detector (and used by the check).
        DetectorCheckResult<T> result = new(detector, this)
        {
            ExtraMetadata = Metadata
        };

        // Call the check extension to perform calculation and update result.
        await PerformCheckAsync.Invoke(this, dataSources, result, cancellationToken);

        return result;
    }

    // TODO: IsRequired and Result here too? Or do we aggregate results separately?

    public override string ToString()
    {
        return string.Format(Description, Metadata.ToString());
    }
}