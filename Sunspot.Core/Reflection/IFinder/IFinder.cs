namespace Sunspot.Core.Reflection.IFinder;

/// <summary>
/// 查找器接口
/// </summary>
/// <typeparam name="TItem"></typeparam>
public interface IFinder<out TItem>
{
    TItem[] Find(Func<TItem, bool> predicate, bool fromCache = false);

    TItem[] FindAll(bool fromCache = false);    
}