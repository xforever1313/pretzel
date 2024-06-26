using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using Pretzel.Logic.Exceptions;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensibility.Extensions;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Templating
{
    public abstract class JekyllEngineBase : ISiteEngine
    {
        private static readonly Regex paragraphRegex = new Regex(@"(<(?:p|h\d{1})>.*?</(?:p|h\d{1})>)", RegexOptions.Compiled | RegexOptions.Singleline);
        protected SiteContext Context;

#pragma warning disable 0649

        [Import]
        public IFileSystem FileSystem { get; set; }

#pragma warning restore 0649

        [ImportMany]
        public IEnumerable<IFilter> Filters { get; set; }

        [ImportMany]
        public IEnumerable<ITag> Tags { get; set; }

        [ImportMany]
        public IEnumerable<TagFactoryBase> TagFactories { get; set; }

        [ImportMany]
        public IEnumerable<IContentTransform> ContentTransformers { get; set; }

        [Import(AllowDefault = true)]
        public ILightweightMarkupEngine LightweightMarkupEngine { get; set; }

        public abstract void Initialize();

        protected abstract void PreProcess();

        protected abstract string RenderTemplate(string content, PageContext pageData);

        public void Process(SiteContext context, bool skipFileOnError = false)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Default rendering engine
            if (LightweightMarkupEngine == null)
            {
                LightweightMarkupEngine = new MarkDigEngine();
            }

            Tracing.Debug("LightweightMarkupEngine: {0}", LightweightMarkupEngine.GetType().Name);

            Context = context;
            PreProcess();

            Tracing.Debug( $"Processing {context.Posts.Count} Posts..." );
            for (int index = 0; index < context.Posts.Count; index++)
            {
                var p = context.Posts[index];
                var previous = GetPrevious(context.Posts, index);
                var next = GetNext(context.Posts, index);
                ProcessFile(context.OutputFolder, p, previous, next, skipFileOnError, p.Filepath);
            }
            Tracing.Debug( $"Processing {context.Posts.Count} Posts... Done!" );

            Tracing.Debug( $"Processing {context.Pages.Count} Pages..." );
            for (int index = 0; index < context.Pages.Count; index++)
            {
                var p = context.Pages[index];
                var previous = GetPrevious(context.Pages, index);
                var next = GetNext(context.Pages, index);
                ProcessFile(context.OutputFolder, p, previous, next, skipFileOnError);
            }
            Tracing.Debug( $"Processing {context.Pages.Count} Pages... Done!" );

            Tracing.Debug( $"Total Time: {stopwatch.Elapsed.TotalSeconds} seconds." );
        }

        private static Page GetPrevious(IList<Page> pages, int index)
        {
            return index < pages.Count - 1 ? pages[index + 1] : null;
        }

        private static Page GetNext(IList<Page> pages, int index)
        {
            return index >= 1 ? pages[index - 1] : null;
        }

        private void ProcessFile(string outputDirectory, Page page, Page previous, Page next, bool skipFileOnError, string relativePath = "")
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                relativePath = MapToOutputPath(page.File);

            if( string.IsNullOrWhiteSpace( page.OutputFile ) )
            {
                page.OutputFile = Path.Combine( outputDirectory, relativePath );
            }

            var extension = Path.GetExtension(page.File);

            if (extension.IsImageFormat())
            {
                CreateOutputDirectory(page.OutputFile);
                CopyFileIfSourceNewer(page.File, page.OutputFile, true);
                return;
            }

            if (page is NonProcessedPage)
            {
                CreateOutputDirectory(page.OutputFile);
                CopyFileIfSourceNewer(page.File, page.OutputFile, true);
                return;
            }

            if( page is RawPage )
            {
                CreateOutputDirectory( page.OutputFile );
                FileSystem.File.WriteAllText( page.OutputFile, page.Content );
                return;
            }

            if (extension.IsMarkdownFile() || extension.IsRazorFile())
            {
                page.OutputFile = page.OutputFile.Replace(extension, ".html");
            }

            var pageContext = PageContext.FromPage(Context, page, outputDirectory, page.OutputFile);

            pageContext.Previous = previous;
            pageContext.Next = next;

            var pageContexts = new List<PageContext> { pageContext };
            object paginateObj;
            if (page.Bag.TryGetValue("paginate", out paginateObj))
            {
                var paginate = Convert.ToInt32(paginateObj);
                var totalPages = (int)Math.Ceiling(Context.Posts.Count / Convert.ToDouble(paginateObj));
                var paginator = new Paginator(Context, totalPages, paginate, 1);
                pageContext.Paginator = paginator;

                var paginateLink = "/page/:page/index.html";
                if (page.Bag.ContainsKey("paginate_link"))
                    paginateLink = Convert.ToString(page.Bag["paginate_link"]);

                var prevLink = page.Url;
                for (var i = 2; i <= totalPages; i++)
                {
                    var newPaginator = new Paginator(Context, totalPages, paginate, i) { PreviousPageUrl = prevLink };
                    var link = paginateLink.Replace(":page", Convert.ToString(i));
                    paginator.NextPageUrl = link;

                    paginator = newPaginator;
                    prevLink = link;

                    var path = Path.Combine(outputDirectory, link.ToRelativeFile());
                    if (path.EndsWith(FileSystem.Path.DirectorySeparatorChar.ToString()))
                    {
                        path = Path.Combine(path, "index.html");
                    }
                    var context = new PageContext(pageContext) { Paginator = newPaginator, OutputPath = path };
                    context.Bag["url"] = link;
                    pageContexts.Add(context);
                }
            }

            foreach (var context in pageContexts)
            {
                var metadata = page.Bag;
                var failed = false;

                var excerptSeparator = context.Bag.ContainsKey("excerpt_separator")
                    ? context.Bag["excerpt_separator"].ToString()
                    : Context.ExcerptSeparator;
                try
                {
                    context.Content = RenderContent(page.File, RenderTemplate(context.Content, context));
                    context.FullContent = context.Content;
                    context.Bag["excerpt"] = GetContentExcerpt(context.Content, excerptSeparator);

                    // Don't override the excerpt if one already exists.
                    if( context.Page.Bag.ContainsKey( "excerpt" ) == false )
                    {
                        context.Page.Bag["excerpt"] = context.Bag["excerpt"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    if (!skipFileOnError)
                    {
                        var message = string.Format("Failed to process {0}, see inner exception for more details", context.OutputPath);
                        throw new PageProcessingException(message, ex);
                    }

                    Console.WriteLine(@"Failed to process {0}: {1}", context.OutputPath, ex);
                    continue;
                }

                while (metadata.ContainsKey("layout"))
                {
                    var layout = metadata["layout"];
                    if( (string)layout == "nil" || layout == null )
                    {
                        break;
                    }

                    string path;
                    if( metadata.ContainsKey( "layout_is_path" ) &&
                        "true".Equals( metadata["layout_is_path"].ToString(), StringComparison.InvariantCultureIgnoreCase )
                    )
                    {
                        path = layout.ToString();
                    }
                    else
                    {
                        path = FindLayoutPath( layout.ToString() );
                    }

                    if( path == null )
                    {
                        break;
                    }

                    try
                    {
                        metadata = ProcessTemplate(context, path);
                    }
                    catch (Exception ex)
                    {
                        if (!skipFileOnError)
                        {
                            var message = string.Format("Failed to process layout {0} for {1}, see inner exception for more details", layout, context.OutputPath);
                            throw new PageProcessingException(message, ex);
                        }

                        Console.WriteLine(@"Failed to process layout {0} for {1} because '{2}'. Skipping file", layout, context.OutputPath, ex.Message);
                        failed = true;
                        break;
                    }
                }
                if (failed)
                {
                    continue;
                }

                CreateOutputDirectory(context.OutputPath);
                FileSystem.File.WriteAllText(context.OutputPath, context.FullContent);
            }
        }

        private string RenderContent(string file, string contents)
        {
            string html;
            try
            {
                var contentsWithoutHeader = contents.ExcludeHeader();

                html = Path.GetExtension(file).IsMarkdownFile()
                       ? LightweightMarkupEngine.Convert(contentsWithoutHeader).Trim()
                       : contentsWithoutHeader;

                if (ContentTransformers != null)
                {
                    html = ContentTransformers.Aggregate(html, (current, contentTransformer) => contentTransformer.Transform(current));
                }
            }
            catch (Exception e)
            {
                Tracing.Info("Error ({0}) converting {1}", e.Message, file);
                Tracing.Debug(e.ToString());
                html = String.Format("<p><b>Error converting markdown:</b><br />{0}</p><p>Original content:<br /><pre>{1}</pre></p>", e.Message, contents);
            }
            return html;
        }

        private static string GetContentExcerpt(string content, string excerptSeparator)
        {
            var excerptSeparatorIndex = content.IndexOf(excerptSeparator, StringComparison.InvariantCulture);
            string excerpt = null;
            if (excerptSeparatorIndex == -1)
            {
                var match = paragraphRegex.Match(content);
                if (match.Success)
                {
                    excerpt = match.Groups[1].Value;
                }
            }
            else
            {
                excerpt = content.Substring(0, excerptSeparatorIndex);
                if (excerpt.StartsWith("<p>") && !excerpt.EndsWith("</p>"))
                {
                    excerpt += "</p>";
                }
            }
            return excerpt;
        }

        public void CopyFileIfSourceNewer(string sourceFileName, string destFileName, bool overwrite)
        {
            if (!FileSystem.File.Exists(destFileName) ||
                FileSystem.File.GetLastWriteTime(sourceFileName) > FileSystem.File.GetLastWriteTime(destFileName))
            {
                FileSystem.File.Copy(sourceFileName, destFileName, overwrite);
            }
        }

        private void CreateOutputDirectory(string outputFile)
        {
            var directory = Path.GetDirectoryName(outputFile);
            if (!FileSystem.Directory.Exists(directory))
                FileSystem.Directory.CreateDirectory(directory);
        }

        private static readonly string[] layoutExtensions = { ".html", ".htm" };

        protected virtual string[] LayoutExtensions
        {
            get { return layoutExtensions; }
        }

        private IDictionary<string, object> ProcessTemplate(PageContext pageContext, string path)
        {
            var templateFile = FileSystem.File.ReadAllText(path);
            var metadata = templateFile.YamlHeader();
            var templateContent = templateFile.ExcludeHeader();

            pageContext.FullContent = RenderTemplate(templateContent, pageContext);

            return metadata;
        }

        private string MapToOutputPath(string file)
        {
            var temp = file.Replace(Context.SourceFolder, "")
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return temp;
        }

        public bool CanProcess(SiteContext context)
        {
            var engineInfo = GetType().GetCustomAttributes(typeof(SiteEngineInfoAttribute), true).SingleOrDefault() as SiteEngineInfoAttribute;
            if (engineInfo == null) return false;
            return context.Engine == engineInfo.Engine;
        }

        private string FindLayoutPath(string layout)
        {
            foreach (var extension in LayoutExtensions)
            {
                var path = Path.Combine(Context.SourceFolder, "_layouts", layout + extension);
                if (FileSystem.File.Exists(path))
                    return path;
            }

            return null;
        }
    }
}
