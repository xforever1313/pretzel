// Pretzel.Categories plugin
using System.Collections.Generic;
using System.IO;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensibility.Extensions;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Categories
{
    public abstract class BaseFolderGenerator : IBeforeProcessingTransform
    {
        private readonly string folderName = string.Empty;
        private readonly string layoutName = string.Empty;

        private bool stopFolderGeneration;

        protected BaseFolderGenerator( string folderToGenerate ) :
            this( folderToGenerate, folderToGenerate )
        {
        }

        protected BaseFolderGenerator(string folderToGenerate, string layoutName)
        {
            this.folderName = folderToGenerate;
            this.layoutName = layoutName;
        }

        public string[] GetArguments(string command) => command == "taste" || command == "bake" ? new[] { $"n{this.folderName}" } : new string[0];

        public void Transform(SiteContext siteContext)
        {
            var layout = "layout";
            var layoutConfigKey = $"{this.layoutName}_pages_layout";

            if (this.stopFolderGeneration)
            {
                return;
            }

            if (siteContext.Config.ContainsKey(layoutConfigKey))
            {
                layout = siteContext.Config[layoutConfigKey].ToString();
            }

            foreach (var name in this.GetNames(siteContext))
            {
                var p = new Page
                {
                    Title = name,
                    Content = $"---\r\n layout: {layout} \r\n {this.layoutName}: {name} \r\n---\r\n",
                    File = Path.Combine(siteContext.SourceFolder, this.folderName, SlugifyFilter.Slugify(name), "index.html"),
                    Filepath = Path.Combine(siteContext.OutputFolder, this.folderName, SlugifyFilter.Slugify(name), "index.html"),
                    OutputFile = Path.Combine(siteContext.OutputFolder, this.folderName, SlugifyFilter.Slugify(name), "index.html"),
                    Bag = $"---\r\n layout: {layout} \r\n {this.layoutName}: {name} \r\n---\r\n".YamlHeader()
                };

                p.Url = new LinkHelper().EvaluateLink(siteContext, p);

                siteContext.Pages.Add(p);
            }
        }

#if false
        public void UpdateOptions(OptionSet options)
        {
            options.Add($"n{this.folderName}", $"Disable the {this.folderName} folder generation", v => this.stopFolderGeneration = v != null);
        }
#endif
        protected abstract IEnumerable<string> GetNames(SiteContext siteContext);
    }
}
