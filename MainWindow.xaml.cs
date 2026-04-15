using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using ManagerComputer.Models;
using ManagerComputer.Network;
using ManagerComputer.Services;

namespace ManagerComputer
{
    public partial class MainWindow : Window
    {
        private readonly ComputerManager _manager = new();
        private readonly ServerListener _listener = new();
        private Computer? _selectedComputer;

        public MainWindow()
        {
            InitializeComponent();
            PcList.ItemsSource = _manager.Computers;
            _manager.FrameReceived += OnFrameReceived;
            _listener.ClientConnected += _manager.AddClient;
            _ = _listener.StartAsync();
        }

        private void PcList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedComputer = PcList.SelectedItem as Computer;
        }

        private void OnFrameReceived(Computer computer, byte[] data)
        {
            if (computer != _selectedComputer) return;

            Dispatcher.Invoke(() =>
            {
                using var ms = new MemoryStream(data);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();
                ScreenPreview.Source = bitmap;
            });
        }

        private void LockButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedComputer != null)
                _manager.SendLock(_selectedComputer);
        }

        private void UnlockButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedComputer != null)
                _manager.SendUnlock(_selectedComputer);
        }

        private void LockAllButton_Click(object sender, RoutedEventArgs e)
        {
            _manager.LockAll();
        }

        private void UnlockAllButton_Click(object sender, RoutedEventArgs e)
        {
            _manager.UnlockAll();
        }
    }
}