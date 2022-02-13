using System;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Categories
{
    public static class SiteContextExtensions
    {
        private const string configKey = "enable_subcategories";

        public static bool SubcategoriesEnabled( this SiteContext siteContext )
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
    }
}
