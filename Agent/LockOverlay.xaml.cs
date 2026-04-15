using System.Windows;
using System.Windows.Input;

namespace ManagerComputer.Agent
{
    public partial class LockOverlay : Window
    {
        public LockOverlay()
        {
            InitializeComponent();
            // Block all keyboard and mouse input
            KeyDown += (s, e) => e.Handled = true;
            PreviewKeyDown += (s, e) => e.Handled = true;
            PreviewMouseDown += (s, e) => e.Handled = true;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Prevent user from closing it manually
            e.Cancel = true;
        }

        public void ForceClose()
        {
            Closing -= (s, e) => { };
            // Temporarily allow closing
            var handler = new System.ComponentModel.CancelEventHandler((s, e) => e.Cancel = false);
            Closing += handler;
            Close();
        }
    }
}