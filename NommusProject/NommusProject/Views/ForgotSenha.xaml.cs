using System;
using System.Windows;
using System.Windows.Controls;
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
            this.DialogResult = true;
            this.Close();
        }

        private void SendRecoveryButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                ShowErrorMessage("Por favor, insira um email válido.");
                return;
            }

            // Desabilitar o botão imediatamente
            var button = sender as Button;
            if (button != null)
            {
                button.Content = "Enviando...";
                button.IsEnabled = false;
            }

            // Simular envio de email (com delay)
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, args) =>
            {
                timer.Stop();

                // Mostrar mensagem de sucesso
                SuccessMessage.Visibility = Visibility.Visible;
                if (button != null)
                {
                    button.Content = "Email Enviado ✓";
                    button.Background = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                }

                // Fechar automaticamente após 3 segundos
                var closeTimer = new System.Windows.Threading.DispatcherTimer();
                closeTimer.Interval = TimeSpan.FromSeconds(3);
                closeTimer.Tick += (closeS, closeArgs) =>
                {
                    closeTimer.Stop();
                    this.DialogResult = true;
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

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void EmailTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            EmailPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(EmailTextBox.Text))
            {
                EmailPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Fechar a janela se clicar no overlay (fora do conteúdo)
            this.DialogResult = true;
            this.Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Fechar com ESC
            if (e.Key == Key.Escape)
            {
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}