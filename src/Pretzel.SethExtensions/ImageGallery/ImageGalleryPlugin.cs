//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ImageGallery
{
    [Export( typeof( IBeforeProcessingTransform ) )]
    public sealed class ImageGalleryPlugin : IBeforeProcessingTransform
    {
        // ---------------- Fields ----------------

        internal const string ImageGalleryDataKey = "image_gallery_data";

        private const string imageGallerySetting = "image_gallery";

        private const string imageThumbNailWorkSetting = "thumbnail_work_dir";

        // ---------------- Functions ----------------

        public void Transform( SiteContext context )
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

            DirectoryInfo thumbNailFolder = new DirectoryInfo( Path.Combine( context.SourceFolder, thumbnailWorkSetting ) );

            foreach( Page page in context.Posts )
            {
                if( page.Bag.ContainsKey( imageGallerySetting ) == false )
                {
                    continue;
                }

                string? imageGallerySettingValue = page.Bag[imageGallerySetting].ToString();
                if( string.IsNullOrEmpty( imageGallerySettingValue ) )
                {
                    throw new InvalidOperationException( $"No value specified in {imageGallerySetting} on page {page.Id}" );
                }

                var imageGalleryFile = new FileInfo(
                    Path.Combine( context.SourceFolder, imageGallerySettingValue )
                );

                ImageGalleryConfig config = ImageGalleryConfigExtensions.FromXml( imageGalleryFile );
                config.Validate();

                var linkHelper = new LinkHelper();

                var imageContentList = new List<ImageInfoContext>();

                Console.WriteLine( $"Generating thumbnails for image gallery on page: {page.Id}..." );
                Parallel.ForEach(
                    config.ImageInfo,
                    ( ImageInfo imageInfo ) =>
                    {
                        string originalFile = Path.Combine(
                            context.SourceFolder,
                            config.InputImageDirectory,
                            imageInfo.FileName
                        );

                        // GetFullPath normalizes separator characters.
                        originalFile = Path.GetFullPath( originalFile );

                        using( var fileStream = new FileStream( originalFile, FileMode.Open, FileAccess.Read ) )
                        using( var image = new MagickImage( fileStream ) )
                        {
                            int thumbnailHeight = (int)Math.Round( image.Height * config.ThumbnailScale );
                            int thumbnailWidth = (int)Math.Round( image.Width * config.ThumbnailScale );
                            var size = new MagickGeometry( thumbnailWidth, thumbnailHeight );

                            image.Resize( size );

                            string inFile = Path.Combine( thumbNailFolder.FullName, imageInfo.ThumbnailFileName );
                            image.Write( inFile );

                            var thumbnailPage = new Page
                            {
                                Id = $"{config.Id}-{imageInfo.FileName}",
                                File = inFile,
                                Filepath = Path.Combine( context.OutputFolder, config.ThumbnailOutputFolder, imageInfo.ThumbnailFileName ),
                                OutputFile = Path.Combine( context.OutputFolder, config.ThumbnailOutputFolder, imageInfo.ThumbnailFileName )
                            };

                            Page? originalPhotoPage = null;
                            lock( context )
                            {
                                thumbnailPage.Url = linkHelper.EvaluateLink( context, thumbnailPage );
                                context.Pages.Add( thumbnailPage );
                                foreach( Page pageToLook in context.Pages )
                                {
                                    if( pageToLook.File == originalFile )
                                    {
                                        Console.WriteLine( "Page file: " + pageToLook.File );
                                        originalPhotoPage = pageToLook;
                                        break;
                                    }
                                }
                            }

                            if( originalPhotoPage is null )
                            {
                                throw new InvalidOperationException(
                                    $"Can not locate original image to thumbnail: {imageInfo.FileName}"
                                );
                            }

                            var imageInfoContext = new ImageInfoContext(
                                linkHelper.EvaluateLink( context, originalPhotoPage ),
                                thumbnailPage.Url,
                                thumbnailWidth,
                                thumbnailHeight,
                                imageInfo
                            );
                            lock( imageContentList )
                            {
                                imageContentList.Add( imageInfoContext );
                            }
                        }
                    }
                );
                page.Bag[ImageGalleryDataKey] = imageContentList.OrderBy( i => i.ImageInfo.Index );
                Console.WriteLine( $"Generating thumbnails for image gallery on page: {page.Id}...Done!" );
            }
        }
    }
}
