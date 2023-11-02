//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using System.Composition;
using System.IO;
using System.Threading.Tasks;
using BitMiracle.LibJpeg;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ImageGallery
{
    [Export( typeof( IBeforeProcessingTransform ) )]
    public sealed class ImageGalleryPlugin : IBeforeProcessingTransform
    {
        // ---------------- Fields ----------------

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

                        using( var fileStream = new FileStream( originalFile, FileMode.Open, FileAccess.Read ) )
                        using( var image = new JpegImage( fileStream ) )
                        {
                            int thumbnailHeight = (int)Math.Round( image.Height * config.ThumbnailScale );
                            int thumbnailWidth = (int)Math.Round( image.Width * config.ThumbnailScale );

                            // TODO: new image.
                            var bytes = new byte[] { 1, 2, 3 };
                            string inFile = Path.Combine( thumbNailFolder.FullName, imageInfo.ThumbnailFileName );
                            File.WriteAllBytes( inFile, bytes );

                            var page = new Page
                            {
                                Id = $"{config.Id}-{imageInfo.FileName}",
                                File = inFile,
                                Filepath = Path.Combine( context.OutputFolder, config.ThumbnailOutputFolder, imageInfo.ThumbnailFileName ),
                                OutputFile = Path.Combine( context.OutputFolder, config.ThumbnailOutputFolder, imageInfo.ThumbnailFileName )
                            };
                            lock( context )
                            {
                                page.Url = linkHelper.EvaluateLink( context, page );
                                context.Pages.Add( page );
                            }
                        }
                    }
                );
                Console.WriteLine( $"Generating thumbnails for image gallery on page: {page.Id}...Done!" );
            }
        }
    }
}
