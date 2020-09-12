using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;
using RazorEngineCore;

namespace Pretzel.Logic.Templating.Razor
{
    [Shared]
    [SiteEngineInfo(Engine = "razor")]
    public class RazorSiteEngine : JekyllEngineBase
    {
        private static readonly string[] layoutExtensions = { ".cshtml" };

        private string includesPath;

        private readonly List<ITag> _allTags = new List<ITag>();

        public override void Initialize()
        {
        }

        private class TagComparer : IEqualityComparer<ITag>
        {
            public bool Equals(ITag x, ITag y)
            {
                if (x == null || y == null)
                {
                    return false;
                }

                return x.Name == y.Name;
            }

            public int GetHashCode(ITag obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        protected override void PreProcess()
        {
            includesPath = Path.Combine(Context.SourceFolder, "_includes");

            if (Tags != null)
            {
                var toAdd = Tags.Except(_allTags, new TagComparer()).ToList();
                _allTags.AddRange(toAdd);
            }

            if (TagFactories != null)
            {
                var toAdd = TagFactories.Select(factory =>
                {
                    factory.Initialize(Context);
                    return factory.CreateTag();
                }).Except(_allTags, new TagComparer()).ToList();

                _allTags.AddRange(toAdd);
            }
        }

        protected override string[] LayoutExtensions
        {
            get { return layoutExtensions; }
        }

        protected override string RenderTemplate(string content, PageContext pageData)
        {
            try
            {
                IRazorEngine engine = new RazorEngine();

                content = Regex.Replace( content, "<p>(@model .*?)</p>", "$1" );

                var pageContent = pageData.Content;
                pageData.Content = pageData.FullContent;

                IEnumerable<AssemblyName> assemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();

                IRazorEngineCompiledTemplate template = engine.Compile(
                    content,
                    builder =>
                    {
                        foreach( AssemblyName assm in assemblies )
                        {
                            builder.AddAssemblyReferenceByName( assm.FullName );
                        }
                    }
                );
                content = template.Run( pageData );
                pageData.Content = pageContent;

                return content;
            }
            catch (Exception e)
            {
                Tracing.Error(
                    $"Failed to render template for page '{pageData.Page.Id}' for reason '{e.Message}', falling back to direct content"
                );
                Tracing.Debug(e.Message);
                Tracing.Debug(e.StackTrace);
                return content;
            }
        }
    }
}
