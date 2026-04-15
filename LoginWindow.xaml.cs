using System.Windows;
using ManagerComputer.Agent;
using ManagerComputer.Network;

namespace ManagerComputer
{
    public partial class LoginWindow : Window
    {
        private static readonly Dictionary<string, string> UserCredentials = new()
        {
            { "user1",  "User1"  },
            { "user2",  "User2"  },
            { "user3",  "User3"  },
            { "user4",  "User4"  },
            { "user5",  "User5"  },
            { "user6",  "User6"  },
            { "user7",  "User7"  },
            { "user8",  "User8"  },
            { "user9",  "User9"  },
            { "user10", "User10" },
        };

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameBox.Text.Trim().ToLower();
            var password = PasswordBox.Password;

            // Admin login → open Server UI
            if (username == "admin" && password == "admin")
            {
                var main = new MainWindow();
                main.Show();
                Close();
                return;
            }

            // User login → start Agent, connect to server
            if (UserCredentials.TryGetValue(username, out var expectedPassword) && password == expectedPassword)
            {
                try
                {
                    var agentService = new AgentService();

                    // Hook up lock overlay
                    LockOverlay? overlay = null;
                    agentService.LockRequested += () =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            overlay = new LockOverlay();
                            overlay.Show();
                        });
                    };
                    agentService.UnlockRequested += () =>
                    {
                        Dispatcher.Invoke(() => overlay?.ForceClose());
                    };

                    // Connect to server (admin PC IP)
                    await agentService.StartAsync("127.0.0.1"); // ← change to teacher's IP

                    var waiting = new Window
                    {
                        Title = "Lab Manager",
                        Content = new System.Windows.Controls.TextBlock
                        {
                            Text = "✅ Connected to server.\nWaiting for teacher instructions.",
                            FontSize = 18,
                            TextAlignment = System.Windows.TextAlignment.Center,
                            VerticalAlignment = System.Windows.VerticalAlignment.Center,
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            Foreground = System.Windows.Media.Brushes.White
                        },
                        Background = new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(30, 30, 46)),
                        Width = 400,
                        Height = 200,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };
                    waiting.Show();
                    Close();
                }
                catch
                {
                    ShowError("Could not connect to server. Is the teacher's PC running?");
                }
                return;
            }

            ShowError("Invalid username or password.");
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}