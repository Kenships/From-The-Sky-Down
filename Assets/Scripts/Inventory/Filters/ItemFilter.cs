using Inventory.Item;

namespace Inventory.Filters
{
    public abstract class ItemFilter
    {
        public abstract bool Validate(GameItemSO item);
    }
}
