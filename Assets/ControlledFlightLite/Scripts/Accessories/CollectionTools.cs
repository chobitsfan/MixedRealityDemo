using System.Collections.Generic;
using UnityEngine;

namespace SparseDesign
{
    /// <summary>
    /// Tools for analysing and/or manipulating collection objects (List, HashSet, Stack,...)
    /// </summary>
    public static class CollectionTools
    {
        public static T GetRandomFromList<T>(List<T> l)
        {
            if (l.Count > 0) return l[Random.Range(0, l.Count)];
            else return default;
        }

        public static void DestroyCollectionOfObjects(List<GameObject> objs)
        {
            foreach (var obj in objs)
            {
                MonoBehaviour.Destroy(obj);
            }
            objs.Clear();
        }

        public static void DestroyCollectionOfObjects(HashSet<GameObject> objs)
        {
            foreach (var obj in objs)
            {
                MonoBehaviour.Destroy(obj);
            }
            objs.Clear();
        }

        public static void DestroyCollectionOfObjects(Stack<GameObject> objs)
        {
            foreach (var obj in objs)
            {
                MonoBehaviour.Destroy(obj);
            }
            objs.Clear();
        }

        public static K GetLowest<K, V>(SortedDictionary<K, V> dict)
        {
            K res = default;
            foreach (var p in dict.Keys)
            {
                res = p;
                break;
            }
            return res;
        }

        public static T GetLowest<T>(Dictionary<T, int> dict)
        {
            int lowestValue = int.MaxValue;
            T best = default;

            foreach (var d in dict)
            {
                if (d.Value < lowestValue)
                {
                    lowestValue = d.Value;
                    best = d.Key;
                }
            }

            return best;
        }

        public static T GetLowest<T>(Dictionary<T, float> dict)
        {
            float lowestValue = float.MaxValue;
            T best = default;

            foreach (var d in dict)
            {
                if (d.Value < lowestValue)
                {
                    lowestValue = d.Value;
                    best = d.Key;
                }
            }
            return best;
        }

        public static T GetLowest<T>(Dictionary<T, double> dict)
        {
            double lowestValue = double.MaxValue;
            T best = default;

            foreach (var d in dict)
            {
                if (d.Value < lowestValue)
                {
                    lowestValue = d.Value;
                    best = d.Key;
                }
            }
            return best;
        }

        public static List<T> Array2DToList<T>(T[,] arr)
        {
            var l = new List<T>(arr.Length);

            for (int x = 0; x <= arr.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= arr.GetUpperBound(1); y++)
                {
                    l.Add(arr[x, y]);
                }
            }
            return l;
        }

        /// <summary>
        /// Takes a List and converts it to a 2D array with diemnsions according to input
        /// </summary>
        /// <typeparam name="T">Array type</typeparam>
        /// <param name="list">The List to convert to array</param>
        /// <param name="lengthDim0">The length of first dimension of the 2D array</param>
        /// <param name="lengthDim1">The length of second dimension of the 2D array</param>
        /// <returns>A 2D array instanced as T[lengthDim0, lengthDim1]</returns>
        ///
        public static T[,] ListTo2dArray<T>(List<T> list, int lengthDim0, int lengthDim1)
        {
            var arr = new T[lengthDim0, lengthDim1];
            int idx = 0;
            for (int x = 0; x < lengthDim0; x++)
            {
                for (int y = 0; y < lengthDim1 && idx < list.Count; y++, idx++)
                {
                    arr[x, y] = list[idx];
                }
            }

            return arr;
        }

        /// <summary>
        /// Subtracts the items in b from a
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>A collection of items that exist in a but not both in a and b (order is preserved)</returns>
        public static List<T> Minus<T>(List<T> a, List<T> b)
        {
            var r = new List<T>();
            foreach (var item in a)
            {
                if (!b.Contains(item)) r.Add(item);
            }
            return r;
        }

        /// <summary>
        /// Subtracts the items in b from a
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>A collection of items that exist in a but not in a and b</returns>
        public static HashSet<T> Minus<T>(HashSet<T> a, HashSet<T> b)
        {
            var r = new HashSet<T>();
            foreach (var item in a)
            {
                if (!b.Contains(item)) r.Add(item);
            }
            return r;
        }

        /// <summary>
        /// Calculates the median of the values in a collection
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Median(List<float> b)
        {
            float res = default;
            if (b == null || b.Count < 1) return res;

            var a = new List<float>(b);

            a.Sort();

            if (a.Count % 2 == 0)
            {
                var idx2 = a.Count / 2;
                var idx1 = idx2 - 1;
                res = (a[idx1] + a[idx2]) / 2f;
            }
            else
            {
                var idx = (a.Count - 1) / 2;
                res = a[idx];
            }

            return res;
        }

        /// <summary>
        /// Calculates the median of the values in a collection
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Median(HashSet<float> b)
        {
            var a = new List<float>(b);
            return CollectionTools.Median(a);
        }

        public static void RemoveNulls<T>(List<T> objs)
        {
            objs.RemoveAll(item => item == null);
        }

        public static void RemoveNulls(List<GameObject> objs)
        {
            objs.RemoveAll(item => item == null);
        }

        public static void RemoveNulls<T>(HashSet<T> objs)
        {
            objs.RemoveWhere(item => item == null);
        }
        public static void RemoveNulls(HashSet<GameObject> objs)
        {
            objs.RemoveWhere(item => item == null);
        }
    }
}