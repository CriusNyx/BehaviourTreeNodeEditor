using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelSet<T>
    : IDictionary<(int x, int y, int z), T>,
    IDictionary<Vector3, T>
{
    #region Fields
    public readonly IVoxelOrientation orientation;

    private Dictionary<(int x, int y, int z), T> map
        = new Dictionary<(int x, int y, int z), T>();

    public IEnumerable<KeyValuePair<(int x, int y, int z), T>> Map => map;
    #endregion

    #region Properties
    public ICollection<(int x, int y, int z)> Keys
        => map.Keys;

    ICollection<Vector3> IDictionary<Vector3, T>.Keys
        => throw new System.InvalidOperationException();

    public ICollection<T> Values
        => map.Values;

    public int Count
        => map.Count;

    public bool IsReadOnly
        => ((IDictionary<(int x, int y, int z), T>)map).IsReadOnly;
    #endregion

    #region Indexers
    public T this[(int x, int y, int z) key] { get => map[key]; set => map[key] = value; }

    public T this[Vector3 key]
    {
        get
            => this[orientation.GetVoxelIndexOfPoint(key)];
        set
            => map[orientation.GetVoxelIndexOfPoint(key)] = value;
    }
    #endregion

    #region Constructors
    public VoxelSet(params ((int x, int y, int z) index, T value)[] entries)
        : this(1f, Vector3.zero, entries)
    { }

    public VoxelSet(IEnumerable<((int x, int y, int z) index, T value)> entries)
        : this(1f, Vector3.zero, entries)
    { }

    public VoxelSet(float voxelSize, Vector3 center, params ((int x, int y, int z) index, T value)[] entries)
        : this(new VoxelOrientation(voxelSize, center), entries)
    { }

    public VoxelSet(float voxelSize, Vector3 center, IEnumerable<((int x, int y, int z) index, T value)> entries)
        : this(new VoxelOrientation(voxelSize, center), entries)
    { }

    public VoxelSet(IVoxelOrientation orientation, params ((int x, int y, int z) index, T value)[] entries)
        : this(orientation, (IEnumerable<((int x, int y, int z) index, T value)>)entries)
    { }

    public VoxelSet(IVoxelOrientation orientation, IEnumerable<((int x, int y, int z) index, T value)> entries)
    {
        this.orientation = orientation;
        foreach (var entry in entries)
        {
            map[entry.index] = entry.value;
        }
    }
    #endregion

    #region Methods
    #region Adding Removing Getting
    public void Add((int x, int y, int z) key, T value)
        => map.Add(key, value);

    public void Add(KeyValuePair<(int x, int y, int z), T> item)
        => map.Add(item.Key, item.Value);

    public void Add(Vector3 key, T value)
    {
        Add(orientation.GetVoxelIndexOfPoint(key), value);
    }

    public void Add(KeyValuePair<Vector3, T> item)
    {
        Add(orientation.GetVoxelIndexOfPoint(item.Key), item.Value);
    }

    public bool Remove((int x, int y, int z) key)
        => map.Remove(key);

    public bool Remove(Vector3 key)
    {
        return Remove(orientation.GetVoxelIndexOfPoint(key));
    }

    public bool Remove(KeyValuePair<(int x, int y, int z), T> item)
    {
        return ((IDictionary<(int x, int y, int z), T>)map).Remove(item);
    }

    public bool Remove(KeyValuePair<Vector3, T> item)
    {
        throw new System.NotImplementedException();
    }

    public bool TryGetValue((int x, int y, int z) key, out T value)
        => map.TryGetValue(key, out value);

    public bool TryGetValue(Vector3 key, out T value)
    {
        return TryGetValue(orientation.GetVoxelIndexOfPoint(key), out value);
    }

    public void Clear()
        => map.Clear();
    #endregion

    #region Contains
    public bool ContainsKey((int x, int y, int z) key)
        => map.ContainsKey(key);

    public bool ContainsKey(Vector3 key)
    {
        return ContainsKey(orientation.GetVoxelIndexOfPoint(key));
    }

    public bool Contains(KeyValuePair<(int x, int y, int z), T> item)
    {
        return ((IDictionary<(int x, int y, int z), T>)map).Contains(item);
    }

    public bool Contains(KeyValuePair<Vector3, T> item)
    {
        return Contains(new KeyValuePair<(int x, int y, int z), T>(orientation.GetVoxelIndexOfPoint(item.Key), item.Value));
    }
    #endregion

    #region Enumerators
    public IEnumerator<KeyValuePair<(int x, int y, int z), T>> GetEnumerator()
    {
        return ((IDictionary<(int x, int y, int z), T>)map).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IDictionary<(int x, int y, int z), T>)map).GetEnumerator();
    }

    IEnumerator<KeyValuePair<Vector3, T>> IEnumerable<KeyValuePair<Vector3, T>>.GetEnumerator()
    {
        throw new System.NotImplementedException();
    }
    #endregion

    #region CopyTo
    public void CopyTo(KeyValuePair<Vector3, T>[] array, int arrayIndex)
    {
        throw new System.NotImplementedException();
    }

    public void CopyTo(KeyValuePair<(int x, int y, int z), T>[] array, int arrayIndex)
    {
        ((IDictionary<(int x, int y, int z), T>)map).CopyTo(array, arrayIndex);
    }

    public bool ContainsKeyOrAdjacent(int x, int y, int z) => ContainsKeyOrAdjacent((x, y, z));
    public bool ContainsKeyOrAdjacent(Vector3 v) => ContainsKeyOrAdjacent(orientation.GetVoxelIndexOfPoint(v));

    public bool ContainsKeyOrAdjacent((int x, int y, int z) key)
    {
        var (x, y, z) = key;
        int[] order = new int[] { 0, -1, 1 };

        foreach (var i in order)
            foreach (var j in order)
                foreach (var k in order)
                    if (ContainsKey((x + i, y + j, z + k)))
                        return true;

        return false;
    }

    public T GetValueOrAdjacent(int x, int y, int z) => GetValueOrAdjacent((x, y, z));
    public T GetValueOfAdjacent(Vector3 v) => GetValueOrAdjacent(orientation.GetVoxelIndexOfPoint(v));

    public T GetValueOrAdjacent((int x, int y, int z) key)
    {
        var (x, y, z) = key;
        int[] order = new int[] { 0, -1, 1 };

        foreach (var i in order)
            foreach (var j in order)
                foreach (var k in order)
                    if (ContainsKey((x + i, y + j, z + k)))
                        return this[(x + i, y + j, z + k)];

        throw new KeyNotFoundException();
    }

    public bool TryGetValueOrAdjacent(int x, int y, int z, out T value) => TryGetValueOrAdjacent((x, y, z), out value);
    public bool TryGetValueOrAdjacent(Vector3 v, out T value) 
        => TryGetValueOrAdjacent(orientation.GetVoxelIndexOfPoint(v), out value);

    public bool TryGetValueOrAdjacent((int x, int y, int z) key, out T value)
    {
        var (x, y, z) = key;
        int[] order = new int[] { 0, -1, 1 };

        foreach (var i in order)
            foreach (var j in order)
                foreach (var k in order)
                    if (ContainsKey((x + i, y + j, z + k)))
                    {
                        value = this[(x + i, y + j, z + k)];
                        return true;
                    }

        value = default;
        return false;
    }


    #endregion
    #endregion
}
