//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ImageGallery
{
    public sealed class ImageInfoContext
    {
        // ---------------- Constructor ----------------

        public ImageInfoContext( Page originalPhoto, Page thumbnailPage, ImageInfo imageInfo )
        {
            this.OriginalPhoto = originalPhoto;
            this.ThumbnailPage = thumbnailPage;
            this.ImageInfo = imageInfo;
        }

        // ---------------- Properties ----------------

        public Page OriginalPhoto { get; private set; }

        public Page ThumbnailPage { get; private set; }

        public ImageInfo ImageInfo { get; private set; }
    }
}
