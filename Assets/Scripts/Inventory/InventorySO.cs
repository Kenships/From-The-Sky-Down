using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Inventory.Filters;
using Inventory.Item;
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(fileName = "InventorySO", menuName = "Scriptable Object/Inventory/InventorySO")]
    public class InventorySO : ScriptableObject
    {
        [SerializeField]
        private SerializedDictionary<ItemStackType, int> stackMaximums;

        [SerializeField]
        private int inventorySize;

        private List<ItemStack> _inventoryStacks;
        private Dictionary<ItemId, int> _itemTotalsCache;
        private List<ItemFilter> _filters;

        public int Vacancy => inventorySize - _inventoryStacks.Count;
        public bool IsFull => inventorySize == _inventoryStacks.Count;

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

            _inventoryStacks ??= new List<ItemStack>();
            _itemTotalsCache ??= new Dictionary<ItemId, int>();
            _filters ??= new List<ItemFilter>();
        }

        public void AddFilter(ItemFilter itemFilter)
        {
            _filters.Add(itemFilter);
        }

        public void AddTypeFilter<T>() where T : GameItemSO
        {
            TypeFilter<T> filter = new TypeFilter<T>();
            AddFilter(filter);
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

        public void PresetInventory(List<ItemStack> inventoryStacks)
        {
            _inventoryStacks = inventoryStacks;
        }

        public bool TryAddItem(GameItemSO item, int count)
        {
            //TODO: better way to handle input validation

            #region Input Validation

            if (_filters.Any(filter => !filter.Validate(item)))
            {
                return false;
            }

            if (count < 0)
            {
                Debug.LogError($"InventorySO: TryAddItem: Tried to add {count} but count " +
                               "cannot be negative.");
            }

            int spaceRemaining;

            if (_itemTotalsCache.TryGetValue(item.ItemId, out int value))
            {
                spaceRemaining = value;
            }
            else
            {
                spaceRemaining = Vacancy * stackMaximums[item.ItemStackType] +
                                 _inventoryStacks.Where(itemStack => itemStack.ItemName == item.ItemName)
                                     .Sum(itemStack => itemStack.VacancyCount);
                _itemTotalsCache[item.ItemId] = spaceRemaining;
            }

            if (count > spaceRemaining)
            {
                return false;
            }

            #endregion

            int remainingCount = count;

            foreach (ItemStack itemStack in _inventoryStacks)
            {
                if (remainingCount == 0)
                {
                    break;
                }

                if (itemStack.TryFill(item, remainingCount, out int successfulCount))
                {
                    remainingCount -= successfulCount;

                    if (!_itemTotalsCache.TryAdd(item.ItemId, successfulCount))
                    {
                        _itemTotalsCache[item.ItemId] += successfulCount;
                    }
                }
            }

            while (remainingCount > 0 || !IsFull)
            {
                var itemStack = new ItemStack(item, stackMaximums[item.ItemStackType]);
                if (itemStack.TryFill(item, remainingCount, out int successfulCount))
                {
                    remainingCount -= successfulCount;

                    if (!_itemTotalsCache.TryAdd(item.ItemId, successfulCount))
                    {
                        _itemTotalsCache[item.ItemId] += successfulCount;
                    }
                }

                _inventoryStacks.Add(itemStack);
            }

            return true;
        }

        public bool TryRemoveItem(GameItemSO item, int count)
        {
            
            #region Input Validation

            if (count < 0)
            {
                Debug.LogError($"InventorySO: TryRemoveItem: Tried to remove {count} but count cannot be negative.");
            }

            int itemTotal;

            if (_itemTotalsCache.TryGetValue(item.ItemId, out int value))
            {
                itemTotal = value;
            }
            else
            {
                itemTotal = _inventoryStacks.Where(itemStack => itemStack.ItemId == item.ItemId).Sum(itemStack => itemStack.Count);
                _itemTotalsCache[item.ItemId] = itemTotal;
            }
            
            if (count > itemTotal)
            {
                return false;
            }

            #endregion
            List<ItemStack> itemsInInventory =
                _inventoryStacks.Where(itemStack => itemStack.ItemId == item.ItemId).ToList();

            
            int remainingCount = count;
            foreach (ItemStack itemStack in itemsInInventory)
            {
                if (remainingCount == 0)
                {
                    break;
                }

                if (itemStack.TryRemove(item, count, out int successfulCount))
                {
                    remainingCount -= successfulCount;

                    _itemTotalsCache[item.ItemId] -= successfulCount;
                }
            }

            if (_itemTotalsCache[item.ItemId] == 0)
            {
                _itemTotalsCache.Remove(item.ItemId);
            }

            return true;
        }
    }
}
