using System.Threading.Tasks;

namespace FrameworkDetector.DataSources;

public interface IDataSource
{
    Task<bool> LoadAndCacheDataAsync();
}