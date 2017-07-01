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
using System.IO;


namespace Adapt.Presentation
{
    /// <summary>
    /// Media file representations
    /// </summary>
    public sealed class MediaFile
      : IDisposable
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public MediaFile(string path, Func<Stream> streamGetter, string albumPath = null)
        {
            _StreamGetter = streamGetter;
            _Path = path;
            _AlbumPath = albumPath;
        }
        /// <summary>
        /// Path to file
        /// </summary>
        public string Path
        {
            get
            {
                if (_IsDisposed)
                {
                    throw new ObjectDisposedException(null);
                }

                return _Path;
            }
        }

        /// <summary>
        /// Path to file
        /// </summary>
        public string AlbumPath
        {
            get
            {
                if (_IsDisposed)
                    throw new ObjectDisposedException(null);

                return _AlbumPath;
            }
            set
            {
                if (_IsDisposed)
                    throw new ObjectDisposedException(null);

                _AlbumPath = value;
            }
        }

        /// <summary>
        /// Get stream if available
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            if (_IsDisposed)
                throw new ObjectDisposedException(null);

            return _StreamGetter();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _IsDisposed;
        private Func<Stream> _StreamGetter;
        private readonly string _Path;
        private string _AlbumPath;

        private void Dispose(bool disposing)
        {
            if (_IsDisposed)
                return;

            _IsDisposed = true;
			if(disposing)
				_StreamGetter = null;
        }
        /// <summary>
        /// 
        /// </summary>
        ~MediaFile()
        {
            Dispose(false);
        }
    }
}
