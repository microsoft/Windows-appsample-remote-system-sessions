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
using System.ComponentModel;
using System.Diagnostics;
using QuizGame.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace QuizGame.Controls
{
    public class OptionSelectedEventArgs
    {
        public Option SelectedOption { get; set; }
    }

    public sealed partial class QuestionControl : UserControl, INotifyPropertyChanged
    {
        public QuestionControl() => InitializeComponent();

        public Question Question
        {
            get => (Question)GetValue(QuestionProperty); 
            set => SetValue(QuestionProperty, value); 
        }

        public static readonly DependencyProperty QuestionProperty =
            DependencyProperty.Register(
                nameof(Question), 
                typeof(Question), 
                typeof(QuestionControl), 
                new PropertyMetadata(null, (s, e) =>
                {
                    Debug.WriteLine("Setting q prop"); 
                }));

        public Option SelectedOption
        {
            get => (Option)GetValue(SelectedOptionProperty);
            set => SetValue(SelectedOptionProperty, value); 
        }

        public static readonly DependencyProperty SelectedOptionProperty =
            DependencyProperty.Register(
                nameof(SelectedOption),
                typeof(Option),
                typeof(QuestionControl),
                new PropertyMetadata(null));

        public bool ShowOptions
        {
            get => (bool)GetValue(ShowOptionsProperty);
            set
            {
                SetValue(ShowOptionsProperty, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OptionsItemTemplate))); 
            }
        }

        public static readonly DependencyProperty ShowOptionsProperty =
            DependencyProperty.Register(
                nameof(ShowOptions),
                typeof(bool),
                typeof(QuestionControl),
                new PropertyMetadata(null));

        public bool IsOptionsEnabled
        {
            get => (bool)GetValue(IsOptionsEnabledProperty);
            set
            {
                SetValue(IsOptionsEnabledProperty, value);
                Options.SelectionMode = value ? ListViewSelectionMode.Single : ListViewSelectionMode.None; 
            }
        }

        public static readonly DependencyProperty IsOptionsEnabledProperty =
            DependencyProperty.Register(
                nameof(IsOptionsEnabled),
                typeof(bool),
                typeof(QuestionControl),
                new PropertyMetadata(null));

        private DataTemplate QuestionOptionsDefaultItemTemplate =>
            Resources[nameof(QuestionOptionsDefaultItemTemplate)] as DataTemplate;

        private DataTemplate QuestionOptionsAnswersRevealedItemTemplate =>
            Resources[nameof(QuestionOptionsAnswersRevealedItemTemplate)] as DataTemplate;

        public DataTemplate OptionsItemTemplate => ShowOptions ? 
            QuestionOptionsAnswersRevealedItemTemplate : QuestionOptionsDefaultItemTemplate;

        public event EventHandler<OptionSelectedEventArgs> OptionSelected;
        public event PropertyChangedEventHandler PropertyChanged;

        private void Options_ItemClick(object sender, ItemClickEventArgs e)
        {
            var option = e.ClickedItem as Option;
            ShowOptions = true;
            option.IsLocalPlayerSelection = true;
            OptionSelected?.Invoke(this, new OptionSelectedEventArgs { SelectedOption = option }); 
        }
    }
}
