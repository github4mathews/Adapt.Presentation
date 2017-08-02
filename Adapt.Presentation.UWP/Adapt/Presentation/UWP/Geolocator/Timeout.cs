using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adapt.Presentation.UWP.Geolocator
{
    internal class Timeout : IDisposable
    {
        public Timeout(int timeout, Action timesup)
        {
            if (timeout == Infite)
            {
                return; // nothing to do
            }

            if (timeout < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout));
            }

            if (timesup == null)
            {
                throw new ArgumentNullException(nameof(timesup));
            }

            Task.Delay(TimeSpan.FromMilliseconds(timeout), _Canceller.Token)
                .ContinueWith(t =>
                {
                    if (!t.IsCanceled)
                    {
                        timesup();
                    }
                });
        }

        public void Cancel()
        {
            _Canceller.Cancel();
        }

        private readonly CancellationTokenSource _Canceller = new CancellationTokenSource();

        public const int Infite = -1;

        public void Dispose()
        {
            _Canceller?.Dispose();
        }
    }
}
