//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ImageGallery
{
    public static class PageContextExtensions
    {
        public static ImageGalleryConfig GetImageGalleryConfig( this PageContext page )
        {
            if( page.Bag.ContainsKey( ImageGalleryPlugin.ImageGalleryDataKey ) == false )
            {
                throw new ArgumentException( $"{page.Page.Id} does not contain Image Gallery Data", nameof( page ) );
            }

            if( page.Bag[ImageGalleryPlugin.ImageGalleryDataKey] is ImageGalleryConfig imageGalleryConfig )
            {
                return imageGalleryConfig;
            }
            else
            {
                throw new ArgumentException( $"{page.Page.Id} does not contain Image Gallery Data, it is of the wrong type or null.", nameof( page ) );
            }
        }
    }
}
