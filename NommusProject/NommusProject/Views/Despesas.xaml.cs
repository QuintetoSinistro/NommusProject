using System;
using System.Windows;
using System.Windows.Input;
using NommusProject.Data;
using Nommus;

namespace NommusProject
{
    public partial class ExpensesWindow : Window
    {
        private bool _popupAberto = false;

        public ExpensesWindow()
        {
            InitializeComponent();
            CarregarDadosUsuario();
            // Chame aqui seus métodos originais de carregar dados
        }

        private void CarregarDadosUsuario()
        {
            var usuario = SessaoUsuario.UsuarioLogado;
            if (usuario == null) return;
            if (UsuarioNomeText != null) UsuarioNomeText.Text = usuario.Nome;
            if (PopupNomeText != null) PopupNomeText.Text = usuario.Nome;
            if (PopupEmailText != null) PopupEmailText.Text = usuario.Email;
        }

        // Lógica do Popup
        private void UsuarioCard_Click(object sender, MouseButtonEventArgs e)
        {
            _popupAberto = !_popupAberto;
            UserPopupCard.Visibility = _popupAberto ? Visibility.Visible : Visibility.Collapsed;
            PopupOverlay.Visibility = _popupAberto ? Visibility.Visible : Visibility.Collapsed;
            e.Handled = true;
        }

        private void FecharPopup_Click(object sender, MouseButtonEventArgs e)
        {
            _popupAberto = false;
            UserPopupCard.Visibility = Visibility.Collapsed;
            PopupOverlay.Visibility = Visibility.Collapsed;
        }

        private void PopupConfiguracoes_Click(object sender, RoutedEventArgs e)
        {
            new ConfiguracoesWindow().Show();
            this.Close();
        }

        private void PopupLogout_Click(object sender, RoutedEventArgs e)
        {
            SessaoUsuario.Logout();
            new UserLogin().Show();
            this.Close();
        }

        // Navegação
        private void FinanceButton_Click(object sender, RoutedEventArgs e) { new MainWindow().Show(); this.Close(); }
        private void CardsButton_Click(object sender, RoutedEventArgs e) { /* Lógica Cartões */ }
        private void ExpensesButton_Click(object sender, RoutedEventArgs e) { }
        private void CreditsButton_Click(object sender, RoutedEventArgs e) { new IncomeWindow().Show(); this.Close(); }
        private void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            new MetasWindow().Show();
            this.Close();
        }
        private void BackToDashboard_Click(object sender, RoutedEventArgs e) { new MainWindow().Show(); this.Close(); }

        // MANTENHA SEUS MÉTODOS ORIGINAIS ABAIXO:
        private void AddExpense_Click(object sender, RoutedEventArgs e) { /* Sua lógica de adicionar */ }
        private void RemoverDespesa_Click(object sender, RoutedEventArgs e) { /* Sua lógica de remover */ }
        private void PlanFutureExpense_Click(object sender, RoutedEventArgs e)
        {
            FuturePlanningPanel.Visibility = FuturePlanningPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
        private void SaveFuturePlan_Click(object sender, RoutedEventArgs e) { /* Sua lógica de salvar plano */ }
        private void ExportReport_Click(object sender, RoutedEventArgs e) { /* Sua lógica de exportar */ }
        private void FilterExpenses_Click(object sender, RoutedEventArgs e) { /* Sua lógica de filtrar */ }
        private void ClearAll_Click(object sender, RoutedEventArgs e) { /* Sua lógica de limpar */ }
    }
}
