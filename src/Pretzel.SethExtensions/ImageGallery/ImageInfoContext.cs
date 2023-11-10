//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

namespace Pretzel.SethExtensions.ImageGallery
{
    public sealed class ImageInfoContext
    {
        // ---------------- Constructor ----------------

        public ImageInfoContext(
            string originalPhotoUrl,
            string thumbnailPageUrl,
            int thumbnailWidth,
            int thumbnailHeight,
            ImageInfo imageInfo
        )
        {
            this.OriginalPhotoUrl = originalPhotoUrl;
            this.ThumbnailUrl = thumbnailPageUrl;
            this.ThumbnailWidth = thumbnailWidth;
            this.ThumbnailHeight = thumbnailHeight;
            this.ImageInfo = imageInfo;
        }

        // ---------------- Properties ----------------

        public string OriginalPhotoUrl { get; private set; }

        public string ThumbnailUrl { get; private set; }

        public int ThumbnailWidth { get; private set; }

        public int ThumbnailHeight { get; private set; }

        public ImageInfo ImageInfo { get; private set; }
    }
}
