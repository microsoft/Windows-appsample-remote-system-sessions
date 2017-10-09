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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using QuizGame.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace QuizGame.Controls
{
    public sealed partial class ScoreSidebarControl : UserControl, INotifyPropertyChanged
    {
        public ScoreSidebarControl() => InitializeComponent();

        private ObservableCollection<Scorecard> _scorecards = new ObservableCollection<Scorecard>();
        public ObservableCollection<Scorecard> Scorecards
        {
            get => _scorecards;
            set
            {
                SetProperty(ref _scorecards, value);
                OnPropertyChanged(nameof(AnsweredScorecards));
                OnPropertyChanged(nameof(UnansweredScorecards));
                OnPropertyChanged(nameof(AreAnyScorecardsUnanswered));
            }
        }

        private ObservableCollection<Scorecard> AnsweredScorecards => 
            new ObservableCollection<Scorecard>(Scorecards
                .Where(x => x.AnswerStatus != AnswerState.Unanswered)
                .OrderByDescending(x => x.TotalScore)); 

        private ObservableCollection<Scorecard> UnansweredScorecards =>
            new ObservableCollection<Scorecard>(Scorecards
                .Where(x => x.AnswerStatus == AnswerState.Unanswered)
                .OrderByDescending(x => x.TotalScore)); 

        public bool AreAnyScorecardsUnanswered => UnansweredScorecards.Any(); 

        private int _timerTick; 
        public int TimerTick
        {
            get => _timerTick;
            set => SetProperty(ref _timerTick, value); 
        }

        public string PlayerName
        {
            get => (string)GetValue(PlayerNameProperty); 
            set => SetValue(PlayerNameProperty, value); 
        }

        public static readonly DependencyProperty PlayerNameProperty =
            DependencyProperty.Register("PlayerName", typeof(string), 
                typeof(ScoreSidebarControl), new PropertyMetadata(null, 
                    (s, e) => (s as ScoreSidebarControl).OnPropertyChanged(null)));

        public AnswerState PlayerAnswerState => Scorecards.FirstOrDefault(
            card => card.PlayerName == PlayerName)?.AnswerStatus ?? AnswerState.Unanswered;

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (Object.Equals(field, value))
            {
                return false;
            }
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
