//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions
{
    public static class SiteExtensions
    {
        /// <summary>
        /// Combines the given relative URL with the site's URL
        /// to generate the full URL.
        /// </summary>
        public static string UrlCombine( this SiteContext context, string urlLocation )
        {
            string url = context.Config["url"].ToString();
            url = url.TrimEnd( '/' );
            urlLocation = urlLocation.TrimStart( '/' );
            return $"{url}/{urlLocation}";
        }
    }
}
