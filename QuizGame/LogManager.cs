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
using Microsoft.Toolkit.Uwp;

namespace QuizGame
{
    public class Log
    {
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string DisplayMessage => ToString();
        public override string ToString() => $"[{Timestamp.ToString("HH:mm:ss")}] {Message}";
    }

    public class LogManager
    {
        public LogManager()
        {
            Messages = new ObservableCollection<Log>();
            App.SessionManager.DebugMessage += (s, e) => DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                Messages.Insert(0, new Log
                {
                    Message = e.Message,
                    Timestamp = DateTime.Now
                });
            });
        }

        public ObservableCollection<Log> Messages { get; set; }

        public void Write(string message)
        {
            Messages.Insert(0, new Log
            {
                Message = message,
                Timestamp = DateTime.Now
            });
        }
    }
}
