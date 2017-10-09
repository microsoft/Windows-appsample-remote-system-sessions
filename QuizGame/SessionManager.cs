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
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.System.RemoteSystems;

namespace QuizGame
{
    public class SessionManager
    {
        private const string Key = "data";

        private RemoteSystemSessionWatcher m_sessionWatcher;
        private RemoteSystemSession m_currentSession;
        private String m_currentSessionName;
        private RemoteSystemSessionMessageChannel m_appMessageChannel;

        public event EventHandler<ParticipantJoinedEventArgs> ParticipantJoined = delegate { };

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public event EventHandler<SessionEventArgs> SessionFound = delegate { };
        public event EventHandler<SessionEventArgs> SessionRemoved = delegate { };
        public event EventHandler<RemoteSystemSessionDisconnectedEventArgs> SessionDisconnected = delegate { };
        public event EventHandler<DebugMessageEventArgs> DebugMessage = delegate { };

        public RemoteSystemSessionParticipant SessionHost { get; set; }

        private bool EnumerationCompleted { get; set; } = false;

        #region Create Sessions

        // Creates a new session, shared experience, for particpants to join 
        // Handles incoming requests to join the newly created session
        public async Task<bool> CreateSession(string sessionName)
        {
            bool status = false;
            RemoteSystemAccessStatus accessStatus = await RemoteSystem.RequestAccessAsync();

            // Checking that remote system access is allowed - ensure capability in the project manifest is set
            if (accessStatus != RemoteSystemAccessStatus.Allowed)
            {
                return status;
            }

            m_currentSessionName = sessionName;
            SendDebugMessage($"Creating session {m_currentSessionName}...");
            var manager = new RemoteSystemSessionController(m_currentSessionName);

            // Handles incoming requests to join the session.
            manager.JoinRequested += Manager_JoinRequested;

            try
            {
                // Create session
                RemoteSystemSessionCreationResult createResult = await manager.CreateSessionAsync();

                if (createResult.Status == RemoteSystemSessionCreationStatus.Success)
                {
                    RemoteSystemSession currentSession = createResult.Session;
                    // Handles disconnect
                    currentSession.Disconnected += (sender, args) =>
                    {
                        SessionDisconnected(sender, args);
                    };

                    m_currentSession = currentSession;

                    SendDebugMessage($"Session {m_currentSession.DisplayName} created successfully.");

                    status = true;
                }
                // Session creation has reached a maximum - message user that there are too many sessions
                else if (createResult.Status == RemoteSystemSessionCreationStatus.SessionLimitsExceeded)
                {
                    status = false;
                    SendDebugMessage("Session limits exceeded.");
                }
                // Failed to create the session
                else
                {
                    status = false;
                    SendDebugMessage("Failed to create session.");
                }
            }
            catch (Win32Exception)
            {
                status = false;
                SendDebugMessage("Failed to create session.");
            }

            return status;
        }

        private void Manager_JoinRequested(RemoteSystemSessionController sender, RemoteSystemSessionJoinRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();
            SendDebugMessage($"Added the participant {args.JoinRequest.Participant.RemoteSystem.DisplayName} to the session {m_currentSessionName}.");
            args.JoinRequest.Accept();
            ParticipantJoined(this, new ParticipantJoinedEventArgs() { Participant = args.JoinRequest.Participant });
            deferral.Complete();
        }

        public void EndSession()
        {
            if (m_currentSession != null)
            {
                m_currentSession.Dispose();
            }

            if (m_sessionWatcher != null)
            {
                m_sessionWatcher.Stop();
                m_sessionWatcher.Added -= RemoteSystemSessionWatcher_RemoteSessionAdded;
                m_sessionWatcher.Removed -= RemoteSystemSessionWatcher_RemoteSessionRemoved;
                m_sessionWatcher = null;
            }

            SendDebugMessage("Session ended.");
        }
        #endregion

        #region Discover Sessions

        // Discovery of existing sessions
        // Handles the addition, removal, or updating of a session
        public async void DiscoverSessions()
        {
            try
            {

                RemoteSystemAccessStatus status = await RemoteSystem.RequestAccessAsync();
                // Checking that remote system access is allowed - ensure capability in the project manifest is set
                if (status != RemoteSystemAccessStatus.Allowed)
                {
                    SendDebugMessage("Access is denied, ensure the \'bluetooth\' and \'remoteSystem\' capabilities are set.");
                    return;
                }

                //  Create watcher to observe for added, updated, or removed sessions
                m_sessionWatcher = RemoteSystemSession.CreateWatcher();
                m_sessionWatcher.Added += RemoteSystemSessionWatcher_RemoteSessionAdded;
                m_sessionWatcher.Removed += RemoteSystemSessionWatcher_RemoteSessionRemoved;
                m_sessionWatcher.Start();
                SendDebugMessage("Session discovery started.");
            }
            catch (Win32Exception)
            {
                SendDebugMessage("Session discovery failed.");
            }
        }

        // Discovered sessions are joined, messsge channel established
        private void RemoteSystemSessionWatcher_RemoteSessionAdded(RemoteSystemSessionWatcher sender, RemoteSystemSessionAddedEventArgs args)
        {
            SendDebugMessage($"Discovered Session {args.SessionInfo.DisplayName}:{args.SessionInfo.ControllerDisplayName}.");
            SessionFound(this, new SessionEventArgs() { SessionInfo = args.SessionInfo });
        }

