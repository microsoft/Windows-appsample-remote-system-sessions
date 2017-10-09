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
using QuizGame.Models;
using QuizGame.Views;
using Windows.System.RemoteSystems;

namespace QuizGame.ViewModels
{
    public class LobbyPlayerViewModel : LobbyViewModel
    {
        public LobbyPlayerViewModel()
        {
            LobbyText = "Waiting for the host to start the game...";
            App.SessionManager.SessionDisconnected += SessionManager_SessionDisconnected;
        }

        private void SessionManager_SessionDisconnected(object sender, RemoteSystemSessionDisconnectedEventArgs e)
        {
            AppShell.Instance.ContentFrame.BackStack.Clear();
            Navigate(typeof(WelcomeView));
        }

        private string _playerName;
        public string PlayerName
        {
            get => _playerName;
            set => SetProperty(ref _playerName, value);
        }

        public override bool CanStartGame => false;

        public override void StartGame() => throw new NotSupportedException("Players cannot start a game");

        public override async void LeaveGame()
        {
            await App.SessionManager.SendMessageToHostAsync(new PlayerMessage
            {
                Type = PlayerMessageType.Leave
            });
        }

        protected override void MessageReceivedAsync(object sender, MessageReceivedEventArgs e)
        {
            object data = new HostMessage();
            e.GetDeserializedMessage(ref data);
            var message = data as HostMessage;
            if (message.Type == HostMessageType.GameStarted)
            {
                App.SessionManager.SendDebugMessage($"Host started the game.");
                Navigate(typeof(GameView), new GamePlayerViewModel()
                {
                    CurrentQuestion = message.Question,
                    Scorecards = message.PlayerScores,
                    PlayerName = PlayerName,
                    QuestionReceived = DateTime.Now
                });
            }
            else if (message.Type == HostMessageType.Scoreboard)
            {
                Scorecards = message.PlayerScores;
            }
        }

        public override void Cleanup()
        {
            App.SessionManager.SessionDisconnected -= SessionManager_SessionDisconnected;
            base.Cleanup();
        }
    }
}
