using Pretzel.Logic.Templating.Context;

namespace Pretzel.Categories
{
    public static class PageExtensions
    {
        internal const string CategoryPageKey = "category";

        internal const string SubCategoryPageKey = "subcategory";

        public static string TryGetCategory( this Page page )
        {
            if( page.Bag.ContainsKey( CategoryPageKey ) == false )
            {
                return string.Empty;
            }

            return page.Bag[CategoryPageKey]?.ToString();
        }

        public static string TryGetSubCategory( this Page page )
        {
            if( page.Bag.ContainsKey( SubCategoryPageKey ) == false )
            {
                return string.Empty;
            }

            return page.Bag[SubCategoryPageKey]?.ToString();
        }
    }
}
