using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        public static IEnumerable<CategoryPage> GetCategoryPages( this SiteContext siteContext )
        {
            ZCategoryCache cache = ZCategoryCache.CurrentCache;
            return cache.CategoryPages;
        }

        /// <summary>
        /// Gets all of the posts from the given category.
        /// </summary>
        /// <returns>
        /// A dictionary that contains all of the categories.
        /// An empty string represents the passed in category.
        /// If the key contains a name, its a sub-category of that category.
        ///
        /// This will at the very least return a dictionary containing an empty string
        /// key.  The value may be empty, however.
        /// </returns>
        public static IReadOnlyDictionary<string, IEnumerable<Page>> GetPostsFromCategory(
            this SiteContext siteContext,
            string category
        )
        {
            ZCategoryCache cache = ZCategoryCache.CurrentCache;

            var dict = new Dictionary<string, List<Page>>();
            dict[string.Empty] = new List<Page>();

            if( cache.CategoryNames.ContainsKey( category ) == false )
            {
                throw new ArgumentException(
                    "Category not found: " + category,
                    nameof( category )
                );
            }

            var subCatgories = new HashSet<string>( cache.CategoryNames[category] );
            foreach( Page post in siteContext.Posts )
            {
                string pageCategory = post.TryGetCategory();
                if( string.IsNullOrWhiteSpace( pageCategory ) )
                {
                    continue;
                }

                string subCategory = post.TryGetSubCategory();
                // If there is no sub-category, it only goes in the top-level.
                // Add it to the top-level category.
                if( pageCategory.Equals( category ) && string.IsNullOrWhiteSpace( subCategory ) )
                {
                    subCategory = string.Empty;
                }
                else if( subCatgories.Contains( subCategory ) == false )
                {
                    continue;
                }
                else if( dict.ContainsKey( subCategory ) == false )
                {
                    dict[subCategory] = new List<Page>();
                }

                dict[subCategory].Add( post );
            }

            return new ReadOnlyDictionary<string, IEnumerable<Page>>(
                dict.ToDictionary( kv => kv.Key, kv => kv.Value.AsEnumerable() )
            );
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
