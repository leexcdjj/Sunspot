namespace Sunspot.Core.Reflection;

/// <summary>
/// 查找器基类
/// </summary>
/// <typeparam name="TItem"></typeparam>
public abstract class FinderBase<TItem> : IFinder<TItem>
{
    private readonly object _lockObj = new object();
    
    protected readonly List<TItem> ItemsCache = new List<TItem>();
    
    protected bool Found;

    public virtual TItem[] Find(Func<TItem, bool> predicate, bool fromCache = false)
    {
        return FindAll(fromCache).Where(predicate).ToArray();
    }

    public virtual TItem[] FindAll(bool fromCache = false)
    {
        lock (_lockObj)
        {
            if (fromCache && Found)
            {
                return ItemsCache.ToArray();
            }
            
            TItem[] allItems = FindAllItems();
            Found = true;
            ItemsCache.Clear();
            ItemsCache.AddRange(allItems);
            return allItems;
        }
    }

    protected abstract TItem[] FindAllItems();
}