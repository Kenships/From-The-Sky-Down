using Inventory.Item;

namespace Inventory.Filters
{
    public class TypeFilter<T> : ItemFilter
    {
        public override bool Validate(GameItemSO item)
        {
            return item is T;
        }
    }
}
