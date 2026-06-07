using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Serializable2DArray<T>
{
    public List<Row<T>> values = new List<Row<T>>();

    [Serializable]
    public class Row<U>
    {
        public List<U> values = new List<U>();
    }

    public Serializable2DArray() { }

    public Serializable2DArray(int width, int height)
    {
        EnsureSize(width, height);
    }

    public void EnsureSize(int width, int height)
    {
        while (values.Count < height)
            values.Add(new Row<T>());
        while (values.Count > height)
            values.RemoveAt(values.Count - 1);

        for (int y = 0; y < height; y++)
        {
            var row = values[y];
            while (row.values.Count < width)
                row.values.Add(default);
            while (row.values.Count > width)
                row.values.RemoveAt(row.values.Count - 1);
        }
    }

    public void SetFromRealArray(T[,] source)
    {
        int height = source.GetLength(0);
        int width = source.GetLength(1);
        EnsureSize(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                values[y].values[x] = source[y, x];
            }
        }
    }

    public T[,] ToRealArray()
    {
        if (values.Count == 0) return new T[0, 0];

        int height = values.Count;
        int width = values[0].values.Count;
        T[,] result = new T[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                result[y, x] = values[y].values[x];
            }
        }

        return result;
    }
}
