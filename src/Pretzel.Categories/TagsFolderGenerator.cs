// Pretzel.Categories plugin
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Categories
{
    [Export( typeof( IBeforeProcessingTransform ) )]
    public class TagsFolderGenerator : BaseFolderGenerator
    {
        public TagsFolderGenerator()
            : base( "tag" )
        {
        }

        protected override IEnumerable<string> GetNames( SiteContext siteContext ) => siteContext.Tags.Select( t => t.Name );
    }
}
