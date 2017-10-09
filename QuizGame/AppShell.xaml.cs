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

using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace QuizGame
{
    public sealed partial class AppShell : Page, INotifyPropertyChanged
    {
        public static AppShell Instance { get; set; } 

        public Frame ContentFrame => frame;

        public ObservableCollection<Log> Logs => App.LogManager.Messages; 

        private SystemNavigationManager NavigationManager { get; } = SystemNavigationManager.GetForCurrentView();

        private bool IsBackButtonVisible => NavigationManager.AppViewBackButtonVisibility == AppViewBackButtonVisibility.Visible;

        public Thickness BackButtonOffsetPadding => new Thickness((IsBackButtonVisible ? 60 : 12), 0, 0, 0);

        private ApplicationViewTitleBar TitleBar { get; } = ApplicationView.GetForCurrentView().TitleBar;

        private bool IsWindowActive { get; set; }

        public Brush TitleBarForegroundColor => new SolidColorBrush(IsWindowActive ?
            TitleBar.ForegroundColor.Value : TitleBar.InactiveForegroundColor.Value);

        public AppShell()
        {
            InitializeComponent();

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            TitleBar.ButtonBackgroundColor = Colors.Transparent;
            TitleBar.ButtonForegroundColor = Colors.White;
            TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF);
            TitleBar.ButtonHoverForegroundColor = Colors.White;
            TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            TitleBar.ButtonInactiveForegroundColor = Colors.Gray;
            TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF);
            TitleBar.ButtonPressedForegroundColor = Colors.White;
            TitleBar.BackgroundColor = Colors.Transparent;
            TitleBar.ForegroundColor = Colors.White;
            TitleBar.InactiveBackgroundColor = Colors.Transparent;
            TitleBar.InactiveForegroundColor = Colors.Gray;

            Window.Current.Activated += (s, e) =>
            {
                IsWindowActive = e.WindowActivationState != CoreWindowActivationState.Deactivated;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TitleBarForegroundColor)));
            };

            ContentFrame.Navigated += (s, e) =>
            {
                NavigationManager.AppViewBackButtonVisibility = ContentFrame.CanGoBack ?
                    AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackButtonOffsetPadding)));
            };

            NavigationManager.BackRequested += (s, e) =>
            {
                if (!e.Handled && ContentFrame.CanGoBack)
                {
                    ContentFrame.GoBack();
                    e.Handled = true;
                }
            };

            Instance = this; 
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
