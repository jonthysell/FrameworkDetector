// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FrameworkDetector.Checks;
using System;

namespace FrameworkDetector.Checks;

//// This is runtime/definition information about checks to be performed for a detector.

/// <summary>
/// Base interface for <see cref="CheckDefinition{T}"/> for common information about a performed check.
/// </summary>
public interface ICheckDefinition
{
    public Guid[] DataSourceIds { get; }

    public string Description { get; }
    public string Name { get; }
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

    public string Description => CheckRegistration.Description;

    public Guid[] DataSourceIds => CheckRegistration.DataSourceIds;

    public CheckFunction<T> PerformCheckAsync => CheckRegistration.PerformCheckAsync;

    // TODO: IsRequired and Result here too? Or do we aggregate results separately?
}