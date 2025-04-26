using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

namespace DialogueSystem.Utilities
{
    public static class CollectionUtility
    {
        public static void AddItem<K, V>(this SerializedDictionary<K, List<V>> serializedDictionary, K key, V value)
        {
            if (serializedDictionary.ContainsKey(key))
            {
                serializedDictionary[key].Add(value);
                return;
            }
            serializedDictionary.Add(key, new List<V>{ value });
        }
    }
}