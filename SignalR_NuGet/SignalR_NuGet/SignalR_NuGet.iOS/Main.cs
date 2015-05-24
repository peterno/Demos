using System;
using System.Diagnostics;

using UIKit;

namespace SignalR_NuGet.iOS
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            try
            {
                UIApplication.Main(args, null, "AppDelegate");
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.ToString());
            }
        }
    }
}
