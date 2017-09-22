using System;
using System.Collections.Generic;
using System.Linq;
using Adapt.Extensions;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    /// <summary>
    /// A panel controls that renders its contents from left to right and wraps vertically. Note this control currently uses HorizontalOptions to control the horizontal alignment of the content.
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
        public static readonly BindableProperty SpacingProperty = BindableProperty.Create(nameof(Spacing), defaultValue: 5d, returnType: typeof(double), declaringType: typeof(WrapLayout), propertyChanged: (bindable, oldvalue, newvalue) => ((WrapLayout)bindable)._LayoutCache.Clear());
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
            VerticalOptions = HorizontalOptions = LayoutOptions.FillAndExpand;
        }
        #endregion

        #region Private Methods
        private NaiveLayoutResult NaiveLayout(double fullWidthConstraint)
        {
            if (HasParent)
            {
                //We shouldn't have to do this if the width would set correctly
                var parentLayout = (WrapLayout)Parent;
                FullWidthConstraint = parentLayout.FullWidthConstraint - (X - parentLayout.X);
            }
            else
            {
                FullWidthConstraint = fullWidthConstraint;
            }

            double currentX = 0, nextY = 0, currentY = 0;
            var result = new NaiveLayoutResult();
            var currentList = new ViewAndRectangleList();

            foreach (var child in Children)
            {
                if (!_LayoutCache.TryGetValue(child, out SizeRequest sizeRequest))
                {
                    if (child is WrapLayout wraplayout)
                    {
                        //It's a direct nested wrap layout, make sure it knows it where to start the next line
                        if (!wraplayout.HasParent)
                        {
                            wraplayout.HasParent = true;
                            wraplayout.ParentWrapStart = X;
                        }
                    }

                    var cache = false;
                    var availableSpace = FullWidthConstraint - currentX;
                    if (child is Layout layout)
                    {
                        //Constrain this so it knows to resize if it can
                        if (!double.IsNaN(availableSpace) && availableSpace > 0 && !layout.WidthRequest.Equals(-1))
                        {
                            layout.WidthRequest = availableSpace;
                        }
                        layout.ForceLayout();
                    }
                    else
                    {
                        cache = true;
                    }

                    sizeRequest = child.Measure(double.PositiveInfinity, double.PositiveInfinity);

                    if (cache) _LayoutCache[child] = sizeRequest;
                }

                var paddedWidth = sizeRequest.Request.Width + Spacing;
                var paddedHeight = sizeRequest.Request.Height + Spacing;

                if (currentX + paddedWidth > FullWidthConstraint)
                {
                    //We're over sized, wrap to the next line and set the X to 0
                    currentX = HasParent ? ParentWrapStart : 0;
                    currentY += nextY;

                    if (currentList.Any())
                    {
                        result.Add(currentList);
                        currentList = new ViewAndRectangleList();
                    }
                }

                currentList.Add(new ViewAndRectangle(child, new Rectangle(currentX, currentY, sizeRequest.Request.Width, sizeRequest.Request.Height)));

                result.LastX = Math.Max(result.LastX, currentX + paddedWidth);
                result.LastY = Math.Max(result.LastY, currentY + paddedHeight);

                nextY = Math.Max(nextY, paddedHeight);
                currentX += paddedWidth;
            }

            result.Add(currentList);

            return result;
        }

        public double FullWidthConstraint { get; set; }

        public double ParentWrapStart { get; set; }

        public bool HasParent { get; set; }
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
            var result = NaiveLayout(widthConstraint);
            return new SizeRequest(new Size(result.LastX, result.LastY));
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            var naiveLayoutResult = NaiveLayout(width);

            foreach (var viewAndRectangleList in naiveLayoutResult)
            {
                int offset;

                if (HorizontalOptions.Equals(LayoutOptions.Center) || HorizontalOptions.Equals(LayoutOptions.CenterAndExpand))
                {
                    //Put contents in the middle
                    offset = (int)((width - viewAndRectangleList.Last().Rectangle.Right) / 2);
                }
                else if (HorizontalOptions.Equals(LayoutOptions.Fill) || HorizontalOptions.Equals(LayoutOptions.FillAndExpand) || HorizontalOptions.Equals(LayoutOptions.Start) || HorizontalOptions.Equals(LayoutOptions.StartAndExpand))
                {
                    //Put contents on the left
                    offset = 0;
                }
                else
                {
                    //Put contents on the right
                    offset = 0;
                    //TODO: Right alignment
                }

                foreach (var viewAndRectangle in viewAndRectangleList)
                {
                    var location = new Rectangle(viewAndRectangle.Rectangle.X + x + offset, viewAndRectangle.Rectangle.Y + y, viewAndRectangle.Rectangle.Width, viewAndRectangle.Rectangle.Height);
                    LayoutChildIntoBoundingRegion(viewAndRectangle.View, location);
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

        private class ViewAndRectangleList : List<ViewAndRectangle>
        {

        }

        /// <summary>
        /// A class for holding the information about how the child views should be lain out
        /// </summary>
        private class NaiveLayoutResult : List<ViewAndRectangleList>
        {
            internal double LastX;
            internal double LastY;
        }
        #endregion
    }
}
