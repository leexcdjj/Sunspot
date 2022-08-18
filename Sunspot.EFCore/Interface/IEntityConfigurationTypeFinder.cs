using Sunspot.Core.Reflection;

namespace Sunspot.EFCore.Interface;

public interface IEntityConfigurationTypeFinder : ITypeFinder, IFinder<Type>
{
}