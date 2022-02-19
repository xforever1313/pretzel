using System.Collections.Generic;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Categories
{
    public class CategoryPage
    {
        // ---------------- Fields ----------------

        private readonly List<CategoryPage> subCategories;

        // ---------------- Constructor ----------------

        public CategoryPage()
        {
            this.subCategories = new List<CategoryPage>();
            this.SubCategories = this.subCategories.AsReadOnly();
        }

        // ---------------- Properties ----------------

        public string CategoryName { get; internal set; }

        public Page Page { get; internal set; }

        public string Url => this.Page.Url;

        public IEnumerable<CategoryPage> SubCategories { get; private set; }

        // ---------------- Functions ----------------

        internal void AddSubPage( CategoryPage subPage )
        {
            this.subCategories.Add( subPage );
        }
    }
}
