//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

using System;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace QuizGame.Controls
{
    public sealed partial class CircleIconControl : UserControl
    {
        public CircleIconControl() => InitializeComponent();

        public string Text
        {
            get => (string)GetValue(TextProperty); 
            set => SetValue(TextProperty, value); 
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CircleIconControl), new PropertyMetadata(null));

        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var width = (float)sender.ActualWidth;
            var height = (float)sender.ActualHeight;
            var center = new Vector2(width / 2, height / 2);
            var strokeWidth = 2f;
            var radius = Math.Min(width, height) / 2 - strokeWidth;
            var startColor = Color.FromArgb(0x00, 0x6E, 0xEF, 0xF8);
            var endColor = Color.FromArgb(0x4D, 0x6E, 0xEF, 0xF8);
            var borderColor = Color.FromArgb(0xFF, 0x6E, 0xEF, 0xF8);
            var gradientStops = new CanvasGradientStop[]
            {
                new CanvasGradientStop() { Color = startColor, Position = 0.5f },
                new CanvasGradientStop() { Color = endColor, Position = 1 }
            };
            var brush = new CanvasRadialGradientBrush(canvas, gradientStops, CanvasEdgeBehavior.Clamp, CanvasAlphaMode.Straight)
            {
                Center = center,
                RadiusX = radius,
                RadiusY = radius
            };
            args.DrawingSession.FillCircle(center, radius, brush);
            args.DrawingSession.DrawCircle(center, radius, borderColor, strokeWidth);
        }
    }
}
