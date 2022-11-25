using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace WpfChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string messageHandle = nameof(IChatClient.SendClientMessageToChat);
        string addMessageKey = nameof(IChatServer.AddMessageToChat);

        string subscribeKey = nameof(IChatServer.Subscribe);
        string unsubscribeKey = nameof(IChatServer.Unsubscribe);
        string downloadStreamKey = nameof(IChatServer.DownloadStream);
        string uploadStreamKey = nameof(IChatServer.UploadStream);

        string userName = String.Empty;

        HubConnection hubConnection = null;

        private bool subscribed;

        string serverUrl = "https://localhost:8001";

        public MainWindow()
        {
            InitializeComponent();
            string input = Microsoft.VisualBasic.Interaction.InputBox("WpfChatClient","Enter your name", "Anonymous...", -1, -1);
            userNameTxtBox.Text = input;
            userName = input;

            if (string.IsNullOrEmpty(this.userName))
            {
                this.userName = userNameTxtBox.Text;
            }

        }

        private async void connectBtn_Click(object sender, RoutedEventArgs e)
        {

            if (hubConnection == null)
                InitConnection();

            if (hubConnection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await hubConnection.StartAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (hubConnection.State == HubConnectionState.Connected)
            {
                await hubConnection.StopAsync();
            }

            if (hubConnection.State == HubConnectionState.Connected)
            {
                connectionStatus.Content = "Connected";
                connectionStatus.Foreground = Brushes.Green;
            }
            else
            {
                connectionStatus.Content = "Disconnected";
                connectionStatus.Foreground = Brushes.Red;
            }

        }

        private async void sendMessageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (hubConnection.State == HubConnectionState.Connected)
            {

                string message = messageTxtBox.Text;

                try
                {
                    //var mymessage = await hubConnection.InvokeAsync<ChatMessage>(addMessageKey, message);
                    await hubConnection.SendAsync(addMessageKey, message);
                    AppendTextToTextBox($"{DateTime.Now.ToString("HH:mm:ss")} :  Me", message, Brushes.Green);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    messageTxtBox.Clear();
                }
            }
        }

        private async void subscribeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (subscribed)
            {
                try
                {
                    await hubConnection.InvokeAsync(unsubscribeKey);
                    subscribed = false;
                    subscribeBtn.Content = "Subscribe";
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                }
            }
            else
            {
                try
                {
                    await hubConnection.InvokeAsync(subscribeKey);
                    subscribed = true;
                    subscribeBtn.Content = "Unsubscribe";
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                }
            }
        }

        private async void readStreamFromServerBtn_Click(object sender, RoutedEventArgs e)
        {
            var stream = hubConnection.StreamAsync<string>(downloadStreamKey);

            await foreach (string element in stream)
            {
                AppendTextToTextBox("Server stream", element, Brushes.Blue);
            }

            AppendTextToTextBox("Server stream", "Stream completed", Brushes.Blue);
        }

        private async void sendStreamToServerBtn_Click(object sender, RoutedEventArgs e)
        {
            var testAsyncEnumerable = TestAsyncEnumerable();
            await hubConnection.SendAsync(uploadStreamKey, testAsyncEnumerable);
        }

        private void InitConnection()
        {

            hubConnection = new HubConnectionBuilder().
                WithUrl( $"{serverPortTxtBox.Text}/chat",
                (HttpConnectionOptions options) => options.Headers.Add("username", userName))
                    .WithAutomaticReconnect()
                    .AddMessagePackProtocol() // added for using MessagePack protocol + using Microsoft.Extensions.DependencyInjection;
                    .Build();

            hubConnection.On<ChatMessage>(messageHandle, message =>
                 AppendTextToTextBox($"{message.CreatedAt.ToString("HH:mm:ss")} : {message.Caller}", message.Text, Brushes.Black));

            // delegates

            hubConnection.Closed += error =>
            {
                MessageBox.Show($"Connection closed. {error?.Message}");
                return Task.CompletedTask;
            };

            hubConnection.Reconnected += id =>
            {
                MessageBox.Show($"Connection reconnected with id: {id}");
                return Task.CompletedTask;
            };

            hubConnection.Reconnecting += error =>
            {
                MessageBox.Show($"Connection reconnecting. {error?.Message}");
                return Task.CompletedTask;
            };
        }

        private void AppendTextToTextBox(string sender, string text, Brush brush)
        {
            TextRange tr = new TextRange(chatTextBox.Document.ContentEnd, chatTextBox.Document.ContentEnd);
            tr.Text = string.Format("{0} : {1}{2}", sender, text, Environment.NewLine);
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
            }
            catch (FormatException) { }
            finally
            {
                chatTextBox.ScrollToEnd();
            }
        }

        private void ShowError(Exception ex)
        {
            MessageBox.Show(ex?.Message ?? "Error");
        }

        private async void Shutdown(object sender, EventArgs e)
        {
            await DisposeAsync(hubConnection);
        }
        async ValueTask DisposeAsync(HubConnection hubConnection)
        {
            if (this.hubConnection != null)
            {
                await this.hubConnection.DisposeAsync();
            }
        }

        async IAsyncEnumerable<string> TestAsyncEnumerable()
        {
            for (int i = 9; i >= 0; i--)
            {
                yield return $"Client {userNameTxtBox.Text} talks : {i.ToString().PadLeft(2, '0')} : {DateTime.Now.ToString("HH:mm:ss")}";
                await Task.Delay(1000);
            }
        }


    }


}
