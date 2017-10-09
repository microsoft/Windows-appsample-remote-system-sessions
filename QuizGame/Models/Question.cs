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

using System.Collections.Generic;
using QuizGame.ViewModels;

namespace QuizGame.Models
{
    /// <summary>
    /// A quiz question that is displayed in the game host UI and sent to players.
    /// </summary>
    public class Question : ViewModelBase
    {
        private string _text; 
        /// <summary>
        /// The question text.
        /// </summary>
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value); 
        }

        /// <summary>
        /// List of available answers.
        /// </summary>
        public List<Option> Options { get; set; }

        /// <summary>
        /// The index of the correct answer in Options.
        /// </summary>
        public int CorrectAnswerIndex { get; set; }
    }

    public class Option : ViewModelBase
    {
        public string Text { get; set; }
        public bool IsCorrectAnswer { get; set; }

        public bool _isLocalPlayerSelection;
        // Updated when the local player selects this option. 
        public bool IsLocalPlayerSelection
        {
            get => _isLocalPlayerSelection;
            set
            {
                if (SetProperty(ref _isLocalPlayerSelection, value))
                {
                    OnPropertyChanged(nameof(IsIncorrectSelectionIndicatorVisible));
                }
            }
        }

        public bool IsIncorrectSelectionIndicatorVisible => !IsCorrectAnswer && IsLocalPlayerSelection;

        public double Opacity => IsCorrectAnswer ? 1.0 : 0.4;
    }
}
