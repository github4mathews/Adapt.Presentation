﻿using System;
using System.Linq;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    /// <summary>
    /// New WrapLayout
    /// </summary>
    /// <author>Jason Smith</author>
    public class WrapLayout : Layout<View>
    {
        private readonly Dictionary<View, SizeRequest> _LayoutCache = new Dictionary<View, SizeRequest>();

        /// <summary>
        /// Backing Storage for the Spacing property
        /// </summary>
        public static readonly BindableProperty SpacingProperty =
#pragma warning disable CS0618 // Type or member is obsolete
            BindableProperty.Create<WrapLayout, double>(w => w.Spacing, 5,
                propertyChanged: (bindable, oldvalue, newvalue) => ((WrapLayout)bindable)._LayoutCache.Clear());
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Spacing added between elements (both directions)
        /// </summary>
        /// <value>The spacing.</value>
        public double Spacing
        {
            get => (double)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        public WrapLayout()
        {
            VerticalOptions = HorizontalOptions = LayoutOptions.FillAndExpand;
        }

        protected override void OnChildMeasureInvalidated()
        {
            base.OnChildMeasureInvalidated();
            _LayoutCache.Clear();
        }

        [Obsolete]
        protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
        {

            NaiveLayout(widthConstraint, heightConstraint, out double lastX, out double lastY);

            return new SizeRequest(new Size(lastX, lastY));
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            var layout = NaiveLayout(width, height, out double lastX, out double lastY);

            foreach (var t in layout)
            {
                var offset = (int)((width - t.Last().Item2.Right) / 2);
                foreach (var dingus in t)
                {
                    var location = new Rectangle(dingus.Item2.X + x + offset, dingus.Item2.Y + y, dingus.Item2.Width, dingus.Item2.Height);
                    LayoutChildIntoBoundingRegion(dingus.Item1, location);
                }
            }
        }

        private List<List<Tuple<View, Rectangle>>> NaiveLayout(double width, double height, out double lastX, out double lastY)
        {
            double startX = 0;
            double startY = 0;
            var right = width;
            double nextY = 0;

            lastX = 0;
            lastY = 0;

            var result = new List<List<Tuple<View, Rectangle>>>();
            var currentList = new List<Tuple<View, Rectangle>>();

            foreach (var child in Children)
            {
                if (!_LayoutCache.TryGetValue(child, out SizeRequest sizeRequest))
                {
                    _LayoutCache[child] = sizeRequest = child.Measure(double.PositiveInfinity, double.PositiveInfinity);
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
                        currentList = new List<Tuple<View, Rectangle>>();
                    }
                }

                currentList.Add(new Tuple<View, Rectangle>(child, new Rectangle(startX, startY, sizeRequest.Request.Width, sizeRequest.Request.Height)));

                lastX = Math.Max(lastX, startX + paddedWidth);
                lastY = Math.Max(lastY, startY + paddedHeight);

                nextY = Math.Max(nextY, paddedHeight);
                startX += paddedWidth;
            }
            result.Add(currentList);
            return result;
        }
    }
}
