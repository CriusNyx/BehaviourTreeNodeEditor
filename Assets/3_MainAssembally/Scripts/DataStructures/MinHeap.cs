using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MinHeap<T, U> where T : IComparable<T>
{
    public int Length { get; private set; } = 0;
    private (T key, U value)[] data;

    public MinHeap()
    {

    }

    public MinHeap(int startSize)
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
        if(Length <= 0)
        {
            throw new InvalidOperationException("Cannot remove from an empty heap");
        }
        else
        {
            var output = Remove(0);
            return output;
        }
    }

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
        data[Length] = (key, value);
        Length++;
    }

    private (T key, U value) Remove(int index)
    {
        if(index < 0 || index >= Length)
        {
            throw new IndexOutOfRangeException();
        }

        var output = data[index];

        data[index] = (default, default);

        data[index] = Pop();

        FixDown(index);
        return output;
    }

    private (T key, U value) Pop()
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
        if (rightIndex >= Length)
        {
            if (LessThen(leftIndex, index))
            {
                Swap(leftIndex, index);
            }
        }
        else
        {
            // Determine which child is smaller
            if (LessThen(leftIndex, rightIndex))
            {
                if (LessThen(leftIndex, index))
                {
                    Swap(leftIndex, index);
                    FixDown(leftIndex);
                }
            }
            else
            {
                if (LessThen(rightIndex, index))
                {
                    Swap(rightIndex, index);
                    FixDown(rightIndex);
                }
            }

        }    }

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

    public bool LessThen(int index1, int index2)
    {
        return data[index1].key.CompareTo(data[index2].key) < 0;
    }

    private void Swap(int index1, int index2)
    {
        var temp = data[index1];
        data[index1] = data[index2];
        data[index2] = temp;
    }
}