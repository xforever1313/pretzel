using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
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

            // Make categories as strings.
            var categoriesAsStrings = new Dictionary<string, HashSet<string>>();
            foreach( Page post in siteContext.Posts )
            {
                string postSubcatgory = post.TryGetSubCategory();
                foreach( string category in post.Categories )
                {
                    if( categoriesAsStrings.ContainsKey( category ) == false )
                    {
                        categoriesAsStrings[category] = new HashSet<string>();
                    }

                    if( categoriesAsStrings[category].Contains( postSubcatgory ) == false )
                    {
                        if( string.IsNullOrWhiteSpace( postSubcatgory ) == false )
                        {
                            categoriesAsStrings[category].Add( postSubcatgory );
                        }
                    }
                }
            }

            // Make categories as pages.
            var categoriesAsPages = new Dictionary<Page, List<Page>>();
            var parentCategoriesAdded = new HashSet<string>();
            foreach( Page page in siteContext.Pages )
            {
                string pageCategory = page.TryGetCategory();
                if( string.IsNullOrWhiteSpace( pageCategory ) )
                {
                    continue;
                }

                if(
                    categoriesAsStrings.ContainsKey( pageCategory ) &&
                    ( parentCategoriesAdded.Contains( pageCategory ) == false )
                )
                {
                    categoriesAsPages[page] = new List<Page>();
                    parentCategoriesAdded.Add( pageCategory );
                }

                var subcategoriesAdded = new HashSet<string>();
                foreach( Page page2 in siteContext.Pages )
                {
                    string pageSubcategory = page2.TryGetSubCategory();
                    if( string.IsNullOrWhiteSpace( pageSubcategory ) )
                    {
                        continue;
                    }

                    if(
                        categoriesAsStrings[pageCategory].Contains( pageSubcategory ) &&
                        ( subcategoriesAdded.Contains( pageSubcategory ) == false )
                    )
                    {
                        categoriesAsPages[page].Add( page2 );
                        subcategoriesAdded.Add( pageSubcategory );
                    }
                }
            }

            this.CategoryNames = new ReadOnlyDictionary<string, IEnumerable<string>>(
                categoriesAsStrings.ToDictionary( kv => kv.Key, kv => kv.Value.AsEnumerable() )
            );

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
