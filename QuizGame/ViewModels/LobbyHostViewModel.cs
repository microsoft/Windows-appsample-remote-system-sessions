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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Toolkit.Uwp;
using QuizGame.Models;
using QuizGame.Views;

namespace QuizGame.ViewModels
{
    public class LobbyHostViewModel : LobbyViewModel
    {
        private readonly Timer _timer;

        public LobbyHostViewModel()
        {
            LobbyText = "Waiting for players to join the game...";
            App.SessionManager.SendDebugMessage($"Starting broadcast of status.");
            _timer = new Timer(SendScoreboard, null, 0, 1000);
            Scorecards.CollectionChanged += Scorecards_CollectionChanged;
        }

        public override void Cleanup()
        {
            Scorecards.CollectionChanged -= Scorecards_CollectionChanged;
            _timer.Dispose();
            base.Cleanup();
        }

        public override bool CanStartGame => Scorecards.Any();

        public override async void StartGame()
        {
            var questions = await LoadQuestionsAsync();

            App.SessionManager.SendDebugMessage($"Starting game.");
            await App.SessionManager.SendMessageToParticipantsAsync(new HostMessage
            {
                Type = HostMessageType.GameStarted,
                Question = questions[0],
                PlayerScores = Scorecards, 
                TimeSent = DateTime.Now
            });

            Cleanup();

            Navigate(typeof(GameView), new GameHostViewModel()
            {
                Questions = new ObservableCollection<Question>(questions),
                Scorecards = Scorecards,
                CurrentQuestion = questions[0]
            });
        }

        public override void LeaveGame()
        {
            App.SessionManager.EndSession();
            Cleanup();
        }

        protected override void MessageReceivedAsync(object sender, MessageReceivedEventArgs e)
        {
            object data = new PlayerMessage();
            e.GetDeserializedMessage(ref data);
            var message = data as PlayerMessage;
            if (message.Type == PlayerMessageType.Join)
            {
                OnPlayerJoined(message);
                App.SessionManager.SendDebugMessage($"{message.PlayerName} joined.");
            }
            else if (message.Type == PlayerMessageType.Leave)
            {
                App.SessionManager.SendDebugMessage($"{message.PlayerName} left.");
                OnPlayerLeft(message);
            }
        }

        private void OnPlayerJoined(PlayerMessage message)
        {
            var score = new Scorecard()
            {
                PlayerId = Guid.NewGuid(),
                PlayerName = message.PlayerName,
                AnswerStatus = AnswerState.Unanswered,
                TotalScore = 0,
                TotalCorrectAnswers = 0,
                TotalTime = TimeSpan.Zero
            };
            Scorecards.Add(score);
        }

        private void OnPlayerLeft(PlayerMessage message) => 
            Scorecards.Remove(Scorecards.FirstOrDefault(s => s.PlayerName.Equals(message.PlayerName)));

        private async void SendScoreboard(Object state)
        {
            var message = new HostMessage
            {
                Type = HostMessageType.Scoreboard,
                PlayerScores = Scorecards,
                TimeSent = DateTime.Now
            };

            await DispatcherHelper.ExecuteOnUIThreadAsync(async () => 
                await App.SessionManager.SendMessageToParticipantsAsync(message));
        }
        private async Task<List<Question>> LoadQuestionsAsync()
        {
            using (var stream = await StreamHelper.GetPackagedFileStreamAsync("Questions.xml"))
            {
                var xml = XElement.Load(stream.AsStream());
                return xml.Descendants("Question").Select(questionElement => new Question
                {
                    Text = questionElement.Attribute("Text").Value,
                    Options = questionElement.Descendants("Option").Select(optionElement => new Option
                    {
                        Text = optionElement.Attribute("Text").Value,
                        IsCorrectAnswer = optionElement.Attribute("IsCorrectAnswer") == null ? false : true
                    }).ToList()
                }).ToList();
            }
        }

        private void Scorecards_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => 
            OnPropertyChanged(nameof(CanStartGame));
    }
}
