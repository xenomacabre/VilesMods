using System;
using System.Collections.Generic;

namespace DoorsExpanded
{
    public static class ListExtensions
    {
        public static List<T> AsList<T>(this IEnumerable<T> enumerable) =>
            enumerable is List<T> list ? list : new List<T>(enumerable);

        public static void ReplaceRange<T>(this List<T> list, int startIndex, int count, IEnumerable<T> newItems)
        {
            var enumerator = newItems.GetEnumerator();
            var index = startIndex;
            var endIndex = Math.Min(startIndex + count, list.Count);
            while (index < endIndex)
            {
                if (!enumerator.MoveNext())
                {
                    list.RemoveRange(index, endIndex - index);
                    return;
                }
                list[index] = enumerator.Current;
                index++;
            }
            if (enumerator.MoveNext())
            {
                var remainingItems = new List<T> { enumerator.Current };
                while (enumerator.MoveNext())
                {
                    remainingItems.Add(enumerator.Current);
                }
                list.InsertRange(index, remainingItems);
            }
        }

        public static List<T> PopAll<T>(this List<T> list)
        {
            var copiedList = new List<T>(list);
            list.Clear();
            return copiedList;
        }

        public static int FindSequenceIndex<T>(this List<T> list, params Predicate<T>[] sequenceMatches)
        {
            return list.FindSequenceIndex(0, list.Count, sequenceMatches);
        }

        public static int FindSequenceIndex<T>(this List<T> list, int startIndex, params Predicate<T>[] sequenceMatches)
        {
            return list.FindSequenceIndex(startIndex, list.Count - startIndex, sequenceMatches);
        }

        public static int FindSequenceIndex<T>(this List<T> list, int startIndex, int count, params Predicate<T>[] sequenceMatches)
        {
            if (sequenceMatches is null)
                throw new ArgumentNullException(nameof(sequenceMatches));
            if (sequenceMatches.Length == 0)
                throw new ArgumentException($"sequenceMatches must not be empty");
            if (count - sequenceMatches.Length < 0)
                return -1;
            count -= sequenceMatches.Length - 1;
            var index = list.FindIndex(startIndex, count, sequenceMatches[0]);
            while (index != -1)
            {
                var allMatched = true;
                for (var matchIndex = 1; matchIndex < sequenceMatches.Length; matchIndex++)
                {
                    if (!sequenceMatches[matchIndex](list[index + matchIndex]))
                    {
                        allMatched = false;
                        break;
                    }
                }
                if (allMatched)
                    break;
                startIndex++;
                count--;
                index = list.FindIndex(startIndex, count, sequenceMatches[0]);
            }
            return index;
        }
    }
}
