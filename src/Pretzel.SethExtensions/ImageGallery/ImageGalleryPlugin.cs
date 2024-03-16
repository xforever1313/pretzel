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

        // ---------------- Functions ----------------

        public void Transform( SiteContext context )
        {
            DirectoryInfo thumbNailFolder = context.GetThumbnailWorkFolder();
            if( thumbNailFolder.Exists == false )
            {
                Directory.CreateDirectory( thumbNailFolder.FullName );
            }

            foreach( Page page in context.Posts )
            {
                AddGallery( context, thumbNailFolder, page );
            }

            foreach( Page page in new List<Page>( context.Pages ) )
            {
                AddGallery( context, thumbNailFolder, page );
            }
        }

        private void AddGallery( SiteContext context, DirectoryInfo thumbNailFolder, Page page )
        {
            if( page.Bag.ContainsKey( imageGallerySetting ) == false )
            {
                return;
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

            FileInfo? defaultImage = null;
            if( string.IsNullOrWhiteSpace( config.DefaultImagePath ) == false )
            {
                defaultImage = new FileInfo(
                    Path.Combine( context.SourceFolder, config.InputImageDirectory, config.DefaultImagePath )
                );
            }

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

                    if( Path.Exists( originalFile ) == false )
                    {
                        if( defaultImage is null )
                        {
                            throw new FileNotFoundException(
                                $"Can not find image '{originalFile}', and {nameof( config.DefaultImagePath )} is not specified."
                            );
                        }
                        else
                        {
                            Console.WriteLine( $"\t-Warning: '{originalFile}' does not exist, using default image." );
                            originalFile = Path.GetFullPath( defaultImage.FullName );
                        }
                    }

                    using( var fileStream = new FileStream( originalFile, FileMode.Open, FileAccess.Read ) )
                    using( var image = new MagickImage( fileStream ) )
                    {
                        int thumbnailHeight = (int)Math.Round( image.Height * ( imageInfo.ThumbnailScale ?? config.ThumbnailScale ) );
                        int thumbnailWidth = (int)Math.Round( image.Width * ( imageInfo.ThumbnailScale ?? config.ThumbnailScale ) );
                        var size = new MagickGeometry( thumbnailWidth, thumbnailHeight );

                        int orignialWidth = image.BaseWidth;
                        int originalHeight = image.BaseHeight;

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
                        thumbnailPage.Bag["layout"] = "nil";

                        Page? originalPhotoPage = null;
                        lock( context )
                        {
                            thumbnailPage.Url = linkHelper.EvaluateLink( context, thumbnailPage );
                            context.Pages.Add( thumbnailPage );
                            foreach( Page pageToLook in context.Pages )
                            {
                                if( pageToLook.File == originalFile )
                                {
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
                            new FileInfo( originalFile ),
                            linkHelper.EvaluateLink( context, originalPhotoPage ),
                            orignialWidth,
                            originalHeight,
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
