// Pretzel.Categories plugin
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Categories
{
    [Export( typeof( IBeforeProcessingTransform ) )]
    public class CategoriesFolderGenerator : BaseFolderGenerator
    {
        public CategoriesFolderGenerator()
            : base("category")
        {
        }

        protected override IEnumerable<string> GetNames(SiteContext siteContext) => siteContext.Categories.Select(t => t.Name);
    }
}
