using Inventory.Item;

namespace Inventory.Filters
{
    
    //NOT a "whitelist" filter, only allows one type through 
    public abstract class ItemFilter
    {
        public abstract bool Validate(GameItemSO item);
    }
}
