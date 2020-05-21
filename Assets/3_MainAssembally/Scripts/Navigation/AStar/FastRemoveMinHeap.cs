using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FastRemoveMinHeap<T, U> where T : IComparable<T>
{
    public int Length { get; private set; } = 0;
    private (T key, U value)[] data;
    private Dictionary<U, int> indexMap = new Dictionary<U, int>();

    public FastRemoveMinHeap()
    {

    }

    public FastRemoveMinHeap(int startSize)
    {
        data = new (T, U)[startSize];
    }

    public void Add(T key, U value)
    {
        Push(key, value);
        FixUp(Length - 1);
    }

    public (T key, U value) Remove()
    {
        if (Length <= 0)
        {
            throw new InvalidOperationException("Cannot remove from an empty heap");
        }
        else
        {
            return Remove(0);
        }
    }

    public (T key, U value) Remove(U value)
    {
        int index = indexMap[value];
        return Remove(index);
    }

    public bool Contains(U value)
        => indexMap.ContainsKey(value);

    private void EnsurePush()
    {
        int length = data?.Length ?? 0;
        if (length < this.Length + 1)
        {
            (T, U)[] newData = new (T, U)[length * 2 + 1];

            if (data != null)
                Array.Copy(data, 0, newData, 0, length);

            data = newData;
        }
    }

    private void Push(T key, U value)
    {
        EnsurePush();

        indexMap[value] = Length;
        data[Length] = (key, value);


        Length++;
    }

    private (T key, U value) Remove(int index)
    {
        if (index < 0 || index >= Length)
        {
            throw new IndexOutOfRangeException();
        }

        var output = data[index];

        indexMap.Remove(data[index].value);

        if (index != Length - 1)
        {
            data[index] = Shrink();
            if (Length != 0)
                indexMap[data[index].value] = index;

            FixDown(index);
        }
        else
        {
            Shrink();
        }

        return output;
    }

    private (T key, U value) Shrink()
    {
        var output = data[Length - 1];

        // ensures garbage collection
        data[Length - 1] = (default, default);

        Length--;

        return output;
    }

    public void FixUp(int index)
    {
        if (index > 0)
        {
            int parentIndex = GetParentIndex(index);
            if (LessThen(index, parentIndex))
            {
                Swap(index, parentIndex);
                FixUp(parentIndex);
            }
        }
    }

    public void FixDown(int index)
    {
        // Check if we are still in bounds
        if (index >= Length)
        {
            return;
        }

        // Get child indexes
        int leftIndex = GetLeftIndex(index);
        int rightIndex = GetRightIndex(index);

        // Check if both children are out of bounds
        if (leftIndex >= Length && rightIndex >= Length)
        {
            return;
        }

        // Check if one child is out of bounds
        else if (rightIndex >= Length)
        {
            if (LessThen(leftIndex, index))
            {
                Swap(leftIndex, index);
            }
            return;
        }

        // Determine which child is smaller
        else if (LessThen(leftIndex, rightIndex))
        {
            if (LessThen(leftIndex, index))
            {
                Swap(leftIndex, index);
                FixDown(leftIndex);
            }
            return;
        }
        else
        {
            if (LessThen(rightIndex, index))
            {
                Swap(rightIndex, index);
                FixDown(rightIndex);
            }
        }
    }

    private int GetLeftIndex(int index)
    {
        return index * 2 + 1;
    }

    private int GetRightIndex(int index)
    {
        return index * 2 + 2;
    }

    private int GetParentIndex(int index)
    {
        if (index == 0)
            return -1;
        else
            return (index - 1) / 2;
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("[");
        for (int i = 0; i < Length; i++)
        {
            builder.Append($"{data[i]}, ");
        }
        return $"{builder.ToString().Trim(',', ' ')}]";
    }

    private bool LessThen(int index1, int index2)
    {
        return data[index1].key.CompareTo(data[index2].key) < 0;
    }

    private bool Equal(int index1, int index2)
    {
        return data[index1].key.CompareTo(data[index2].key) == 0;
    }

    private void Swap(int index1, int index2)
    {
        var temp = data[index1];
        data[index1] = data[index2];
        data[index2] = temp;

        indexMap[data[index1].value] = index1;
        indexMap[data[index2].value] = index2;
    }

    public void CheckInvariant()
    {
        if (indexMap.Count != Length)
        {
            throw new Exception("The index map length does not equal the length of the heap");
        }
        for (int i = 0; i < Length; i++)
        {
            var value = data[i].value;
            if (indexMap[value] != i)
            {
                throw new Exception("Index is not correct for element");
            }

            int left = GetLeftIndex(i);
            int right = GetRightIndex(i);
            if (left < Length)
            {
                if (!(LessThen(i, left) || Equal(i, left)))
                {
                    throw new Exception("Heap Invariant is not correct");
                }
            }
            if (right < Length)
            {
                if (!(LessThen(i, right) || Equal(i, right)))
                {
                    throw new Exception("Heap Invariant is not correct");
                }
            }
        }
        for (int i = Length; i < data.Length; i++)
        {
            if (!Equals(data[i].key, default(T)) || !Equals(data[i].value, default(U)))
            {
                throw new Exception("Garbage data is not correct");
            }
        }
    }

    public void Clear()
    {
        indexMap = new Dictionary<U, int>(data?.Length ?? 0);
        for(int i = 0; i < Length; i++)
        {
            data[i] = (default, default);
        }
        Length = 0;
    }
}