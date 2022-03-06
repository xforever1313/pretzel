//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions
{
    public static class PageExtensions
    {
        public static string GetAuthor( this Page page, string defaultAuthor = "Anonymous" )
        {
            const string key = "author";

            if( page.Bag.ContainsKey( key ) == false )
            {
                return defaultAuthor ?? string.Empty;
            }
            else
            {
                return page.Bag[key].ToString();
            }
        }

        public static string GetExerpt( this Page page )
        {
            const string key = "excerpt";

            if( page.Bag.ContainsKey( key ) )
            {
                return page.Bag[key].ToString();
            }
            else
            {
                return page.Content;
            }
        }
    }
}
