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
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using QuizGame.Models;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace QuizGame.Controls
{
    public sealed partial class CountdownControl : UserControl, IDisposable
    {
        public CountdownControl() => InitializeComponent();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                backgroundDefaultBrush.Dispose();
                backgroundAnswerCorrectBrush.Dispose();
                backgroundAnswerIncorrectBrush.Dispose();
                borderDefaultBrush.Dispose();
                borderGradientBrush.Dispose();
                borderAnswerCorrectBrush.Dispose();
                borderAnswerIncorrectBrush.Dispose();
                hairlineStrokeStyle.Dispose();
                textFormat.Dispose();
            }
        }

        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            AnimatedControl.RemoveFromVisualTree();
            AnimatedControl = null;
        }

        private void AnimatedControl_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            // Initialize the brushes.
            backgroundDefaultBrush = CreateRadialGradientBrush(sender,
                Color.FromArgb(0x00, 0x6E, 0xEF, 0xF8), Color.FromArgb(0x4D, 0x6E, 0xEF, 0xF8));
            backgroundAnswerCorrectBrush = CreateRadialGradientBrush(sender,
                Color.FromArgb(0x00, 0x42, 0xC9, 0xC5), Color.FromArgb(0x4D, 0x42, 0xC9, 0xC5));
            backgroundAnswerIncorrectBrush = CreateRadialGradientBrush(sender,
                Color.FromArgb(0x00, 0xDE, 0x01, 0x99), Color.FromArgb(0x4D, 0xDE, 0x01, 0x99));
            borderDefaultBrush = new CanvasSolidColorBrush(sender, Color.FromArgb(0xFF, 0x6E, 0xEF, 0xF8));
            borderGradientBrush = new CanvasLinearGradientBrush(sender,
                Color.FromArgb(0xFF, 0x0F, 0x56, 0xA4), Color.FromArgb(0xFF, 0x6E, 0xEF, 0xF8))
            {
                StartPoint = new Vector2(centerPoint.X - radius, centerPoint.Y),
                EndPoint = new Vector2(centerPoint.X + radius, centerPoint.Y)
            };
            borderAnswerCorrectBrush = new CanvasSolidColorBrush(sender, Color.FromArgb(0xFF, 0x42, 0xC9, 0xC5));
            borderAnswerIncorrectBrush = new CanvasSolidColorBrush(sender, Color.FromArgb(0xFF, 0xDE, 0x01, 0x99));

            // Calculate the text position for vertical centering to account for the
            // fact that text is not vertically centered within its layout bounds. 
            var textLayout = new CanvasTextLayout(sender, "0123456789", textFormat, 0, 0);
            var drawMidpoint = (float)(textLayout.DrawBounds.Top + (textLayout.DrawBounds.Height / 2));
            var layoutMidpoint = (float)(textLayout.LayoutBounds.Top + (textLayout.LayoutBounds.Height / 2));
            var textPositionDelta = drawMidpoint - layoutMidpoint;
            textPosition = new Vector2(centerPoint.X, centerPoint.Y - textPositionDelta);
        }

        private static ICanvasBrush CreateRadialGradientBrush(ICanvasResourceCreator creator, Color startColor, Color endColor)
        {
            var gradientStops = new CanvasGradientStop[]
            {
                new CanvasGradientStop() { Color = startColor, Position = 0.5f },
                new CanvasGradientStop() { Color = endColor, Position = 1.0f }
            };
            return new CanvasRadialGradientBrush(creator, gradientStops,
                CanvasEdgeBehavior.Clamp, CanvasAlphaMode.Straight)
            {
                Center = centerPoint,
                RadiusX = radius,
                RadiusY = radius
            };
        }

        private void AnimatedControl_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            var session = args.DrawingSession;

            // Scale to fit the current window size. 
            var scale = (float)Math.Min(sender.Size.Width, sender.Size.Height) / size;
            session.Transform = Matrix3x2.CreateScale(scale, scale);

            // Calculate key values affecting this draw pass. 
            var numberString = _countdownSeconds.ToString();// Math.Ceiling(secondsRemaining).ToString();

            var sweepAngle = fullCircle - (fullCircle * (_countdownSeconds) / 26);
            ICanvasBrush backgroundBrush, borderBrush;
            switch (_answerState)
            {
                case AnswerState.AnsweredCorrectly:
                    backgroundBrush = backgroundAnswerCorrectBrush;
                    borderBrush = borderAnswerCorrectBrush;
                    break;
                case AnswerState.AnsweredIncorrectly:
                    backgroundBrush = backgroundAnswerIncorrectBrush;
                    borderBrush = borderAnswerIncorrectBrush;
                    break;
                case AnswerState.Unanswered:
                default:
                    backgroundBrush = backgroundDefaultBrush;
                    borderBrush = _text == null ? borderGradientBrush : borderDefaultBrush;
                    break;
            }

            // Draw the background.
            using (var builder = new CanvasPathBuilder(session))
            {
                builder.BeginFigure(centerPoint);
                builder.AddArc(centerPoint, radius, radius, startAngle, sweepAngle);
                builder.EndFigure(CanvasFigureLoop.Closed);
                using (var geometry = CanvasGeometry.CreatePath(builder))
                {
                    session.FillGeometry(geometry, backgroundBrush);
                }
            }

            // Draw the border.
            using (var builder = new CanvasPathBuilder(session))
            {
                builder.BeginFigure(centerPoint.X, centerPoint.Y - radius);
                builder.AddArc(centerPoint, radius, radius, startAngle, sweepAngle);
                builder.EndFigure(CanvasFigureLoop.Open);
                using (var geometry = CanvasGeometry.CreatePath(builder))
                {
                    session.DrawGeometry(geometry, borderBrush, 10);
                }
            }

            // Draw the foreground.
            session.DrawText(_text ?? _countdownSeconds.ToString(), textPosition, Colors.White, textFormat);
        }

        public int CountdownSeconds
        {
            get => (int)GetValue(CountdownSecondsProperty); 
            set => SetValue(CountdownSecondsProperty, value); 
        }

        private int _countdownSeconds = 30;
        public static readonly DependencyProperty CountdownSecondsProperty =
            DependencyProperty.Register("CountdownSeconds", typeof(int),
                typeof(CountdownControl), new PropertyMetadata(30,
                    (s, e) => (s as CountdownControl)._countdownSeconds = (int)e.NewValue));

        public String Text
        {
            get => (String)GetValue(TextProperty); 
            set => SetValue(TextProperty, value); 
        }

        private String _text;
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(String),
                typeof(CountdownControl), new PropertyMetadata(null,
                    (s, e) => (s as CountdownControl)._text = (String)e.NewValue));

        public AnswerState AnswerState
        {
            get => (AnswerState)GetValue(AnswerStateProperty); 
            set => SetValue(AnswerStateProperty, value); 
        }

        private AnswerState _answerState = AnswerState.Unanswered;
        public static readonly DependencyProperty AnswerStateProperty =
            DependencyProperty.Register("AnswerState", typeof(AnswerState),
                typeof(CountdownControl), new PropertyMetadata(AnswerState.Unanswered,
                    (s, e) => (s as CountdownControl)._answerState = (AnswerState)e.NewValue));

        private ICanvasBrush backgroundDefaultBrush;
        private ICanvasBrush backgroundAnswerCorrectBrush;
        private ICanvasBrush backgroundAnswerIncorrectBrush;
        private ICanvasBrush borderDefaultBrush;
        private CanvasLinearGradientBrush borderGradientBrush;
        private ICanvasBrush borderAnswerCorrectBrush;
        private ICanvasBrush borderAnswerIncorrectBrush;
        private Color borderColor = Colors.White;
        private const float size = 1000;
        private const float padding = 8;
        private const float radius = size / 2 - padding;
        private const float fullCircle = (float)Math.PI * 2;
        private const float startAngle = fullCircle * 0.75f;
        private const float updatesPerSecond = 60f;
        private static readonly Vector2 centerPoint = new Vector2(size / 2);
        private Vector2 textPosition;
        private static readonly CanvasStrokeStyle hairlineStrokeStyle = new CanvasStrokeStyle()
        {
            TransformBehavior = CanvasStrokeTransformBehavior.Hairline
        };
        static readonly CanvasTextFormat textFormat = new CanvasTextFormat()
        {
            FontFamily = "XamlAutoFontFamily",
            FontSize = size / 2,
            HorizontalAlignment = CanvasHorizontalAlignment.Center,
            VerticalAlignment = CanvasVerticalAlignment.Center
        };

    }
}
