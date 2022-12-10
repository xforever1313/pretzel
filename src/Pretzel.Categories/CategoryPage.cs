using System.Collections.Generic;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Categories
{
    public class CategoryPage
    {
        // ---------------- Fields ----------------

        private readonly List<CategoryPage> subCategories;

        private readonly List<Page> posts;

        // ---------------- Constructor ----------------

        public CategoryPage()
        {
            this.subCategories = new List<CategoryPage>();
            this.SubCategories = this.subCategories.AsReadOnly();

            this.posts = new List<Page>();
            this.Posts = this.posts.AsReadOnly();
        }

        // ---------------- Properties ----------------

        public string CategoryName { get; internal set; }

        public Page Page { get; internal set; }

        public string Url => this.Page.Url;

        public IEnumerable<CategoryPage> SubCategories { get; private set; }

        public IEnumerable<Page> Posts { get; private set; }

        // ---------------- Functions ----------------

        internal void AddSubPage( CategoryPage subPage )
        {
            this.subCategories.Add( subPage );
        }

        internal void AddPost( Page post )
        {
            this.posts.Add( post );
        }
    }
}
