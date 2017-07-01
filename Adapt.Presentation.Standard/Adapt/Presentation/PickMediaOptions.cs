namespace Adapt.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class PickMediaOptions
    {
        #region Fields
        private int _CustomPhotoSize = 100;
        #endregion

        /// <summary>
        /// Gets or sets the size of the photo.
        /// </summary>
        /// <value>The size of the photo.</value>
        public PhotoSize PhotoSize { get; } = PhotoSize.Full;

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