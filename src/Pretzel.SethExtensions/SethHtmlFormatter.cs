//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using CommonMark;
using CommonMark.Syntax;

namespace Pretzel.SethExtensions
{
    /// <summary>
    /// Inspired from: https://github.com/Knagis/CommonMark.NET/wiki#htmlformatter
    /// </summary>
    public class SethHtmlFormatter : CommonMark.Formatters.HtmlFormatter
    {
        // ---------------- Constructor ----------------

        public SethHtmlFormatter( System.IO.TextWriter target, CommonMarkSettings settings )
            : base( target, settings )
        {
        }

        // ---------------- Functions ----------------

        protected sealed override void WriteInline( Inline inline, bool isOpening, bool isClosing, out bool ignoreChildNodes )
        {
            if(
                // verify that the inline element is one that should be modified
                ( inline.Tag == InlineTag.Link ) &&
                // verify that the formatter should output HTML and not plain text
                ( this.RenderPlainTextInlines.Peek() == false )
            )
            {
                WriteInlineLinkTag( inline, isOpening, isClosing, out ignoreChildNodes );
            }
            else
            {
                base.WriteInline( inline, isOpening, isClosing, out ignoreChildNodes );
            }
        }

        /// <summary>
        /// Override this to change up how to handle anchor tags.
        /// By default, if we are an outgoing link, we include target=_blank and nofollow.
        /// </summary>
        protected virtual void WriteInlineLinkTag( Inline inline, bool isOpening, bool isClosing, out bool ignoreChildNodes )
        {
            // If our URL starts with http, its probably outgoing,
            // so add target=_blank, but also other security tags
            // see also: https://www.pixelstech.net/article/1537002042-The-danger-of-target=_blank-and-opener
            if( inline.TargetUrl.StartsWith( "http" ) )
            {
                // instruct the formatter to process all nested nodes automatically
                ignoreChildNodes = false;

                // start and end of each node may be visited separately
                if( isOpening )
                {
                    this.Write( "<a target=\"_blank\" rel=\"noopener noreferrer nofollow\" href=\"" );
                    this.WriteEncodedUrl( inline.TargetUrl );
                    this.Write( "\">" );
                }

                // note that isOpening and isClosing can be true at the same time
                if( isClosing )
                {
                    this.Write( "</a>" );
                }
            }
            else
            {
                // in all other cases the default implementation will output the correct HTML
                base.WriteInline( inline, isOpening, isClosing, out ignoreChildNodes );
            }
        }
    }
}
