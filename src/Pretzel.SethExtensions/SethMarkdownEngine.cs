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
using Pretzel.Logic.Extensibility;

namespace Pretzel.SethExtensions
{
    #if !NO_EXPORT_SETH_MARKDOWN_ENGINE
    [Export( typeof( ILightweightMarkupEngine ) )]
    #endif
    public sealed class SethMarkdownEngine : ILightweightMarkupEngine
    {
        public string Convert( string source )
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Use<SethUrlMarkdownExtension>()
                .Build();

            return Markdown.ToHtml( source, pipeline );
        }
    }
}
