using System.Reflection;

namespace Sunspot.Core.Reflection.IFinder;

public interface IAllAssemblyFinder : IAssemblyFinder, IFinder<Assembly>
{
}