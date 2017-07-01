using System;

namespace Adapt.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class StoreCameraMediaOptions
        : StoreMediaOptions
    {
        /// <summary>
        /// Allow cropping on photos and trimming on videos
        /// If null will use default
        /// Photo: UWP cropping can only be disabled on full size
        /// Video: UWP trimming when disabled won't allow time limit to be set
        /// </summary>
        public bool? AllowCropping { get; } = null;

        /// <summary>
        /// Default camera
        /// Should work on iOS and Windows, but not guaranteed on Android as not every camera implements it
        /// </summary>
        public CameraDevice DefaultCamera
        {
            get;
            set;
        }

        /// <summary>
        /// Get or set for an OverlayViewProvider
        /// </summary>
        public Func<object> OverlayViewProvider
        {
            get;
            set;
        }

        /// <summary>
        // Get or set if the image should be stored public
        /// </summary>
        public bool SaveToAlbum
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the size of the photo.
        /// </summary>
        /// <value>The size of the photo.</value>
        public PhotoSize PhotoSize { get; set; } = PhotoSize.Full;


        private int _CustomPhotoSize = 100;
        /// <summary>
        /// The custom photo size to use, 100 full size (same as Full),
        /// and 1 being smallest size at 1% of original
        /// Default is 100
        /// </summary>
        public int CustomPhotoSize
        {
            get { return _CustomPhotoSize; }
            set
            {
                if (value > 100)
                    _CustomPhotoSize = 100;
                else if (value < 1)
                    _CustomPhotoSize = 1;
                else
                    _CustomPhotoSize = value;
            }
        }


        private int _Quality = 100;
        /// <summary>
        /// The compression quality to use, 0 is the maximum compression (worse quality),
        /// and 100 minimum compression (best quality)
        /// Default is 100
        /// </summary>
        public int CompressionQuality
        {
            get { return _Quality; }
            set
            {
                if (value > 100)
                    _Quality = 100;
                else if (value < 0)
                    _Quality = 0;
                else
                    _Quality = value;
            }
        }

    }
}