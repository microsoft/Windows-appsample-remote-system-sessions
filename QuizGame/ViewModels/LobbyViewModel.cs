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
using QuizGame.Models;

namespace QuizGame.ViewModels
{
    public abstract class LobbyViewModel : ViewModelBase
    {
        public LobbyViewModel()
        {
            Scorecards = new ObservableCollection<Scorecard>();
            App.SessionManager.MessageReceived += MessageReceivedAsync; 
        }

        private ObservableCollection<Scorecard> _scorecards;
        public ObservableCollection<Scorecard> Scorecards
        {
            get => _scorecards;
            set => SetProperty(ref _scorecards, value);
        }

        public string LobbyText
        {
            get => _lobbyText;
            set => SetProperty(ref _lobbyText, value);
        }
        private string _lobbyText;

        private string _gameName; 
        public string GameName
        {
            get => _gameName;
            set => SetProperty(ref _gameName, value); 
        }

        public abstract bool CanStartGame { get; }

        public abstract void StartGame(); 

        public abstract void LeaveGame();

        protected abstract void MessageReceivedAsync(object sender, MessageReceivedEventArgs e);

        public override void Cleanup()
        {
            App.SessionManager.MessageReceived -= MessageReceivedAsync; 
            base.Cleanup();
        }
    }
}
