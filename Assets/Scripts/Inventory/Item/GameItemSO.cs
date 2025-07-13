using UnityEngine;

namespace Inventory.Item
{
    [CreateAssetMenu(fileName = "GameItemSO", menuName = "Scriptable Object/Inventory/GameItemSO")]
    public class GameItemSO : ScriptableObject
    {
        public string ItemName;
        public Sprite ItemSprite;
        public GameObject ItemPrefab;
        public ItemStackType ItemStackType;
    }
}