        private void RemoteSystemSessionWatcher_RemoteSessionRemoved(RemoteSystemSessionWatcher sender, RemoteSystemSessionRemovedEventArgs args)
        {
            SendDebugMessage($"Session {args.SessionInfo.DisplayName}:{args.SessionInfo.ControllerDisplayName} was ended by host.");
            SessionRemoved(this, new SessionEventArgs() { SessionInfo = args.SessionInfo });
        }

        public void StopSessionDiscovery()
        {
            try
            {
                m_sessionWatcher.Added -= RemoteSystemSessionWatcher_RemoteSessionAdded;
                m_sessionWatcher.Removed -= RemoteSystemSessionWatcher_RemoteSessionRemoved;
                m_sessionWatcher.Stop();
                SendDebugMessage("Session discovery ended.");
            }
            catch (Exception)
            {
                SendDebugMessage("Failed to end session discovery.");
            }
        }
        #endregion

        #region Join Sessions

        // Joins the current session, handles session disconnect - rejoin TBD
        public async Task<bool> JoinSession(RemoteSystemSessionInfo session, string name)
        {
            bool status = false;

            RemoteSystemSessionJoinResult joinResult = await session.JoinAsync();

            var watcher = joinResult.Session.CreateParticipantWatcher();

            watcher.Added += (s, e) =>
            {
                if (e.Participant.RemoteSystem.DisplayName.Equals(session.ControllerDisplayName))
                {
                    SessionHost = e.Participant;
                }
            };

            watcher.EnumerationCompleted += (s, e) =>
            {
                EnumerationCompleted = true;

                if (SessionHost == null)
                {
                    SendDebugMessage("Session host was not found during enumeration.");
                }
            };

            watcher.Start();

            // Checking that remote system access is allowed - ensure capability in the project manifest is set
            if (joinResult.Status == RemoteSystemSessionJoinStatus.Success)
            {
                // Join session
                m_currentSession = joinResult.Session;

                // Handles disconnect
                m_currentSession.Disconnected += (sender, args) =>
                {
                    SendDebugMessage("Session was disconnected.");
                    SessionDisconnected(sender, args);
                };

                status = true;
            }
            else
            {
                status = false;
                SendDebugMessage("Failed to join.");
            }

            return status;
        }
        #endregion

        #region Session Messaging

        // Send simple message to all session participants
        // Send simple message to specific session participants
        // Receive message

        public async Task SendMessageToParticipantsAsync(object message)
        {
            try
            {
                // The messaging channel accepts a string, channel name - "Everyone" - all 
                if (m_appMessageChannel == null)
                    m_appMessageChannel = new RemoteSystemSessionMessageChannel(m_currentSession, "Everyone");
            }

            catch (Win32Exception)
            {
                SendDebugMessage("Failed to send message to all paticipants.");
            }

            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(message.GetType()).WriteObject(stream, message);
                byte[] data = stream.ToArray();

                // Send message to all
                ValueSet sentMessage = new ValueSet { [Key] = data }; 

                // Send specific participants
                await m_appMessageChannel.BroadcastValueSetAsync(sentMessage);
            }
        }

        public void StartReceivingMessages()
        {
            m_appMessageChannel = new RemoteSystemSessionMessageChannel(m_currentSession, "Everyone");
            m_appMessageChannel.ValueSetReceived += (sender, args) =>
            {
                ValueSet receivedMessage = args.Message;
                MessageReceived(this, new MessageReceivedEventArgs() { ReceivedMessage = receivedMessage[Key], Id = args.Sender.RemoteSystem.Id });
            };
        }

        public async Task<bool> SendMessageToHostAsync(object message)
        {
            bool status = false;

            // Host isn't enumerated, so message can't be sent.
            if (!EnumerationCompleted)
            {
                return status;
            }

            try
            {
                if (m_appMessageChannel == null)
                    m_appMessageChannel = new RemoteSystemSessionMessageChannel(m_currentSession, "Everyone");

                byte[] data;
                using (var stream = new MemoryStream())
                {
                    new DataContractJsonSerializer(message.GetType()).WriteObject(stream, message);
                    data = stream.ToArray();
                }

                // Send message to specific participant, in this case, the host.
                ValueSet sentMessage = new ValueSet
                {
                    [Key] = data
                };
                await m_appMessageChannel.SendValueSetAsync(sentMessage, SessionHost);

                SendDebugMessage("Message successfully sent to host.");
                status = true;
            }
            catch (Win32Exception)
            {
                SendDebugMessage("Failed to send message to host.");
                status = false;
            }

            return status;
        }
        #endregion

        public void SendDebugMessage(string message)
        {
            DebugMessage(this, new DebugMessageEventArgs
            {
                Message = message,
                Timestamp = DateTime.Now
            });
        }
    }

    public class ParticipantJoinedEventArgs : EventArgs
    {
        public RemoteSystemSessionParticipant Participant { get; set; }
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public void GetDeserializedMessage(ref object message)
        {
            using (var stream = new MemoryStream((byte[])ReceivedMessage))
            {
                message = new DataContractJsonSerializer(message.GetType()).ReadObject(stream);
            }
        }

        public object ReceivedMessage { get; set; }

        public string Id { get; set; }
    }

    public class SessionEventArgs
    {
        public RemoteSystemSessionInfo SessionInfo { get; set; }
    }

    public class DebugMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public bool IsErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
        public override string ToString() => $"[{Timestamp}]: {Message}"; 
    }
}
