using System;
using System.Collections.Generic;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Categories
{
    public static class SiteContextExtensions
    {
        private const string configKey = "enable_subcategories";

        public static bool IsSubcategoriesEnabled( this SiteContext siteContext )
        {
            if( siteContext.Config.ContainsKey( configKey ) == false )
            {
                return false;
            }
            else if( "true".Equals( siteContext.Config[configKey].ToString(), StringComparison.InvariantCultureIgnoreCase ) == false )
            {
                return false;
            }

            return true;
        }

        public static void ThrowIfSubCategoriesDisabled( this SiteContext siteContext )
        {
            if( siteContext.IsSubcategoriesEnabled() == false )
            {
                throw new InvalidOperationException(
                    "Sub-categories disabled"
                );
            }
        }

        public static IDictionary<Page, IList<Page>> GetCategoryPages( this SiteContext siteContext )
        {
            siteContext.ThrowIfSubCategoriesDisabled();

            var subCategoriesAsStrings = new Dictionary<string, HashSet<string>>();

            var subCategories = new Dictionary<Page, IList<Page>>();

            foreach( Page post in siteContext.Posts )
            {
                string postSubcatgory = post.TryGetSubCategory();
                foreach( string category in post.Categories )
                {
                    if( subCategoriesAsStrings.ContainsKey( category ) == false )
                    {
                        subCategoriesAsStrings[category] = new HashSet<string>();
                    }

                    if( subCategoriesAsStrings[category].Contains( postSubcatgory ) == false )
                    {
                        if( string.IsNullOrWhiteSpace( postSubcatgory ) == false )
                        {
                            subCategoriesAsStrings[category].Add( postSubcatgory );
                        }
                    }
                }
            }

            var parentCategoriesAdded = new HashSet<string>();
            foreach( Page page in siteContext.Pages )
            {
                string pageCategory = page.TryGetCategory();
                if( string.IsNullOrWhiteSpace( pageCategory ) )
                {
                    continue;
                }

                if(
                    subCategoriesAsStrings.ContainsKey( pageCategory ) &&
                    ( parentCategoriesAdded.Contains( pageCategory ) == false )
                )
                {
                    subCategories[page] = new List<Page>();
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
                        subCategoriesAsStrings[pageCategory].Contains( pageSubcategory ) &&
                        ( subcategoriesAdded.Contains( pageSubcategory ) == false )
                    )
                    {
                        subCategories[page].Add( page2 );
                        subcategoriesAdded.Add( pageSubcategory );
                    }
                }
            }

            return subCategories;
        }

        public static IEnumerable<Page> GetPostsFromSubCategory( this SiteContext siteContext, string subCategory )
        {
            siteContext.ThrowIfSubCategoriesDisabled();

            var posts = new List<Page>();

            foreach( Page post in siteContext.Posts )
            {
                string pageSubCategory = post.TryGetSubCategory();
                if( string.IsNullOrWhiteSpace( pageSubCategory ) )
                {
                    continue;
                }

                if( subCategory.Equals( pageSubCategory ) )
                {
                    posts.Add( post );
                }
            }

            return posts;
        }
    }
}
