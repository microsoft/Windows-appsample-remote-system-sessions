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
using Microsoft.Toolkit.Uwp;
using QuizGame.Models;
using QuizGame.Views;

namespace QuizGame.ViewModels
{
    public class JoinGameViewModel : ViewModelBase
    {
        public JoinGameViewModel()
        {
            IsLoading = true; 
            Games = new ObservableCollection<Game>();
            App.SessionManager.SessionFound += OnSessionFound;
            App.SessionManager.SessionRemoved += OnSessionRemoved;
            App.SessionManager.DiscoverSessions();
            Games.CollectionChanged += (s, e) => IsLoading = Games.Count == 0;
        }

        private string _playerName; 
        public string PlayerName
        {
            get => _playerName;
            set => SetProperty(ref _playerName, value); 
        }

        private ObservableCollection<Game> _games; 
        public ObservableCollection<Game> Games
        {
            get => _games;
            set => SetProperty(ref _games, value); 
        }

        private Game _selectedGame;
        public Game SelectedGame
        {
            get => _selectedGame;
            set { if (SetProperty(ref _selectedGame, value)) OnPropertyChanged(nameof(IsGameSelected)); } 
        }

        public bool IsGameSelected => SelectedGame != null;

        private bool _isLoading; 
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value); 
        }

        public async void JoinGame()
        {
            if (SelectedGame != null && !String.IsNullOrWhiteSpace(PlayerName))
            {
                
                await App.SessionManager.JoinSession(SelectedGame.Info, PlayerName);
                App.SessionManager.StartReceivingMessages();
                PlayerMessage command = new PlayerMessage()
                {
                    Type = PlayerMessageType.Join,
                    PlayerName = PlayerName
                };
                App.SessionManager.SendDebugMessage($"Joining {SelectedGame.Name}");
                await App.SessionManager.SendMessageToHostAsync(command);
                Cleanup();
                Navigate(typeof(LobbyView), new LobbyPlayerViewModel { GameName = SelectedGame.Name, PlayerName = PlayerName }); 
            }
        }

        public void LeaveGameView()
        {
            Cleanup();
        }

        private void OnSessionFound(object s, SessionEventArgs e)
        {
            var game = new Game { Name = e.SessionInfo.DisplayName, Info = e.SessionInfo };
            DispatcherHelper.ExecuteOnUIThreadAsync(() => Games.Add(game));
        }

        private void OnSessionRemoved(object s, SessionEventArgs e)
        {
            var host = new Game { Name = e.SessionInfo.DisplayName, Info = e.SessionInfo };
            var result = Games.FirstOrDefault(g => g.Info.ControllerDisplayName == e.SessionInfo.ControllerDisplayName);
            DispatcherHelper.ExecuteOnUIThreadAsync(() => Games.Remove(result));
        }

        public override void Cleanup()
        {
            App.SessionManager.SessionFound -= OnSessionFound;
            App.SessionManager.SessionRemoved -= OnSessionRemoved;
            App.SessionManager.StopSessionDiscovery();
            base.Cleanup(); 
        }
    }
}