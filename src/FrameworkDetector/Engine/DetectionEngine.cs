using FrameworkDetector.Checks;
using FrameworkDetector.DataSources;
using FrameworkDetector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkDetector.Engine;
public class DetectionEngine
{
    private List<DetectorDefinition> _detectors { get; init; } = new();

    public DetectionEngine(IEnumerable<IDetector> detectors)
    {
        foreach (IDetector detector in detectors)
        {
            _detectors.Add(detector.CreateDefinition());
        }
    }

    // TODO: Have a progress type with more detailed information.
    public async Task<ToolRunResult> DetectAgainstSourcesWithProgressAsync(DataSourceCollection sources, IProgress<int> progress, CancellationToken cancellationToken)
    {
        var result = new ToolRunResult(AssemblyInfo.ToolName, AssemblyInfo.ToolVersion, sources);

        ParallelOptions options = new()
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Environment.ProcessorCount,
        };

        // Step 1. Initialize all the data sources.
        var loopResult = Parallel.ForEach(sources.Values.SelectMany(inner => inner), options, async source =>
        {
            await source.LoadAndCacheDataAsync();
        });

        if (!loopResult.IsCompleted)
        {
            // TODO: We should define how we want to handle, log, and report exceptions at various points.
            throw new InvalidOperationException("Data Sources Couldn't be initialized.");
        }

        // Step 2. Run all the detectors against the data sources.
        Parallel.ForEach(_detectors, options, async detector =>
        {
            // TODO: Probably parallelizing on the detectors is enough vs. each check
            // Required checks need to all pass for the framework to be detected and reported as such.
            foreach (var requiredCheck in detector.RequiredChecks)
            {
                await Task.Yield();

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var innerResult = await requiredCheck.PerformCheckAsync(sources, cancellationToken);
            }

            // Optional checks won't fail the detection of the framework and are used to provide stronger confidence or additional metadata about the framework.
            foreach (var optionalCheck in detector.OptionalChecks)
            {
                await Task.Yield();

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var innerResult = await optionalCheck.PerformCheckAsync(sources, cancellationToken);
            }
        });

        // TODO: Step 3. Aggregate/Finalize all the results?

        return result;
    }
}
