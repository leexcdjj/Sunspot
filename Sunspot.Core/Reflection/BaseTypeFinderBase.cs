using System.Reflection;
using Sunspot.Core.Reflection.IFinder;

namespace Sunspot.Core.Reflection;

public abstract class BaseTypeFinderBase<TBaseType>: FinderBase<Type>, ITypeFinder, IFinder<Type>
{
    private readonly IAllAssemblyFinder _allAssemblyFinder;

    protected BaseTypeFinderBase(IAllAssemblyFinder allAssemblyFinder)
    {
        _allAssemblyFinder = allAssemblyFinder;
    }

    protected override Type[] FindAllItems()
    {
        return ((IEnumerable<Assembly>) _allAssemblyFinder.FindAll(true))
            .SelectMany<Assembly,
                Type>((Func<Assembly, IEnumerable<Type>>) (assembly => (IEnumerable<Type>) assembly.GetTypes()))
            .Where<Type>((Func<Type, bool>) (type => type.IsDeriveClassFrom<TBaseType>())).Distinct<Type>()
            .ToArray<Type>();
    }
}