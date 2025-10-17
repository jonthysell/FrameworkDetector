// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace FrameworkDetector.Engine;

public class DetectorDefinition : IConfigSetupDetectorRequirements, IConfigAdditionalDetectorRequirements
{
    public IDetector Info { get; init; }

    public Dictionary<string, IDetectorCheckGroup> RequiredChecks { get; init; } = new();

    public Dictionary<string, IDetectorCheckGroup> OptionalChecks { get; init; } = new();

    public DetectorDefinition(IDetector detector)
    {
        Info = detector;
    }

    public IConfigAdditionalDetectorRequirements Required(string groupName, Func<IDetectorCheckGroup, IDetectorCheckGroup> checks)
    {
        var dcg = new DetectorCheckGroup(groupName);
        RequiredChecks.Add(groupName, checks(dcg));

        // Mark all required
        foreach (var check in dcg)
        {
            check.IsRequired = true;
            check.GroupName = groupName;
        }

        return this;
    }

    // TODO: We could define a record here of metadata about the optional check beyond just a simple string... (for now though not sure what we want here beyond a string... as I think languages and other libraries and features would just be their own dedicated detectors)
    public IConfigAdditionalDetectorRequirements Optional(string groupName, Func<IDetectorCheckGroup, IDetectorCheckGroup> checks)
    {
        var dcg = new DetectorCheckGroup(groupName);
        OptionalChecks.Add(groupName, checks(dcg));

        // Tag all metadata to the check
        foreach (var check in dcg)
        {
            check.GroupName = groupName;
        }
        
        return this;
    }

    public DetectorDefinition BuildDefinition()
    {
        return this;
    }
}