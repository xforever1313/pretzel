//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System.IO;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ImageGallery
{
    public static class SiteExtensions
    {
        // ---------------- Fields ----------------

        private const string imageThumbNailWorkSetting = "thumbnail_work_dir";

        // ---------------- Functions ----------------

        public static DirectoryInfo GetThumbnailWorkFolder( this SiteContext context )
        {
            string thumbnailWorkSetting = "_thumbnails";
            if( context.Config.ContainsKey( imageThumbNailWorkSetting ) )
            {
                string? imageThumbnailWorkValue = context.Config[imageThumbNailWorkSetting].ToString();
                if( string.IsNullOrWhiteSpace( imageThumbnailWorkValue ) == false )
                {
                    thumbnailWorkSetting = imageThumbnailWorkValue;
                }
            }

            return new DirectoryInfo( Path.Combine( context.SourceFolder, thumbnailWorkSetting ) );
        }
    }
}
