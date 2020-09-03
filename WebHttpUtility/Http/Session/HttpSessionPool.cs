using HttpServer.Server;
using HttpServer.Utils;
using System;
using System.Collections.Generic;
using System.Timers;

namespace HttpServer.Http.Session
{
    public class HttpSessionPool
    {
        private HttpServerContext serverContext;
        private Dictionary<Guid, HttpSession> Sessions = new Dictionary<Guid, HttpSession>();

        private readonly object Sessions_Lock = new object();

        private int interval;
        private Timer _timer = new Timer();

        public HttpSessionPool(HttpServerContext serverContext) {
            this.serverContext = serverContext;

            _timer.Elapsed += _timer_Elapsed;
            interval = serverContext.ServerConfig.SessionMaxInactiveMinutes / 2 * 60 * 1000;
            if (interval <= 0) interval = 2 * 60 * 1000;

            _timer.Interval = interval;
        }

        public void Start() {
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e) {
            lock (Sessions_Lock) {
                List<Guid> toRemove = new List<Guid>();
                long currentTime = DateTime.Now.GetUnixTime();
                foreach (var item in Sessions) {
                    if (item.Value._CheckExpired(currentTime)) {
                        item.Value.Invalidate();
                        toRemove.Add(item.Key);
                    }
                }
                foreach (var item in toRemove) {
                    Sessions.Remove(item);
                }
            }
        }

        public Guid GetNewSessionId() {
            Guid guid = Guid.NewGuid();
            return guid;
        }

        public HttpSession GetSessionFromPool(Guid sessionID) {
            lock (Sessions_Lock) {
                HttpSession session;
                if(sessionID == Guid.Empty) {
                    sessionID = GetNewSessionId();
                    session = new HttpSession(sessionID);
                    Sessions.Add(sessionID, session);
                }
                else if (Sessions.ContainsKey(sessionID)) {
                    session = Sessions[sessionID];
                    if (session.IsInvalidated) {
                        session = new HttpSession(sessionID);
                        Sessions[sessionID] = session;
                    }
                }
                else {
                    session = new HttpSession(sessionID);
                    Sessions.Add(sessionID, session);
                }
                session.SetAccessed();
                return session;
            }
        }
    }
}
