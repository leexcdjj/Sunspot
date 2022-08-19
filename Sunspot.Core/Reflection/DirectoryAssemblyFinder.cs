using System.Collections.Concurrent;
using System.Reflection;
using Sunspot.Core.Reflection.IFinder;

namespace Sunspot.Core.Reflection;

public class DirectoryAssemblyFinder : IAssemblyFinder, IFinder<Assembly>
{
    private static readonly ConcurrentDictionary<string, Assembly[]> CacheDict =
        new ConcurrentDictionary<string, Assembly[]>();

    private readonly string _path;

    public DirectoryAssemblyFinder(string path)
    {
        _path = path;
    }

    public Assembly[] Find(Func<Assembly, bool> predicate, bool fromCache = false)
    {
        return FindAll(fromCache).Where(predicate).ToArray();
    }

    public Assembly[] FindAll(bool fromCache = false)
    {
        if (fromCache && CacheDict.ContainsKey(_path))
        {
            return CacheDict[_path];
        }

        Assembly[] array = Directory.GetFiles(_path, "*.dll", SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(_path, "*.exe", SearchOption.TopDirectoryOnly)).ToArray()
            .Select(Assembly.LoadFrom).Distinct().ToArray();
        
        CacheDict[_path] = array;
        return array;
    }
}