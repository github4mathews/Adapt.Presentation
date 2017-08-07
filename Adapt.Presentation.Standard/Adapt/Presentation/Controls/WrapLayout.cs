using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    /// <summary>
    /// A panel controls that renders its contents from left to right and wraps vertically
    /// </summary>
    public class WrapLayout : Layout<View>
    {
        #region Fields
        private readonly Dictionary<View, SizeRequest> _LayoutCache = new Dictionary<View, SizeRequest>();
        #endregion

        #region Bindable Properties
        /// <summary>
        /// Backing Storage for the Spacing property
        /// </summary>
        public static readonly BindableProperty SpacingProperty =
#pragma warning disable CS0618 // Type or member is obsolete
            BindableProperty.Create<WrapLayout, double>(w => w.Spacing, 5,
                propertyChanged: (bindable, oldvalue, newvalue) => ((WrapLayout)bindable)._LayoutCache.Clear());
#pragma warning restore CS0618 // Type or member is obsolete
        #endregion

        #region Public Properties
        /// <summary>
        /// Spacing added between elements (both directions)
        /// </summary>
        /// <value>The spacing.</value>
        public double Spacing
        {
            get => (double)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }
        #endregion

        #region Constructor
        public WrapLayout()
        {
            BackgroundColor = Color.Red;
            VerticalOptions = HorizontalOptions = LayoutOptions.FillAndExpand;
        }
        #endregion

        #region Private Methods
        private NaiveLayoutResult NaiveLayout(double width)
        {
            double startX = 0;
            double startY = 0;
            var right = width;
            double nextY = 0;

            //lastX = 0;
            //lastY = 0;

            var result = new NaiveLayoutResult();

            var currentList = new ViewAndRectableList();

            foreach (var child in Children)
            {
                if (!_LayoutCache.TryGetValue(child, out SizeRequest sizeRequest))
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    _LayoutCache[child] = sizeRequest = child.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity);
#pragma warning restore CS0618 // Type or member is obsolete
                }

                var paddedWidth = sizeRequest.Request.Width + Spacing;
                var paddedHeight = sizeRequest.Request.Height + Spacing;

                if (startX + paddedWidth > right)
                {
                    startX = 0;
                    startY += nextY;

                    if (currentList.Count > 0)
                    {
                        result.Add(currentList);
                        currentList = new ViewAndRectableList();
                    }
                }

                currentList.Add(new ViewAndRectangle(child, new Rectangle(startX, startY, sizeRequest.Request.Width, sizeRequest.Request.Height)));

                result.LastX = Math.Max(result.LastX, startX + paddedWidth);
                result.LastY = Math.Max(result.LastY, startY + paddedHeight);

                nextY = Math.Max(nextY, paddedHeight);
                startX += paddedWidth;
            }
            result.Add(currentList);
            return result;
        }
        #endregion

        #region Protected Overrides
        protected override void OnChildMeasureInvalidated()
        {
            base.OnChildMeasureInvalidated();
            _LayoutCache.Clear();
        }

        [Obsolete]
        protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
        {
            var result=NaiveLayout(widthConstraint);
            return new SizeRequest(new Size(result.LastX, result.LastY));
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            var layout = NaiveLayout(width);

            foreach (var t in layout)
            {
                var offset = (int)((width - t.Last().Rectangle.Right) / 2);
                foreach (var dingus in t)
                {
                    var location = new Rectangle(dingus.Rectangle.X + x + offset, dingus.Rectangle.Y + y, dingus.Rectangle.Width, dingus.Rectangle.Height);
                    LayoutChildIntoBoundingRegion(dingus.View, location);
                }
            }
        }
        #endregion

        #region Private Inner Classes
        private class ViewAndRectangle
        {
            #region Constructor
            public ViewAndRectangle(View view, Rectangle rectangle)
            {
                View = view;
                Rectangle = rectangle;
            }
            #endregion

            #region Public Properties
            public View View { get; }
            public Rectangle Rectangle { get; }
            #endregion
        }

        private class ViewAndRectableList : List<ViewAndRectangle>
        {

        }

        private class NaiveLayoutResult : List<ViewAndRectableList>
        {
            internal double LastX ;
            internal double LastY ;
        }
        #endregion
    }
}
