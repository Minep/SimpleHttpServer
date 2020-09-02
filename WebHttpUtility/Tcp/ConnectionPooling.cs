using HttpServer.Common;
using HttpServer.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace HttpServer.tcp
{
    public class ConnectionPooling
    {
        private Timer timer_idle;
        private Timer timer_dead;

        private double ratio = 0;
        private double thershold = 0;
        public double IdleCheckIntervalMS
        {
            get => timer_idle.Interval;
            set
            {
                timer_idle.Interval = value;
            }
        }

        public double DeadCheckIntervalMS
        {
            get => timer_dead.Interval;
            set {
                timer_dead.Interval = value;
            }
        }

        public int MaxConnections { get; set; } = 100;
        public int MaxDisposeConnection { get; set; } = 5;

        /// <summary>
        /// Try release <see cref="MaxDisposeConnection"/> idle connections when the present
        /// connections exceed <see cref="DisposeThersholdPrecentage"/> times  
        /// of <see cref="MaxConnections"/>
        /// </summary>
        public double DisposeThersholdPrecentage
        {
            get => thershold;
            set
            {
                thershold = value;
                ratio = timer_idle.Interval * thershold;
            }
        }

        private Heap<HttpSession> connectionHeap = new Heap<HttpSession>(true);


        public ConnectionPooling(HttpServerContext serverContext)
        {
            timer_idle = new Timer();
            timer_dead = new Timer();
         
            IdleCheckIntervalMS = serverContext.ServerConfig.IdleConnectionCleanUpInterval;
            DeadCheckIntervalMS = serverContext.ServerConfig.DeadConnectionCleanUpInterval;
            MaxConnections = serverContext.ServerConfig.MaxConnectionAccepted;
            MaxDisposeConnection = serverContext.ServerConfig.MaxCleanUpNumber;
            DisposeThersholdPrecentage = serverContext.ServerConfig.CleanUpThersholdRatio;

            timer_idle.Elapsed += IdleChecking;
            timer_dead.Elapsed += DeadChecking;
        }

        private void IdleChecking(object sender, ElapsedEventArgs e)
        {
            ReleaseIdles();
        }

        private void DeadChecking(object sender, ElapsedEventArgs e) {
            ReleaseDead();
        }

        public void PutConnection(HttpSession connection)
        {
            if (connectionHeap.Count < MaxConnections)
            {
                connectionHeap.Append(connection);
            }
            else
            {
                ReleaseIdles();
            }
        }

        public void StartChecking()
        {
            timer_idle.Start();
            timer_dead.Start();
        }

        public void StopChecking() {
            timer_idle.Stop();
            timer_dead.Stop();
        }

        public void ReleaseIdles()
        {
            connectionHeap
                .ExtractSuccessiveTop(MaxDisposeConnection)
                .ForEach(state => state.Dispose());
        }

        public void ReleaseDead() {
            connectionHeap
                .RemoveWhere(session => session.IsShutDown)
                ?.Dispose();
        }
    }
}
