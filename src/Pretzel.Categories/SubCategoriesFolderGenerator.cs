// Pretzel.Categories plugin
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Categories
{
    [Export( typeof( IBeforeProcessingTransform ) )]
    public class SubCategoriesFolderGenerator : BaseFolderGenerator
    {
        private const string pageKey = "subcategory";

        public SubCategoriesFolderGenerator() :
            // For ease-of-use, put subcategories in the category folder,
            // (a sub-category is a category)
            // but make the layout "subcategory" so we can tell the difference
            // when generating a site.
            base( "category", "subcategory" )
        {
        }

        protected override IEnumerable<string> GetNames( SiteContext siteContext )
        {
            var list = new List<string>();
            if( siteContext.SubcategoriesEnabled() == false )
            {
                return list;
            }

            Console.WriteLine( "Sub-categories enabled" );

            foreach( Page page in siteContext.Posts )
            {
                if( page.Bag.ContainsKey( pageKey ) == false )
                {
                    continue;
                }
                if( string.IsNullOrWhiteSpace( page.Bag[pageKey].ToString() ) )
                {
                    continue;
                }

                string subCategory = page.Bag[pageKey].ToString();
                if(
                    siteContext.Categories.Any(
                        c => c.Name.Equals( subCategory, StringComparison.InvariantCultureIgnoreCase )
                    )
                )
                {
                    throw new InvalidOperationException(
                        $"Sub-category '{subCategory}' labeled as category on page {page.File}"
                    );
                }

                list.Add( page.Bag[pageKey].ToString() );
            }

            return list;
        }
    }
}
