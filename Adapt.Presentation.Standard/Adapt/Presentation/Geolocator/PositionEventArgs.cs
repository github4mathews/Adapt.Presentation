using System;

namespace Adapt.Presentation.Geolocator
{
    /// <summary>
    /// Position args
    /// </summary>
    public class PositionEventArgs
        : EventArgs
    {
        /// <summary>
        /// Position args
        /// </summary>
        public PositionEventArgs(Position position)
        {
            Position = position ?? throw new ArgumentNullException(nameof(position));
        }

        /// <summary>
        /// The Position
        /// </summary>
        public Position Position
        {
            get;
        }
    }
}