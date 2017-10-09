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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp;
using QuizGame.Models;

namespace QuizGame.ViewModels
{
    public class QuestionChangedEventArgs
    {
        public Question NewQuestion { get; set; }
    }

    public class ScoreboardChangedEventArgs
    {
        public ObservableCollection<Scorecard> NewScores { get; set; }
    }

    public abstract class GameViewModel : ViewModelBase
    {
        private readonly Timer _countdownTimer; 

        public GameViewModel()
        {
            App.SessionManager.MessageReceived += MessageReceived;
            DispatcherHelper.ExecuteOnUIThreadAsync(() => CountdownTimerTick = 26);
            _countdownTimer = new Timer(OnCountdownTimerTick, null, 1, 1000); 
        }

        private string _playerName;
        public string PlayerName
        {
            get => _playerName;
            set => SetProperty(ref _playerName, value);
        }

        private ObservableCollection<Scorecard> _scorecards = new ObservableCollection<Scorecard>();
        public ObservableCollection<Scorecard> Scorecards
        {
            get => _scorecards;
            set => SetProperty(ref _scorecards, value);
        }

        private Question _currentQuestion;
        public Question CurrentQuestion
        {
            get => _currentQuestion;
            set => SetProperty(ref _currentQuestion, value);
        }

        private bool _showAnswers;
        public bool ShowAnswers
        {
            get => _showAnswers;
            set => SetProperty(ref _showAnswers, value);
        }

        private int _countdownTimerTick;
        public int CountdownTimerTick
        {
            get => _countdownTimerTick;
            set => SetProperty(ref _countdownTimerTick, value); 
        }

        public bool IsHostView => GetType() == typeof(GameHostViewModel);

        public abstract bool IsOptionsEnabled { get; set; }

        public abstract Task OptionSelected(Option option);
        protected abstract void MessageReceived(object s, MessageReceivedEventArgs e); 

        public async virtual Task NextQuestionAsync()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() => CountdownTimerTick = 26); 
        }

        protected void OnQuestionChanged(QuestionChangedEventArgs args)
        {
            QuestionChanged?.Invoke(this, args); 
        }

        public event EventHandler<QuestionChangedEventArgs> QuestionChanged;

        protected void OnScoreboardChanged(ScoreboardChangedEventArgs args)
        {
            ScoreboardChanged?.Invoke(this, args);
        }

        public override void Cleanup()
        {
            _countdownTimer.Dispose();
            App.SessionManager.MessageReceived -= MessageReceived;
        }

        private void OnCountdownTimerTick(object state)
        {
            if (CountdownTimerTick > 0)
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => CountdownTimerTick--);

            }
        }

        public event EventHandler<ScoreboardChangedEventArgs> ScoreboardChanged;
    }
}