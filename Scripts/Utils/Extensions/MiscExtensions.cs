using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utils.Extensions
{
    public static class MiscExtensions
    {
        public static T GetRandom<T>(this List<T> list, int startInd, int endInd, Func<T, bool> condition = null)
        {
            var checkList = list;
            if (condition != null)
                checkList = list.Where(condition).ToList();
            return checkList.Count == 0
                ? default(T)
                : checkList[Random.Range(Mathf.Clamp(startInd, 0, checkList.Count), Mathf.Clamp(endInd, 0, checkList.Count))];
        }

        public static T GetRandom<T>(this List<T> list, Func<T, bool> condition = null)
        {
            return list.GetRandom(0, list.Count, condition);
        }
        
        public static void ShuffleList<T>(this IList<T> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var randomIndex = Random.Range(i, list.Count);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }
        
        public static Color SetAlpha(this Color c, float a)
        {
            c.a = a;
            return c;
        }

        public static string SetColorTag(this string s, string color)
        {
            return string.IsNullOrEmpty(s) ? s : $"<color={color}>{s}</color>";
        }
        
        public static T2 GetValue<T1, T2>(this Dictionary<T1, T2> d, T1 key)
        {
            T2 v;
            d.TryGetValue(key, out v);
            return v;
        }

        public static int GetCurrentDaySeconds(this TimeSpan timespan)
        {
            return timespan.Hours * 3600 + timespan.Minutes * 60 + timespan.Seconds;
        }

        public static string Truncate(this string s, int maxLength)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Length <= maxLength ? s : s.Substring(0, maxLength);
        }

        public static void AddItem<TKey, TCollection, TItem>(this Dictionary<TKey, TCollection> d, TKey key, TItem item)
            where TCollection : ICollection<TItem>, new()
        {
            if (!d.ContainsKey(key))
                d.Add(key, new TCollection());
            d[key].Add(item);
        }

        public static void RemoveItem<TKey, TCollection, TItem>(this Dictionary<TKey, TCollection> d, TKey key, TItem item, bool removeKeyIfEmpty = true)
            where TCollection : ICollection<TItem>
        {
            if (!d.ContainsKey(key))
                return;
            d[key].Remove(item);
            if (removeKeyIfEmpty && d[key].Any())
                d.Remove(key);
        }

        public static void AddAndHandleKey<TKey, TItem>(this Dictionary<TKey, TItem> d, TKey key, Action<TKey, TItem> action) where TItem : new()
        {
            if (!d.ContainsKey(key))
                d.Add(key, new TItem());
            action?.Invoke(key, d[key]);
        }

        public static void HandleAndRemoveKey<TKey, TCollection, TItem>(this Dictionary<TKey, TCollection> d, TKey key, Action<TCollection> action)
            where TCollection : ICollection<TItem>, new()
        {
            if (d.ContainsKey(key))
            {
                action?.Invoke(d[key]);
                d.Remove(key);
            }
        }

        public static void AddIfUnique<T>(this IList<T> list, T item)
        {
            if (!list.Contains(item))
                list.Add(item);
        }
    }
}