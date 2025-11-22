using Nommus;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NommusProject
{
    public partial class ExpensesWindow : Window
    {
        public ExpensesWindow()
        {
            InitializeComponent();
        }

        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao voltar para dashboard: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Por favor, insira uma descrição para o gasto.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ValueTextBox.Text) || ValueTextBox.Text == "0,00")
            {
                MessageBox.Show("Por favor, insira um valor válido para o gasto.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Gasto adicionado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

            // Limpar campos após adicionar
            DescriptionTextBox.Text = "";
            ValueTextBox.Text = "0,00";
            CategoryComboBox.SelectedIndex = -1;
            FixedRadio.IsChecked = true;
        }

        private void PlanFutureExpense_Click(object sender, RoutedEventArgs e)
        {
            if (FuturePlanningPanel.Visibility == Visibility.Visible)
            {
                FuturePlanningPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                FuturePlanningPanel.Visibility = Visibility.Visible;
            }
        }

        private void SaveFuturePlan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FutureValueTextBox.Text) || FutureValueTextBox.Text == "0,00")
            {
                MessageBox.Show("Por favor, insira um valor válido para o planejamento futuro.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(FutureDateTextBox.Text) || FutureDateTextBox.Text == "dd/MM/aaaa")
            {
                MessageBox.Show("Por favor, insira uma data válida para o planejamento futuro.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Gasto futuro planejado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

            // Limpar campos do planejamento futuro
            FutureValueTextBox.Text = "0,00";
            FutureDateTextBox.Text = "dd/MM/aaaa";
            FuturePlanningPanel.Visibility = Visibility.Collapsed;
        }

        private void ExportReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Relatório de gastos exportado com sucesso!", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void FilterExpenses_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Filtro aplicado aos gastos!", "Filtrar", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Tem certeza que deseja limpar todos os gastos?\nEsta ação não pode ser desfeita.",
                                        "Confirmar Limpeza",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Todos os gastos foram removidos!", "Limpeza Concluída", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Navigation methods
        // Navigation methods
        private void FinanceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao navegar para Finanças: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CardsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar tela de Cartões
            MessageBox.Show("Navegar para Cartões", "Navegação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExpensesButton_Click(object sender, RoutedEventArgs e)
        {
            // Já está na tela de gastos
            MessageBox.Show("Você já está na tela de Gastos", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CreditsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar tela de Créditos
            MessageBox.Show("Navegar para Créditos", "Navegação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar tela de Metas
            MessageBox.Show("Navegar para Metas", "Navegação", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}