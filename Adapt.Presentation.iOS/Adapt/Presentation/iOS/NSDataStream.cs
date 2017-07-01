//
//  Copyright 2012, Xamarin Inc.
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

using System;
using System.IO;
using Foundation;
using System.Runtime.InteropServices;

namespace Adapt.Presentation.iOS
{
    internal class NsDataStream : Stream
    {
        private NSData _TheData;
        private uint _Pos;

        public NsDataStream(NSData data)
        {
            _TheData = data;
        }

        protected override void Dispose(bool disposing)
        {
            if (_TheData == null)
            {
                return;
            }

            _TheData.Dispose();
            _TheData = null;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_Pos >= _TheData.Length)
            {
                return 0;
            }
            var len = (int)Math.Min (count, (double)(_TheData.Length - _Pos));
            Marshal.Copy(new IntPtr(_TheData.Bytes.ToInt64() + _Pos), buffer, offset, len);
            _Pos += (uint)len;
            return len;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => (long) _TheData.Length;

        public override long Position
        {
            get
            {
                return _Pos;
            }
            set
            {
            }
        }
    }
}
