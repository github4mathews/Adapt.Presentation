using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Adapt.Extensions
{
    /// <summary>
    /// Some extension methods for IEnumerable lists
    /// </summary>
    public static class IEnumerableExtension
    {
        /// <summary>
        /// Takes a given collection and attempts to convert it to the specified type
        /// </summary>
        public static List<T> ToType<T>(this IEnumerable list)
        {
#if (!NETFX_CORE)
            var retVal = new List<T>();

            foreach (var item in list)
            {
                if (item == null)
                {
                    continue;
                }

                if (!item.GetType().GetInterfaces().Contains(typeof(IConvertible)))
                {
                    throw new Exception($"The type {item.GetType().Name} is not a convertible type");
                }

                try
                {
                    retVal.Add((T)Convert.ChangeType(item, typeof(T), null));
                }
                catch
                {
                    throw new Exception($"The value of {item} can not be converted to type {typeof(T).Name}");
                }
            }

            return retVal;
#else
            throw new NotImplementedException();
#endif
        }

        public static string ToString<TSource>(this IEnumerable<TSource> list, Func<TSource, string> predicate)
        {
            var retVal = string.Empty;

            foreach (var item in list)
            {
                retVal += predicate(item);
            }

            return retVal;
        }

        public static Collection<TSource> ToCollection<TSource>(this IEnumerable<TSource> source)
        {
            return new Collection<TSource>(source.ToList());
        }

        public static void AddRange<TSource>(this ICollection<TSource> collection, IEnumerable<TSource> range)
        {
            foreach (var item in range)
            {
                collection.Add(item);
            }
        }


        public static void RemoveRange<TSource>(this ICollection<TSource> collection, IEnumerable<TSource> range)
        {
            foreach (var item in range)
            {
                collection.Remove(item);
            }
        }

        public static bool ContainsAny(this IEnumerable collection, IEnumerable range)
        {
            if (range == null)
            {
                return false;
            }

            foreach (var item in range)
            {
                foreach (var innerItem in collection)
                {
                    if (item.Equals(innerItem))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}
