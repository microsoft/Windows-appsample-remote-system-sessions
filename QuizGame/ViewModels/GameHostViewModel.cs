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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp;
using QuizGame.Models;
using QuizGame.Views;

namespace QuizGame.ViewModels
{
    public class GameHostViewModel : GameViewModel
    {
        private readonly Timer _timer;

        public GameHostViewModel()
        {
            _timer = new Timer(SendScoreboard, null, 0, 1000);
        }

        public ObservableCollection<Question> Questions { get; set; }

        public override bool IsOptionsEnabled
        {
            get => false;
            set => throw new NotSupportedException("Must always return false; hosts cannot answer questions");
        }

        public override Task OptionSelected(Option option) => throw new NotSupportedException("Hosts cannot answer questions");

        public override async Task NextQuestionAsync()
        {
            // If there is another question, send it to all players. 
            // Otherwise, indicate the game is over and send final scores. 
            var currentQuestionIndex = Questions.IndexOf(Questions.First(x =>
                x.Text == CurrentQuestion.Text));
            if (currentQuestionIndex < Questions.Count - 1)
            {
                CurrentQuestion = Questions[currentQuestionIndex + 1];
                OnQuestionChanged(new QuestionChangedEventArgs { NewQuestion = CurrentQuestion });
                foreach (var s in Scorecards)
                {
                    s.AnswerStatus = AnswerState.Unanswered;
                }
                Scorecards = new ObservableCollection<Scorecard>(Scorecards);

                App.SessionManager.SendDebugMessage($"Sending next question to players.");
                await App.SessionManager.SendMessageToParticipantsAsync(new HostMessage
                {
                    Type = HostMessageType.Question,
                    Question = CurrentQuestion,
                    TimeSent = DateTime.Now
                });

                await base.NextQuestionAsync();
            }
            else
            {
                await App.SessionManager.SendMessageToParticipantsAsync(new HostMessage
                {
                    Type = HostMessageType.GameOver,
                    PlayerScores = Scorecards,
                    TimeSent = DateTime.Now,
                });
                var orderedScorecards = Scorecards.OrderByDescending(scorecard => scorecard.TotalScore);
                var leader = orderedScorecards.FirstOrDefault();
                App.SessionManager.SendDebugMessage($"Game ended.");
                Navigate(typeof(EndGameView), new EndGameViewModel
                {
                    Scorecards = new ObservableCollection<Scorecard>(orderedScorecards),
                    PlayerName = leader.PlayerName
                });
            }

        }

        protected override void MessageReceived(object s, MessageReceivedEventArgs e)
        {
            // Deserialize the message
            object data = new PlayerMessage();
            e.GetDeserializedMessage(ref data);

            var message = data as PlayerMessage;

            if (message.Type == PlayerMessageType.Answer)
            {
                App.SessionManager.SendDebugMessage($"Answer received from player.");
                var score = (int)(10 * (1 - (float)message.AnswerTime.Seconds / 25f));
                if (score < 0)
                {
                    score = 0;
                }

                var item = Scorecards.FirstOrDefault(x => x.PlayerName == message.PlayerName);
                item.TotalTime = item.TotalTime.Add(message.AnswerTime);
                item.TotalScore += message.IsCorrect ? score + 1 : 0;
                item.TotalCorrectAnswers += message.IsCorrect ? 1 : 0;
                item.AnswerStatus = message.IsCorrect ? AnswerState.AnsweredCorrectly : AnswerState.AnsweredIncorrectly;
                OnScoreboardChanged(new ScoreboardChangedEventArgs { NewScores = Scorecards });
            }

            // If the host isn't waiting on any more players to respond, reveal the answer

            if (!Scorecards.Any(x => x.AnswerStatus == AnswerState.Unanswered))
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => ShowAnswers = true);
            }
        }

        private async void SendScoreboard(Object state)
        {
            var leader = Scorecards.OrderByDescending(x => x.TotalScore).FirstOrDefault();
            foreach (var s in Scorecards)
            {
                s.IsLeader = false;
                if (s.PlayerName == leader.PlayerName)
                {
                    s.IsLeader = true;
                }
            }

            var message = new HostMessage
            {
                Type = HostMessageType.Scoreboard,
                PlayerScores = Scorecards,
                TimeSent = DateTime.Now
            };

            await App.SessionManager.SendMessageToParticipantsAsync(message);
        }

        public override void Cleanup()
        {
            _timer.Dispose();
            App.SessionManager.MessageReceived -= MessageReceived;
            base.Cleanup();
        }
    }
}
