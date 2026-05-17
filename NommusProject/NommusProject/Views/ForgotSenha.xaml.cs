using System;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Nommus
{
    public partial class ForgotPasswordWindow : Window
    {
        public ForgotPasswordWindow()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // apenas fecha, sem DialogResult
        }

        private void SendRecoveryButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                ShowErrorMessage("Por favor, insira um email válido.");
                return;
            }

            var button = sender as Button;
            if (button != null)
            {
                button.Content = "Enviando...";
                button.IsEnabled = false;
            }

            // Aqui você deve implementar a lógica real de envio de email
            // Por enquanto, mantemos a simulação
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                SuccessMessage.Visibility = Visibility.Visible;
                if (button != null)
                {
                    button.Content = "Email Enviado ✓";
                    button.Background = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                }

                var closeTimer = new System.Windows.Threading.DispatcherTimer();
                closeTimer.Interval = TimeSpan.FromSeconds(3);
                closeTimer.Tick += (closeS, closeArgs) =>
                {
                    closeTimer.Stop();
                    this.Close();
                };
                closeTimer.Start();
            };
            timer.Start();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        private void ShowErrorMessage(string message) { /* mantém */ }
        private void EmailTextBox_GotFocus(object sender, RoutedEventArgs e) { /* mantém */ }
        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e) { /* mantém */ }

        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close(); // apenas fecha
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape) this.Close();
        }
    }
}