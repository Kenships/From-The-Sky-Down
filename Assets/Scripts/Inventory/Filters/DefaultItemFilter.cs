using Inventory.Item;

namespace Inventory.Filters
{
    public class DefaultItemFilter : ItemFilter
    {
        public override bool Validate(GameItemSO item)
        {
            return true;
        }
    }
}
