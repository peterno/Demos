using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SignalR_NuGet
{
    public class App : Application
    {
        #region GUI

        Label _timerText;
        Label _receiveText;
        private Entry _sendText;
        Button _sendButton;
        Button _startTimerButton;
        Button _startButton;
        Button _stopButton;
        private Picker _picker;
        private string _selectedUri;
        private ListView _consoleListView;
        private ObservableCollection<string> _consoleMessages;

        public App()
        {
            _timerText = new Label { Text = "This will show the timer event text." };
            _receiveText = new Label { Text = "This will show the received text." };
            _sendText = new Entry { Placeholder = "Enter text to send", Text = "" };
            _consoleMessages = new ObservableCollection<string>();
            _consoleListView = new ListView
            {
                ItemsSource = _consoleMessages,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HasUnevenRows = true,
                SeparatorVisibility = SeparatorVisibility.None
            };

            _picker = new Picker();
            _picker.Items.Add("http://192.168.250.133/chatdemo");
            _picker.Items.Add("http://demopeternsignalr.azurewebsites.net");
            _picker.Items.Add("http://demopeternsignalr2.azurewebsites.net");
            _picker.SelectedIndexChanged += (sender, args) => _selectedUri = _picker.Items[_picker.SelectedIndex];
            _picker.SelectedIndex = 0;

            _sendButton = new Button { Text = "Send", IsEnabled = false };
            _sendButton.Clicked += OnSendButtonOnClicked;
            _startTimerButton = new Button { Text = "Start timer", IsEnabled = false };
            _startTimerButton.Clicked += OnStartTimerButtonOnClicked;
            _startButton = new Button { Text = "Start", IsEnabled = true };
            _startButton.Clicked += async (sender, args) =>
            {
                _picker.IsEnabled = false;
                _startButton.IsEnabled = false;
                if (await Start())
                {
                    _stopButton.IsEnabled = true;
                }
                else
                {
                    _picker.IsEnabled = true;
                    _startButton.IsEnabled = true;
                }
            };
            _stopButton = new Button { Text = "Stop", IsEnabled = false };
            _stopButton.Clicked += OnStopButtonOnClicked;


            // The root page of your application
            MainPage = new ContentPage
            {
                Padding = new Thickness(0, Device.OnPlatform(20, 0, 0), 0, 0),
                Content = new StackLayout
                {
                    Children = {
						new Label {
							XAlign = TextAlignment.Center,
							Text = "Test of SignalR"
						},
                        _picker,
                        _startButton,
                        _stopButton,
                        new Label{ Text="---------------------------------", XAlign = TextAlignment.Center},
                        _sendText,
                        _sendButton,
                        _startTimerButton,
                        _receiveText,
                        _timerText,
                        _consoleListView
					}
                }
            };
        } 
        #endregion

        private async void OnStopButtonOnClicked(object sender, EventArgs args)
        {
            if (!IsMoveShape())
            {
                await _hub.Invoke("StopTimer", Device.OS.ToString());
            }
            _stopButton.IsEnabled = false;
            _shallReconnect = false;
            _connection.Stop();
            _picker.IsEnabled = true;
            _startButton.IsEnabled = true;
        }

        private async void OnSendButtonOnClicked(object sender, EventArgs args)
        {
            await _hub.Invoke("Send", Device.OS.ToString(), _sendText.Text);
        }

        private async void OnStartTimerButtonOnClicked(object sender, EventArgs args)
        {
            await _hub.Invoke("StartTimer", Device.OS.ToString());
            _startTimerButton.IsEnabled = false;
        }


        HubConnection _connection;
        IHubProxy _hub;
        private bool _shallReconnect;

        protected async Task<bool> Start()
        {
            try
            {
                var querystringData = new Dictionary<string, string> {{"identity", "Peter"}};

                _connection = new HubConnection(_selectedUri, querystringData); 
                _connection.TraceLevel = TraceLevels.All;
                _connection.TraceWriter = new DebugTextWriter();

                _connection.Closed += OnConnectionOnClosed;
                _connection.Received += s => Log("Received: " + s);
                _connection.Reconnected += OnReconnected;
                _connection.Reconnecting += OnReconnecting;
                _connection.ConnectionSlow += () => Log("ConnectionSlow");
                _connection.Error += exception => Log("Error: Exception: " + exception.ToString());
                _connection.StateChanged += OnStateChanged;

                if (IsMoveShape())
                {
                    _hub = _connection.CreateHubProxy("moveShape");
                }
                else
                {
                    _hub = _connection.CreateHubProxy("Chat");
                }

                // Start using ModernHttpClient
                await _connection.Start(new ModernHttpClientSignalR());

                if (IsMoveShape())
                {
                    _hub.On("shapeMoved", (double x, double y) =>
                    {
                        var text = string.Format("{0}: {1}", x, y);
                        Log(text);
                        Device.BeginInvokeOnMainThread(() => _receiveText.Text = text);
                    });

                    
                }
                else
                {
                    _hub.On("messageReceived", (string platform, string message) =>
                    {
                        var text = string.Format("{0}: {1}", platform, message);
                        Log(text);
                        Device.BeginInvokeOnMainThread(() => _receiveText.Text = text);
                    });

                    _hub.On("timerEvent", (string message) =>
                    {
                        Log(message);
                        Device.BeginInvokeOnMainThread(() => _timerText.Text = message);
                    });
                }

                return true;
            }
            catch (Exception exc)
            {
                Log(exc.ToString());
            }

            return false;
        }

        private void OnStateChanged(StateChange change)
        {
            Log("StateChanged: " + change.NewState);
            if (change.NewState == ConnectionState.Connected)
            {
                _shallReconnect = true;
                Device.BeginInvokeOnMainThread(() =>
                {
                    _sendButton.IsEnabled = !IsMoveShape();
                    _startTimerButton.IsEnabled = !IsMoveShape();
                });
            }
        }

        private void OnReconnecting()
        {
            Log("Reconnecting");
            Device.BeginInvokeOnMainThread(() => { _sendButton.IsEnabled = false; });
        }

        private void OnReconnected()
        {
            Log("Reconnected");
            Device.BeginInvokeOnMainThread(() => { _sendButton.IsEnabled = !IsMoveShape(); });
        }

        private async void OnConnectionOnClosed()
        {
            Log("Closed");
            Device.BeginInvokeOnMainThread(() =>
            {
                _sendButton.IsEnabled = false;
                _startTimerButton.IsEnabled = false;
            });

            if (_shallReconnect)
            {
                try
                {
                    Log("Waiting");
                    await Task.Delay(50);
                    Log("Restarting");
                    _connection.Stop();
                    await _connection.Start(new ModernHttpClientSignalR());
                    Log("Restarted");
                }
                catch (Exception exc)
                {
                    Log("Failed to restart: " + exc.ToString());
                }
            }
            else
            {
                Device.BeginInvokeOnMainThread(() => _picker.IsEnabled = true);
                Device.BeginInvokeOnMainThread(() => _startButton.IsEnabled = true);
            }
        }

        private bool IsMoveShape()
        {
            if (_selectedUri.Contains("http://demopeternsignalr2.azurewebsites.net"))
            {
                return true;
            }

            return false;
        }

        protected async override void OnStart()
        {
            // Handle when your app starts
            Log("Start");
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            Log("Sleep");
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
            Log("Resume");
        }

        private void Log(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var newMessage = message.Replace("\n", "").Replace("\r", "");
                _consoleMessages.Add(DateTime.Now.ToString("T") + ": " + newMessage);
                if (_consoleMessages.Count >= 100)
                {
                    _consoleMessages.RemoveAt(0);
                }
                _consoleListView.ScrollTo(_consoleMessages.Last(), ScrollToPosition.Start, true);
            });

            Debug.WriteLine(message);
        }

    }

    public class DebugTextWriter : TextWriter
    {
        private StringBuilder buffer;

        public DebugTextWriter()
        {
            buffer = new StringBuilder();
        }

        public override void Write(char value)
        {
            switch (value)
            {
                case '\n':
                    return;
                case '\r':
                    Debug.WriteLine(buffer.ToString());
                    buffer.Clear();
                    return;
                default:
                    buffer.Append(value);
                    break;
            }
        }

        public override void Write(string value)
        {
            Debug.WriteLine(value);

        }
        #region implemented abstract members of TextWriter
        public override Encoding Encoding
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}
