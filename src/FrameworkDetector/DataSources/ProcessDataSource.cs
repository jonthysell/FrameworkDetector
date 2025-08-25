using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkDetector.DataSources;

public class ProcessDataSource : IDataSource
{
    public int ProcessId { get; }

    // TODO: Provide other helpers for finding processes, such as by executable name or window title.
    public ProcessDataSource(int processId)
    {
        ProcessId = processId;
    }

    public Task<bool> LoadAndCacheDataAsync()
    {
        throw new NotImplementedException();
    }
}
