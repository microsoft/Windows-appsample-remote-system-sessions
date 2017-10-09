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
using System.Runtime.Serialization;

namespace QuizGame.Models
{
    [DataContract]
    public class Scorecard
    {
        [DataMember]
        public Guid PlayerId { get; set; }
        [DataMember]
        public string PlayerName { get; set; }
        [DataMember]
        public AnswerState AnswerStatus { get; set; }
        [DataMember]
        public int TotalCorrectAnswers { get; set; }
        [DataMember]
        public TimeSpan TotalTime { get; set; }
        [DataMember]
        public int TotalScore { get; set; }
        [DataMember]
        public bool IsLeader { get; set; }

        public string NameIcon => PlayerName == null ? "P" : PlayerName[0].ToString();
        public string CorrectAnswersString => $"{TotalCorrectAnswers}/4";
        public string TimeString => TotalTime.ToString("mm':'ss"); 
        public bool IsWaiting => AnswerStatus == AnswerState.Unanswered;
        public bool IsCorrect => AnswerStatus == AnswerState.AnsweredCorrectly;
        public bool IsIncorrect => AnswerStatus == AnswerState.AnsweredIncorrectly;
    }

    public class ScorecardComparer : IEqualityComparer<Scorecard>
    {
        public bool Equals(Scorecard x, Scorecard y)
        {
            return x.PlayerName == y.PlayerName &&
                x.AnswerStatus == y.AnswerStatus &&
                x.TotalCorrectAnswers == y.TotalCorrectAnswers &&
                x.TotalTime == y.TotalTime &&
                x.TotalScore == y.TotalScore &&
                x.IsLeader == y.IsLeader; 
        }

        public int GetHashCode(Scorecard obj)
        {
            int hash = 13;
            hash = (hash * 7) + obj.PlayerName.GetHashCode();
            hash = (hash * 7) + obj.TotalTime.GetHashCode();
            hash = (hash * 7) + obj.TotalCorrectAnswers.GetHashCode();
            return hash; 
        }
    }
}
