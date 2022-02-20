// Pretzel.Categories plugin
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensibility.Extensions;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Categories
{
    [Export( typeof( IBeforeProcessingTransform ) )]
    public class SubCategoriesFolderGenerator : IBeforeProcessingTransform
    {
        // ---------------- Fields ----------------

        private const string pageKey = PageExtensions.SubCategoryPageKey;

        private const string categoryLayoutName = CategoriesFolderGenerator.CategoryFolderName;
        private const string subcategoryLayoutName = "subcategory";

        // ---------------- Constructor ----------------

        public SubCategoriesFolderGenerator()
        {
        }

        // ---------------- Functions ----------------

        public void Transform( SiteContext siteContext )
        {
            string layout = "layout";
            string layoutConfigKey = $"{subcategoryLayoutName}_pages_layout";

            if( siteContext.Config.ContainsKey( layoutConfigKey ) )
            {
                layout = siteContext.Config[layoutConfigKey].ToString();
            }

            Console.WriteLine( "Sub-categories enabled" );

            var dict = new Dictionary<string, List<string>>();
            foreach( Page post in siteContext.Posts )
            {
                string categoryName = post.TryGetCategory();
                string subCategoryName = post.TryGetSubCategory();

                if(
                    string.IsNullOrWhiteSpace( categoryName ) ||
                    string.IsNullOrWhiteSpace( subCategoryName )
                )
                {
                    continue;
                }

                if( dict.ContainsKey( categoryName ) == false )
                {
                    dict[categoryName] = new List<string>();
                }

                if( dict[categoryName].Contains( subCategoryName ) == false )
                {
                    dict[categoryName].Add( subCategoryName );
                }
            }

            foreach( KeyValuePair<string, List<string>> kvp in dict )
            {
                string category = kvp.Key;
                foreach( string subcat in kvp.Value )
                {
                    var p = new Page
                    {
                        Title = subcat,
                        Content = $"---\r\n layout: {layout} \r\n {categoryLayoutName}: {category} \r\n {subcategoryLayoutName}: {subcat} \r\n---\r\n",
                        File = Path.Combine( siteContext.SourceFolder, categoryLayoutName, SlugifyFilter.Slugify( category ), SlugifyFilter.Slugify( subcat ), "index.html" ),
                        Filepath = Path.Combine( siteContext.OutputFolder, categoryLayoutName, SlugifyFilter.Slugify( category ), SlugifyFilter.Slugify( subcat ), "index.html" ),
                        OutputFile = Path.Combine( siteContext.OutputFolder, categoryLayoutName, SlugifyFilter.Slugify( category ), SlugifyFilter.Slugify( subcat ), "index.html" ),
                        Bag = $"---\r\n layout: {layout} \r\n {categoryLayoutName}: {category} \r\n {subcategoryLayoutName}: {subcat} \r\n---\r\n".YamlHeader()
                    };

                    p.Url = new LinkHelper().EvaluateLink( siteContext, p );
                    siteContext.Pages.Add( p );
                }
            }
        }
    }
}
