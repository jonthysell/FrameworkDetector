// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector;

public interface IDetector
{
    string Name { get; }

    string Description { get; }

    string FrameworkId { get; }

    DetectorResult Result { get; }
}

public interface IDetectorByProcess : IDetector
{
    Task<DetectorStatus> DetectByProcessAsync(Process process, CancellationToken cancellationToken);
}

public interface IDetectorByPath : IDetector
{
    Task<DetectorStatus> DetectByPathAsync(string path, CancellationToken cancellationToken);
}
