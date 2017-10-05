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

using Foundation;
using System;
using System.Threading.Tasks;
using UIKit;

namespace Adapt.Presentation.iOS
{
    /// <summary>
    /// Media Picker Controller
    /// </summary>
    public sealed class MediaPickerController : UIImagePickerController
    {
        #region Constructor
        internal MediaPickerController(NSObject mpDelegate)
        {
            base.Delegate = mpDelegate;
        }
        #endregion

        #region Public Overrides
        /// <summary>
        /// Deleage
        /// </summary>
        public override NSObject Delegate
        {
            get { return base.Delegate; }
            set
            {
                if (value == null)
                {
                    base.Delegate = null;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets result of picker
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public Task<MediaFile> GetResultAsync()
        {
            return ((MediaPickerDelegate)Delegate).Task;
        }
        #endregion
    }
}