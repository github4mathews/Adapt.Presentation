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
using System.Globalization;
using System.IO;

namespace Adapt.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public static class MediaExtensions
    {
        public static void VerifyOptions(this StoreMediaOptions self)
        {
            if (self == null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            if (Path.IsPathRooted(self.Directory))
            {
                throw new ArgumentException("options.Directory must be a relative path", nameof(self));
            }
        }

        public static string GetFilePath(this StoreMediaOptions self, string rootPath)
        {
            var isPhoto = !(self is StoreVideoOptions);

            var name = self?.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
                if (isPhoto)
                {
                    name = "IMG_" + timestamp + ".jpg";
                }
                else
                {
                    name = "VID_" + timestamp + ".mp4";
                }
            }

            var ext = Path.GetExtension(name);
            if (string.IsNullOrEmpty(ext))
            {
                ext = isPhoto ? ".jpg" : ".mp4";
            }

            name = Path.GetFileNameWithoutExtension(name);

            var folder = Path.Combine(rootPath ?? string.Empty, self?.Directory ?? string.Empty);

            return Path.Combine(folder, name + ext);
        }
    }
}
