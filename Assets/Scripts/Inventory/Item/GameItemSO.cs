using UnityEngine;

namespace Inventory.Item
{
    public abstract class GameItemSO : ScriptableObject
    {
        public ItemId ItemId;
        [Space(10)]
        public string ItemName;
        public string ItemDescription;
        [Space(10)]
        public ItemStackType ItemStackType;
        [Space(10)]
        public Sprite ItemSprite;
        public GameObject ItemPrefab;
        
    }
}
