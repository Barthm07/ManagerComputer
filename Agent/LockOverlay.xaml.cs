using System.Windows;

namespace ManagerComputer.Agent
{
    public partial class LockOverlay : Window
    {
        private bool _forceClose = false;

        public LockOverlay()
        {
            InitializeComponent();
            KeyDown += (s, e) => e.Handled = true;
            PreviewKeyDown += (s, e) => e.Handled = true;
            PreviewMouseDown += (s, e) => e.Handled = true;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_forceClose)
                e.Cancel = true; // block user from closing
        }

        public void ForceClose()
        {
            _forceClose = true;
            Close();
        }
    }
}