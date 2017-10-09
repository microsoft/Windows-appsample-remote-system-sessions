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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using QuizGame.Models;
using QuizGame.Views;

namespace QuizGame.ViewModels
{
    public class GamePlayerViewModel : GameViewModel
    {

        private DateTime _questionsReceived;
        public DateTime QuestionReceived
        {
            get => _questionsReceived;
            set => SetProperty(ref _questionsReceived, value);
        }

        private bool _isOptionsEnabled = true;
        public override bool IsOptionsEnabled
        {
            get => _isOptionsEnabled;
            set => SetProperty(ref _isOptionsEnabled, value);
        }

        protected async override void MessageReceived(object s, MessageReceivedEventArgs e)
        {
            object data = new HostMessage();
            e.GetDeserializedMessage(ref data);
            var message = data as HostMessage;
            if (message.Type == HostMessageType.Question)
            {
                App.SessionManager.SendDebugMessage($"Received question from host.");
                QuestionReceived = DateTime.Now;
                CurrentQuestion = message.Question;
                await NextQuestionAsync();
            }
            else if (message.Type == HostMessageType.Scoreboard)
            {
                // Only update the scoreboard if it's changed since the last refresh
                var current = new HashSet<Scorecard>(Scorecards, new ScorecardComparer());
                if (!current.SetEquals(message.PlayerScores))
                {
                    Scorecards = message.PlayerScores;
                    OnScoreboardChanged(new ScoreboardChangedEventArgs { NewScores = message.PlayerScores });
                }
            }
            else if (message.Type == HostMessageType.GameOver)
            {
                App.SessionManager.SendDebugMessage($"Host ended game.");
                Navigate(typeof(EndGameView), new EndGameViewModel
                {
                    Scorecards = new ObservableCollection<Scorecard>(
                        Scorecards.OrderByDescending(scorecard => scorecard.TotalScore)),
                    PlayerName = PlayerName
                });
            }
        }

        public async override Task NextQuestionAsync()
        {
            OnQuestionChanged(new QuestionChangedEventArgs { NewQuestion = CurrentQuestion });
            IsOptionsEnabled = true;
            await base.NextQuestionAsync();
        }

        public async override Task OptionSelected(Option option)
        {
            ShowAnswers = true;
            IsOptionsEnabled = false;
            var message = new PlayerMessage()
            {
                Type = PlayerMessageType.Answer,
                IsCorrect = option.IsCorrectAnswer,
                AnswerTime = DateTime.Now.Subtract(QuestionReceived),
                PlayerName = PlayerName,
                Question = CurrentQuestion
            };
            App.SessionManager.SendDebugMessage($"Sent selected option to host.");
            await App.SessionManager.SendMessageToHostAsync(message);
        }
    }
}
