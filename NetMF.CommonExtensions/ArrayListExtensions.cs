using System;
using Microsoft.SPOT;
using System.Collections;

namespace NetMf.CommonExtensions
{
    /// <summary>
    /// Extension methods for ArrayList
    /// </summary>
    public static class ArrayListExtensions
    {
        /// <summary>
        /// Add Range to ArrayList
        /// </summary>
        /// <param name="list">Aray List to add to</param>
        /// <param name="arr">Items to be added to the collection</param>
        public static void AddRange(this ArrayList list, Array arr)
        {
            foreach (object b in arr)
            {
                list.Add(b);
            }
        }


        /// <summary>
        /// Removes a range of elements from the ArrayList
        /// </summary>
        /// <param name="list">List to operate on</param>
        /// <param name="index">starting index</param>
        /// <param name="count"></param>
        public static void RemoveRange(this ArrayList list, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                list.RemoveAt(index);
            }
        }

        /// <summary>
        /// Bubble-sort the specified array using the specified comparer.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="comparer"></param>
        public static void Sort(this ArrayList array, IComparer comparer)
        {
            object temp;
            for (int pos = array.Count - 1; pos >= 0; pos--)
            {
                for (int scan = 0; scan < pos; scan++)
                {
                    if (comparer.Compare(array[scan], array[scan + 1]) > 0)
                    {
                        temp = array[scan];
                        array[scan] = array[scan + 1];
                        array[scan + 1] = temp;
                    }
                }
            }
        }

        /// <summary>
        /// The method inserts the passed object to the array, which is presumed to be sorted.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <param name="comparer"></param>
        public static void AddToSorted(this ArrayList array, object value, IComparer comparer)
        {
            int count = array.Count;
            for (int i = 0; i < count; i++)
            {
                if (comparer.Compare(array[i], value) > 0)
                {
                    array.Insert(i, value);
                    return;
                }
            }

            //So the new value is the biggest one
            array.Add(value);
        }
    }

    /// <summary>
    /// Extension methods for Array
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Bubble-sort the specified array using the specified comparer.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="comparer"></param>
        public static void Sort(this object[] array, IComparer comparer)
        {
            object temp;
            for (int pos = array.Length - 1; pos >= 0; pos--)
            {
                for (int scan = 0; scan < pos; scan++)
                {
                    if (comparer.Compare(array[scan], array[scan + 1]) > 0)
                    {
                        temp = array[scan];
                        array[scan] = array[scan + 1];
                        array[scan + 1] = temp;
                    }
                }
            }
        }

        /// <summary>
        /// Bubble-sort the specified array using the specified comparer.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="comparer"></param>
        public static void Sort(this int[] array)
        {
            int temp;
            for (int pos = array.Length - 1; pos >= 0; pos--)
            {
                for (int scan = 0; scan < pos; scan++)
                {
                    if (array[scan] > array[scan + 1])
                    {
                        temp = array[scan];
                        array[scan] = array[scan + 1];
                        array[scan + 1] = temp;
                    }
                }
            }
        }
    }

    /// <summary>
    /// The class implements an <see cref="IComparer"/> for Int32 objects.
    /// </summary>
    public class Int32Comparer:IComparer{
        public int Compare(object x, object y)
        {
            return (Int32)x - (Int32)y;
        }
    }
}
