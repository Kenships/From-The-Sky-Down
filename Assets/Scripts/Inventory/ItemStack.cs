using System;
using Inventory.Item;
using UnityEngine;

namespace Inventory
{
    [Serializable]
    public class ItemStack
    {
        public ItemStack(GameItemSO item, int maxCount = 1)
        {
            Item = item;
            MaxCount = maxCount;
        }

        public bool TryFill(GameItemSO item, int count, out int successfulCount)
        {
            successfulCount = 0;
            
            
            if (item.ItemName == ItemName)
            {
                successfulCount = Mathf.Min(VacancyCount, count);
                Count += successfulCount;

                return successfulCount > 0;
            }

            return false;
        }

        public bool TryRemove(GameItemSO item, int count, out int successfulCount)
        {
            successfulCount = 0;

            if (item.ItemId == ItemId)
            {
                successfulCount = Mathf.Min(Count, count);
                Count -= successfulCount;
                
                return successfulCount > 0;
            }
            
            return false;
        }
        
        public GameItemSO Item {get; private set; }
        public int Count {get; private set; }
        public int MaxCount { get; private set; }
        public bool IsEmpty => Count == 0;
        public bool IsStackable => MaxCount > 1;
        public bool IsFull => Count >= MaxCount;
        public int VacancyCount => MaxCount - Count;
        public ItemId ItemId => Item.ItemId;
        public string ItemName => Item.ItemName;
        public string ItemDescription => Item.ItemDescription;
        public ItemStackType ItemStackType => Item.ItemStackType;
        public Sprite ItemSprite => Item.ItemSprite;
        public GameObject ItemPrefab => Item.ItemPrefab;
       
    }
}
