using Nommus;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NommusProject
{
    public partial class IncomeWindow : Window
    {
        public IncomeWindow()
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

        private void AddIncome_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Por favor, insira uma descrição para a receita.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ValueTextBox.Text) || ValueTextBox.Text == "0,00")
            {
                MessageBox.Show("Por favor, insira um valor válido para a receita.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Receita adicionada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

            // Limpar campos após adicionar
            DescriptionTextBox.Text = "";
            ValueTextBox.Text = "0,00";
            CategoryComboBox.SelectedIndex = -1;
            FixedRadio.IsChecked = true;
        }

        private void PlanFutureIncome_Click(object sender, RoutedEventArgs e)
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

            MessageBox.Show("Receita futura planejada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

            // Limpar campos do planejamento futuro
            FutureValueTextBox.Text = "0,00";
            FutureDateTextBox.Text = "dd/MM/aaaa";
            FuturePlanningPanel.Visibility = Visibility.Collapsed;
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
            try
            {
                ExpensesWindow expensesWindow = new ExpensesWindow();
                expensesWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao navegar para Gastos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void IncomeButton_Click(object sender, RoutedEventArgs e)
        {
            // Já está na tela de receitas
            MessageBox.Show("Você já está na tela de Receitas", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar tela de Metas
            MessageBox.Show("Navegar para Metas", "Navegação", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}