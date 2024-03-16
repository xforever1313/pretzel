//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System.IO;

namespace Pretzel.SethExtensions.ImageGallery
{
    public sealed class ImageInfoContext
    {
        // ---------------- Constructor ----------------

        public ImageInfoContext(
            FileInfo originalPhotoFilePath,
            string originalPhotoUrl,
            int originalWidth,
            int originalHeight,
            string thumbnailPageUrl,
            int thumbnailWidth,
            int thumbnailHeight,
            ImageInfo imageInfo
        )
        {
            this.OriginalPhotoFilePath = originalPhotoFilePath;
            this.OriginalPhotoUrl = originalPhotoUrl;
            this.OriginalWidth = originalWidth;
            this.OriginalHeight = originalHeight;
            this.ThumbnailUrl = thumbnailPageUrl;
            this.ThumbnailWidth = thumbnailWidth;
            this.ThumbnailHeight = thumbnailHeight;
            this.ImageInfo = imageInfo;
        }

        // ---------------- Properties ----------------

        public FileInfo OriginalPhotoFilePath { get; }

        public string OriginalPhotoUrl { get; }

        public int OriginalWidth { get; }

        public int OriginalHeight { get; }

        public string ThumbnailUrl { get; }

        public int ThumbnailWidth { get; }

        public int ThumbnailHeight { get; }

        public ImageInfo ImageInfo { get; }
    }
}
