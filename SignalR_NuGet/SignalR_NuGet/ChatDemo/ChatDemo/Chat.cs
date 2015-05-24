using Microsoft.AspNet.SignalR;
using System;
using System.Diagnostics;
using System.Timers;

namespace ChatDemo
{
    public class Chat : Hub
    {
        public Chat()
        {
            Debug.WriteLine("Chat ctor");
        }
        protected override void Dispose(bool disposing)
        {
            Debug.WriteLine("Chat disposing(" + disposing + ")");
            base.Dispose(disposing);
        }
        public void Send(string platform, string message)
        {
            Clients.All.messageReceived(platform, message + Context.ConnectionId.ToString());
            Debug.WriteLine("Send (" + platform + "," + message + ") ID=" + Context.ConnectionId.ToString()); 
        }

        public void StartTimer(string platform)
        {
            DelayFactory.Delay.Start(2);
            Debug.WriteLine("StartTimer (" + platform + ") ID=" + Context.ConnectionId.ToString()); 
        }

        public void StopTimer(string platform)
        {
            DelayFactory.Delay.Stop();
            Debug.WriteLine("StopTimer (" + platform + ") ID=" + Context.ConnectionId.ToString()); 
        }

        public override System.Threading.Tasks.Task OnReconnected()
        {
            Debug.WriteLine("Reconnected");
            return base.OnReconnected();
        }
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            Debug.WriteLine("OnDisconnected(" + stopCalled + ")" + Context.ConnectionId.ToString());  
            return base.OnDisconnected(stopCalled);
        }
        public override System.Threading.Tasks.Task OnConnected()
        {
            Debug.WriteLine("OnConnected " + Context.ConnectionId.ToString());  
            return base.OnConnected();
        }



    }


    public class MyDelay
    {
        Timer myTimer;
        int reference_Count = 0;

        public void Start(int delay)
        {
            if (myTimer == null)
            {
                myTimer = new Timer();
                myTimer.Elapsed += myEvent;
                myTimer.Interval = delay * 1000;
                myTimer.Enabled = true;
            }
            reference_Count++;
        }
        public void Stop()
        {
            reference_Count--;
            if (reference_Count == 0)
            {
                if (myTimer != null)
                {
                    myTimer.Enabled = false;
                    myTimer.Dispose();
                    myTimer = null;
                }
            }
        }
        private void myEvent(object source, ElapsedEventArgs e)
        {
            string dt = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff");
            IHubContext iChatHubContext = GlobalHost.ConnectionManager.GetHubContext<Chat>();
            iChatHubContext.Clients.All.timerEvent(dt);
        }
    }
    public static class DelayFactory
    {
        public static MyDelay Delay = new MyDelay();
    }
}