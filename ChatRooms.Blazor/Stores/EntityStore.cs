namespace ChatRooms.Blazor.Stores;

public abstract class EntityStore<TItem>
{
    private List<TItem> _items = [];
    private readonly Lock _lock = new();
    private bool _loaded;

    public IReadOnlyList<TItem> Items
    {
        get { lock (_lock) return _items.ToList().AsReadOnly(); }
    }

    public event Action? StateChanged;

    public bool IsLoaded
    {
        get { lock (_lock) return _loaded; }
    }

    protected void ReplaceAll(IEnumerable<TItem> items)
    {
        lock (_lock)
        {
            _items = [.. items];
            _loaded = true;
        }
        NotifyStateChanged();
    }

    protected void InsertAt(int index, TItem item)
    {
        lock (_lock)
        {
            _items.Insert(index, item);
        }
        NotifyStateChanged();
    }

    protected void RemoveWhere(Func<TItem, bool> predicate)
    {
        lock (_lock)
        {
            _items.RemoveAll(p => predicate(p));
        }
        NotifyStateChanged();
    }

    protected void UpdateWhere(Func<TItem, bool> predicate, Func<TItem, TItem> update)
    {
        lock (_lock)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (predicate(_items[i]))
                {
                    _items[i] = update(_items[i]);
                }
            }
        }
        NotifyStateChanged();
    }

    public void Invalidate()
    {
        lock (_lock)
        {
            _items.Clear();
            _loaded = false;
        }
        NotifyStateChanged();
    }

    protected void NotifyStateChanged() => StateChanged?.Invoke();
}
