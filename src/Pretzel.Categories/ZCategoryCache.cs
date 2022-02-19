using System.Collections.Generic;
using System.Composition;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Categories
{
    /// <remarks>
    /// Hacky workaround.  Need to name with a "Z"
    /// since this must come last.
    /// </remarks>
    [Export( typeof( IBeforeProcessingTransform ) )]
    public class ZCategoryCache : IBeforeProcessingTransform
    {
        // ---------------- Constructor ---------------

        public ZCategoryCache()
        {
        }

        // ---------------- Properties ---------------

        public IReadOnlyDictionary<string, IEnumerable<string>> CategoryNames { get; private set; }

        public IEnumerable<CategoryPage> CategoryPages { get; private set; }

        public static ZCategoryCache CurrentCache { get; private set; }

        // ---------------- Functions ---------------

        public void Transform( SiteContext siteContext )
        {
            siteContext.ThrowIfSubCategoriesDisabled();

            // Make categories as pages.
            var categoriesAsPages = new Dictionary<Page, List<Page>>();

            void addSubPages( Page parentPage )
            {
                foreach( Page page in siteContext.Pages )
                {
                    string pageCategory = page.TryGetCategory();
                    string subCategory = page.TryGetSubCategory();

                    if( ReferenceEquals( parentPage, page ) )
                    {
                        continue;
                    }
                    else if( string.IsNullOrWhiteSpace( pageCategory ) )
                    {
                        continue;
                    }
                    else if( string.IsNullOrWhiteSpace( subCategory ) )
                    {
                        // If there's no sub-category, continue.
                        continue;
                    }

                    if( parentPage.TryGetCategory() == pageCategory )
                    {
                        categoriesAsPages[parentPage].Add( page );
                    }
                }
            }

            foreach( Page page in siteContext.Pages )
            {
                string pageCategory = page.TryGetCategory();
                string subCategory = page.TryGetSubCategory();

                if( string.IsNullOrWhiteSpace( pageCategory ) )
                {
                    continue;
                }
                else if( string.IsNullOrWhiteSpace( subCategory ) == false )
                {
                    // If there's a sub-category, its not a parent page.
                    continue;
                }

                categoriesAsPages[page] = new List<Page>();
                addSubPages( page );
            }

            var categoryPages = new List<CategoryPage>();
            foreach( var page in categoriesAsPages )
            {
                var topLevelPage = new CategoryPage
                {
                    CategoryName = page.Key.TryGetCategory(),
                    Page = page.Key,
                };
                foreach( var subPage in page.Value )
                {
                    var subCategoryPage = new CategoryPage
                    {
                        CategoryName = subPage.TryGetSubCategory(),
                        Page = subPage
                    };

                    topLevelPage.AddSubPage( subCategoryPage );
                }
                categoryPages.Add( topLevelPage );
            }

            foreach( Page post in siteContext.Posts )
            {
                string category = post.TryGetCategory();
                string subCat = post.TryGetSubCategory();
                foreach( CategoryPage topLevelCat in categoryPages )
                {
                    if( topLevelCat.CategoryName != category )
                    {
                        continue;
                    }

                    if( string.IsNullOrWhiteSpace( subCat ) )
                    {
                        // If there is no sub-category, just add
                        // it to the top-level category.
                        topLevelCat.AddPost( post );
                    }
                    else
                    {
                        foreach( CategoryPage subCatPage in topLevelCat.SubCategories )
                        {
                            if( subCatPage.CategoryName == subCat )
                            {
                                subCatPage.AddPost( post );
                            }
                        }
                    }
                }
            }

            this.CategoryPages = categoryPages.AsReadOnly();

            CurrentCache = this;
        }
    }
}
