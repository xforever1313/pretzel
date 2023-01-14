using Markdig;

namespace Pretzel.Logic.Extensibility.Extensions
{
    public class MarkDigEngine : ILightweightMarkupEngine
    {
        public string Convert(string source)
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            return Markdown.ToHtml( source, pipeline );
        }
    }
}
