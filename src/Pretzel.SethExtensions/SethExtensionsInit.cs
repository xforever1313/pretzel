//
//          Copyright Seth Hendrick 2020.
// Distributed under the Microsoft Public License (MS-PL).
//

using System.Composition;
using CommonMark;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions
{
    /// <summary>
    /// Used to init my extensions to pretzel.
    /// </summary>
    [Export( typeof( IBeforeProcessingTransform ) )]
    public class SethExtensionsInit : IBeforeProcessingTransform
    {
        // ---------------- Constructor ----------------

        static SethExtensionsInit()
        {
            // Don't override if it is already set.
            if( CommonMarkSettings.Default.OutputDelegate == null )
            {
                CommonMarkSettings.Default.OutputDelegate =
                    ( doc, output, settings ) =>
                    new SethHtmlFormatter( output, settings ).WriteDocument( doc );
            }
        }

        // ---------------- Functions ----------------

        public void Transform( SiteContext context )
        {
            // Nothing to do.  Really, this is just here so the static constructor
            // gets called, and our custom HTML Formatter also gets called.
        }
    }
}
