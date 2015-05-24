using Microsoft.AspNet.SignalR.Client.Http;
using ModernHttpClient;
using System.Net.Http;
using Xamarin.Forms;

namespace SignalR_NuGet
{
    public class ModernHttpClientSignalR : DefaultHttpClient
    {
        protected override HttpMessageHandler CreateHandler()
        {
            if (Device.OS == TargetPlatform.iOS || Device.OS == TargetPlatform.Android)
            {
                return new NativeMessageHandler();
            }

            return base.CreateHandler();
        }
    }

}
