//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System.Collections.Generic;
using System;
using System.Composition;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace Pretzel.SethExtensions
{
    public class SethUrlMarkdownExtension : IMarkdownExtension
    {
        public void Setup( MarkdownPipelineBuilder pipeline )
        {
        }

        public void Setup( MarkdownPipeline pipeline, IMarkdownRenderer renderer )
        {
            var htmlRender = renderer as HtmlRenderer;
            if( htmlRender is null )
            {
                return;
            }

            var linkRender = renderer.ObjectRenderers.FindExact<LinkInlineRenderer>();
            if( linkRender is not null )
            {
                linkRender.TryWriters.Remove( TryLinkInlineRenderer );
                linkRender.TryWriters.Add( TryLinkInlineRenderer );
            }

            var autoLinkRender = renderer.ObjectRenderers.Find<AutolinkInlineRenderer>();
            if( autoLinkRender is not null )
            {
                autoLinkRender.TryWriters.Remove( TryAutoLinkInlineRenderer );
                autoLinkRender.TryWriters.Add( TryAutoLinkInlineRenderer );
            }
        }

        private bool TryLinkInlineRenderer( HtmlRenderer renderer, LinkInline linkInline )
        {
            if( linkInline.Url is null )
            {
                return false;
            }

            // Only process absolute Uri
            if(
                ( Uri.TryCreate( linkInline.Url, UriKind.RelativeOrAbsolute, out Uri? uri ) == false ) ||
                ( uri.IsAbsoluteUri == false )
            )
            {
                return false;
            }

            linkInline.SetAttributes(
                new HtmlAttributes()
                {
                    Properties = new List<KeyValuePair<string, string?>>()
                    {
                        new KeyValuePair<string, string?>( "target", "_blank" ),
                        new KeyValuePair<string, string?>( "rel", SethHtmlFormatter.ATagRelProperties )
                    }
                }
            );

            return false;
        }

        private bool TryAutoLinkInlineRenderer( HtmlRenderer renderer, AutolinkInline linkInline )
        {
            if( linkInline.Url is null )
            {
                return false;
            }

            // Only process absolute Uri
            if(
                ( Uri.TryCreate( linkInline.Url, UriKind.RelativeOrAbsolute, out Uri? uri ) == false ) ||
                ( uri.IsAbsoluteUri == false )
            )
            {
                return false;
            }

            linkInline.SetAttributes(
                new HtmlAttributes()
                {
                    Properties = new List<KeyValuePair<string, string?>>()
                    {
                        new KeyValuePair<string, string?>( "target", "_blank" ),
                        new KeyValuePair<string, string?>( "rel", SethHtmlFormatter.ATagRelProperties )
                    }
                }
            );

            return false;
        }
    }
}