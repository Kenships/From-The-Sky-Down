using System.Collections.Generic;
using Inventory.Item;

namespace Inventory.Filters
{
    public class BlacklistFilter<T> : ItemFilter
    {
        public override bool Validate(GameItemSO item)
        {
            return item is not T;
        }
    }
}
