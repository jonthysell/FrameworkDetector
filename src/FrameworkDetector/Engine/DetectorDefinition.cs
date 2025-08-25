using FrameworkDetector.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrameworkDetector.Engine;

public class DetectorDefinition
{
    public IDetector Detector { get; init; }

    public List<IDetectorCheck> RequiredChecks { get; init; } = new();

    public List<IDetectorCheck> OptionalChecks { get; init; } = new();

    public DetectorDefinition(IDetector detector)
    {
        Detector = detector;
    }

    public async Task<DetectorStatus> DetectAsync(DataSources.IDataSource source)
    {
        throw new NotImplementedException();
    }   
}