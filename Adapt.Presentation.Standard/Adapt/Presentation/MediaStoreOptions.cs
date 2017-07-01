//
//  Copyright 2011-2013, Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//

using System;

namespace Adapt.Presentation
{
    /// <summary>
    /// Media Options
    /// </summary>
    public class StoreMediaOptions
    {
        /// <summary>
        /// 
        /// </summary>
        protected StoreMediaOptions()
        {
        }

        /// <summary>
        /// Directory name
        /// </summary>
        public string Directory
        {
            get;
            set;
        }

        /// <summary>
        /// File name
        /// </summary>
        public string Name
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Camera device
    /// </summary>
    public enum CameraDevice
    {
        /// <summary>
        /// Back of device
        /// </summary>
        Rear,
        /// <summary>
        /// Front facing of device
        /// </summary>
        Front
    }

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


    /// <summary>
    /// Photo size enum.
    /// </summary>
    public enum PhotoSize
    {
        /// <summary>
        /// 25% of original
        /// </summary>
        Small,
        /// <summary>
        /// 50% of the original
        /// </summary>
        Medium,
        /// <summary>
        /// 75% of the original
        /// </summary>
        Large,
        /// <summary>
        /// Untouched
        /// </summary>
        Full,
        /// <summary>
        /// Custom size between 1-100
        /// Must set the CustomPhotoSize value
        /// Only applies to iOS and Android
        /// Windows will auto configure back to small, medium, large, and full
        /// </summary>
        Custom
    }

    /// <summary>
    /// Video quality
    /// </summary>
    public enum VideoQuality
    {
        /// <summary>
        /// Low
        /// </summary>
        Low = 0,
        /// <summary>
        /// Medium
        /// </summary>
        Medium = 1,
        /// <summary>
        /// High
        /// </summary>
        High = 2
    }

    /// <summary>
    /// Store Video options
    /// </summary>
    public class StoreVideoOptions
      : StoreCameraMediaOptions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public StoreVideoOptions()
        {
            Quality = VideoQuality.High;
            DesiredLength = TimeSpan.FromMinutes(10);
        }

        /// <summary>
        /// Desired Length
        /// </summary>
        public TimeSpan DesiredLength
        {
            get;
            set;
        }

        /// <summary>
        /// Desired Quality
        /// </summary>
        public VideoQuality Quality
        {
            get;
            set;
        }
    }
}
