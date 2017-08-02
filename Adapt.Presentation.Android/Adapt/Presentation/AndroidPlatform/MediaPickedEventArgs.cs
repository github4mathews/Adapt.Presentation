using System;
using System.Threading.Tasks;

namespace Adapt.Presentation.AndroidPlatform
{
    internal class MediaPickedEventArgs
        : EventArgs
    {
        public MediaPickedEventArgs(int id, Exception error)
        {
            RequestId = id;
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }

        public MediaPickedEventArgs(int id, bool isCanceled, MediaFile media = null)
        {
            RequestId = id;
            IsCanceled = isCanceled;
            if (!IsCanceled && media == null)
            {
                throw new ArgumentNullException(nameof(media));
            }

            Media = media;
        }

        public int RequestId
        {
            get;
            private set;
        }

        public bool IsCanceled
        {
            get;
            private set;
        }

        public Exception Error
        {
            get;
            private set;
        }

        public MediaFile Media
        {
            get;
            private set;
        }

        public Task<MediaFile> ToTask()
        {
            var tcs = new TaskCompletionSource<MediaFile>();

            if (IsCanceled)
            {
                tcs.SetResult(null);
            }
            else if (Error != null)
            {
                tcs.SetException(Error);
            }
            else
            {
                tcs.SetResult(Media);
            }

            return tcs.Task;
        }


    }
}