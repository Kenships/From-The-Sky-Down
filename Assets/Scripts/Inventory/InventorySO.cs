using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Inventory.Filters;
using Inventory.Item;
using UnityEngine;

namespace Inventory
{
    public enum ItemStackType
    {
        Unstackable,
        SmallStack,
        LargeStack,
        Unlimited
    }
    
    [CreateAssetMenu(fileName = "InventorySO", menuName = "Scriptable Object/Inventory/InventorySO")]
    public class InventorySO : ScriptableObject
    {
        [SerializeField] private SerializedDictionary<ItemStackType, int> stackMaximums;
        [SerializeField] private List<ItemStack> inventory = new();
        [SerializeField] private int inventorySize;
        private List<ItemFilter> _filters;
        
        public int Vacancy => inventorySize - inventory.Count;
        public bool IsFull => inventorySize == inventory.Count;

        private void OnEnable()
        {
            Initialize();
        }

        private void Initialize()
        {
            foreach (ItemStackType currencyType in Enum.GetValues(typeof(ItemStackType)))
            {
                stackMaximums.TryAdd(currencyType, 1);
            }
            
            _filters ??= new List<ItemFilter>();
        }

        public void AddFilter(ItemFilter itemFilter)
        {
            _filters.Add(itemFilter);
        }

        public void AddAllFilters(List<ItemFilter> filters)
        {
            _filters.AddRange(filters);
        }

        public void SetFilters(List<ItemFilter> filters)
        {
            _filters = filters;
        }

        public void RemoveFilter(ItemFilter itemFilter)
        {
            _filters.Remove(itemFilter);
        }

        public void PresetInventory(List<ItemStack> inventory)
        {
            this.inventory = inventory;
        }

        public bool TryAddItem(GameItemSO item, int count)
        {
            //TODO: better way to handle input validation
            
            #region Input Validation
            
            if (_filters.Any(filter => !filter.Validate(item)))
            {
                return false;
            }
            
            if (count < 0) Debug.LogError($"InventorySO: TryAddItem: Tried to add {count} but count " +
                                          $"cannot be negative.");
            
            int spaceRemaining = Vacancy * stackMaximums[item.ItemStackType] + 
                                 inventory.Where(itemStack => itemStack.ItemName == item.ItemName)
                                     .Sum(itemStack => itemStack.VacancyCount);
            
            if (count > spaceRemaining) return false;
            #endregion
            
            int remainingCount = count;
            
            foreach (ItemStack itemStack in inventory)
            {
                if (remainingCount == 0) break;
                
                if (itemStack.TryAdd(item, remainingCount, out int stackSuccessfulCount))
                {
                    remainingCount -= stackSuccessfulCount;
                }
            }
            
            while (remainingCount > 0 || !IsFull)
            {
                ItemStack itemStack = new ItemStack(item, stackMaximums[item.ItemStackType]);
                if (itemStack.TryAdd(item, remainingCount, out int stackSuccessfulCount))
                {
                    remainingCount -= stackSuccessfulCount;
                }
                inventory.Add(itemStack);
            }

            return true;
        }

        public bool TryRemoveItem(GameItemSO item, int count)
        {
            #region Input Validation
            if (count < 0) Debug.LogError($"InventorySO: TryRemoveItem: Tried to remove {count} but count cannot be negative.");

            int countInStorage = 0;

            foreach (ItemStack itemStack in inventory)
            {
                if (itemStack.ItemName == item.ItemName)
                {
                    countInStorage += itemStack.Count;
                }
            }
            
            if (count > countInStorage) return false;
            #endregion
            
            int remainingCount = count;
            
            for (int i = count - 1; i >= 0; i--)
            {
                if (remainingCount == 0) break;
                
                ItemStack itemStack = inventory[i];
                if (itemStack.TryRemove(item, count, out int stackSuccessfulCount))
                {
                    remainingCount -= stackSuccessfulCount;
                }
            }
            
            return true;
        }
    }

    [Serializable]
    public class ItemStack
    {
        public ItemStack(GameItemSO item, int maxCount = 1)
        {
            Item = item;
            MaxCount = maxCount;
        }

        public bool TryAdd(GameItemSO item, int count, out int successfulCount)
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

            if (item.ItemName == ItemName)
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
        public string ItemName => Item.ItemName;
        public Sprite ItemSprite => Item.ItemSprite;
        public GameObject ItemPrefab => Item.ItemPrefab;
        public ItemStackType ItemStackType => Item.ItemStackType;
    }
}
