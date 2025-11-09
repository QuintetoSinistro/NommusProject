using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NommusProject
{
    public partial class RegisterScreen : Window
    {
        public RegisterScreen()
        {
            InitializeComponent();

            // TextBoxes
            NameTextBox.TextChanged += (s, e) => TogglePlaceholder(NameTextBox, NamePlaceholder);
            CpfTextBox.TextChanged += (s, e) => TogglePlaceholder(CpfTextBox, CpfPlaceholder);
            PhoneTextBox.TextChanged += (s, e) => TogglePlaceholder(PhoneTextBox, PhonePlaceholder);
            EmailTextBox.TextChanged += (s, e) => TogglePlaceholder(EmailTextBox, EmailPlaceholder);

            // PasswordBoxes
            PasswordBox.PasswordChanged += (s, e) => TogglePlaceholder(PasswordBox, PasswordPlaceholder);
            ConfirmPasswordBox.PasswordChanged += (s, e) => TogglePlaceholder(ConfirmPasswordBox, ConfirmPasswordPlaceholder);
        }

        private void TogglePlaceholder(TextBox box, TextBlock placeholder)
        {
            placeholder.Visibility = string.IsNullOrEmpty(box.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        private void TogglePlaceholder(PasswordBox box, TextBlock placeholder)
        {
            placeholder.Visibility = string.IsNullOrEmpty(box.Password) ? Visibility.Visible : Visibility.Hidden;
        }

        private void Login_click(object sender, RoutedEventArgs e)
        {
            UserLogin userLogin = new UserLogin();
            userLogin.Show();
            this.Close();
        }
    }
}