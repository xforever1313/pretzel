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

        public static CategoryPage GetTopLevelCategory(
            this SiteContext siteContext,
            string categoryName
        )
        {
            ZCategoryCache cache = ZCategoryCache.CurrentCache;
            CategoryPage category = cache.CategoryPages.FirstOrDefault( c => c.CategoryName.Equals( categoryName ) );
            if( category is null )
            {
                throw new ArgumentException(
                    $"Can not find category {categoryName}",
                    nameof( categoryName )
                );
            }

            return category;
        }

        public static IEnumerable<Page> GetPostsFromSubCategory(
            this SiteContext siteContext,
            string subCategory
        )
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
