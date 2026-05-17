using Nommus;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NommusProject
{
    public partial class IncomeWindow : Window
    {
        private ObservableCollection<Transacao> _receitasCollection;

        public IncomeWindow()
        {
            InitializeComponent();
            InicializarComponentes();
            CarregarDadosUsuario();
            CarregarReceitas();
        }

        // Inicializa os componentes da tela
        private void InicializarComponentes()
        {
            _receitasCollection = new ObservableCollection<Transacao>();
            ReceitasItemsControl.ItemsSource = _receitasCollection;
        }

        // Carrega e exibe os dados do usuário logado
        private void CarregarDadosUsuario()
        {
            if (SessaoUsuario.UsuarioLogado != null)
            {
                var usuario = SessaoUsuario.UsuarioLogado;

                if (UsuarioNomeText != null)
                    UsuarioNomeText.Text = usuario.Nome;

                if (UsuarioTipoText != null)
                {
                    UsuarioTipoText.Text = usuario.Tipo.ToString();
                    ConfigurarCorTipoUsuario();
                }
            }
        }

        // Define a cor do tipo de usuário na sidebar
        private void ConfigurarCorTipoUsuario()
        {
            var usuario = SessaoUsuario.UsuarioLogado;
            if (usuario == null) return;

            switch (usuario.Tipo)
            {
                case TipoUsuario.Basic:
                    UsuarioTipoText.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(59, 130, 246)); // Azul
                    break;
                case TipoUsuario.Premium:
                    UsuarioTipoText.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(245, 158, 11)); // Dourado
                    break;
                case TipoUsuario.Adm:
                    UsuarioTipoText.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(239, 68, 68)); // Vermelho
                    break;
            }
        }

        // Volta para a tela principal do dashboard
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
                MessageBox.Show($"Erro ao voltar para dashboard: {ex.Message}", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Adiciona uma nova receita
        private async void AddIncome_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCamposReceita())
                return;

            try
            {
                var (valor, categoria, tipoReceita, data) = ProcessarDadosFormulario();
                var receita = CriarNovaReceita(valor, categoria, tipoReceita, data);

                await SalvarReceita(receita);
                LimparFormulario();
                AtualizarListaReceitas(receita);
            }
            catch (FormatException)
            {
                MessageBox.Show("Por favor, insira um valor numérico válido.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao adicionar receita: {ex.Message}", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Valida os campos do formulário de receita
        private bool ValidarCamposReceita()
        {
            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Por favor, insira uma descrição para a receita.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ValueTextBox.Text) || ValueTextBox.Text == "0,00")
            {
                MessageBox.Show("Por favor, insira um valor válido para a receita.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // Processa e converte os dados do formulário
        private (double valor, string categoria, string tipoReceita, DateTime data) ProcessarDadosFormulario()
        {
            // Converter valor
            decimal valorDecimal = Convert.ToDecimal(ValueTextBox.Text.Replace("R$", "").Trim());
            double valor = (double)valorDecimal;

            // Obter categoria selecionada
            string categoria = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Outros";

            // Obter tipo de receita
            string tipoReceita = "Fixa";
            if (VariableRadio.IsChecked == true) tipoReceita = "Variável";
            if (ExtraRadio.IsChecked == true) tipoReceita = "Extra";

            // Converter data
            DateTime data = DateTime.Parse(DateTextBox.Text);

            return (valor, categoria, tipoReceita, data);
        }

        // Cria um novo objeto Receita
        private Receita CriarNovaReceita(double valor, string categoria, string tipoReceita, DateTime data)
        {
            return new Receita
            {
                DescricaoTransacao = DescriptionTextBox.Text,
                ValorTransacao = valor,
                CategoriaId = categoria.ToLower().Replace(" ", "-"),
                DataTransacao = data,
                FonteReceita = categoria,
                ReceitaRecorrente = tipoReceita == "Fixa",
                UsuarioId = SessaoUsuario.UsuarioLogado.Id,
                TipoTransacao = "Receita"
            };
        }

        // Salva a receita e atualiza o saldo do usuário
        private async System.Threading.Tasks.Task SalvarReceita(Receita receita)
        {
            double saldoAntes = SessaoUsuario.UsuarioLogado.saldoDisponivel;

            // Adiciona a transação e atualiza o saldo
            await receita.AdicionarTransacaoAsync();

            // Recarrega usuário atualizado do arquivo
            var usuarioAtualizado = await Usuario.BuscarUsuarioPorIdAsync(SessaoUsuario.UsuarioLogado.Id);
            SessaoUsuario.UsuarioLogado = usuarioAtualizado;

            MessageBox.Show($"Receita adicionada com sucesso!\n" +
                           $"Saldo antes: R$ {saldoAntes:F2}\n" +
                           $"Saldo atual: R$ {usuarioAtualizado.saldoDisponivel:F2}",
                           "Sucesso",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Limpa os campos do formulário
        private void LimparFormulario()
        {
            DescriptionTextBox.Text = "";
            ValueTextBox.Text = "0,00";
            CategoryComboBox.SelectedIndex = -1;
            FixedRadio.IsChecked = true;
            DateTextBox.Text = DateTime.Today.ToString("dd/MM/yyyy");
        }

        // Adiciona a nova receita na lista
        private void AtualizarListaReceitas(Receita receita)
        {
            _receitasCollection.Insert(0, receita);
        }

        // Carrega a lista de receitas do usuário
        private async void CarregarReceitas()
        {
            try
            {
                if (SessaoUsuario.UsuarioLogado != null)
                {
                    var transacoes = await Transacao.CarregarTransacoesPorUsuarioAsync(SessaoUsuario.UsuarioLogado.Id);
                    var receitas = transacoes.Where(t => t.TipoTransacao == "Receita")
                                           .OrderByDescending(r => r.DataTransacao)
                                           .ToList();

                    AtualizarListaReceitasUI(receitas);
                    ConfigurarMensagemListaVazia(receitas.Any());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar receitas: {ex.Message}", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Atualiza a interface com a lista de receitas
        private void AtualizarListaReceitasUI(System.Collections.Generic.List<Transacao> receitas)
        {
            if (_receitasCollection == null)
            {
                _receitasCollection = new ObservableCollection<Transacao>();
                ReceitasItemsControl.ItemsSource = _receitasCollection;
            }

            _receitasCollection.Clear();
            foreach (var receita in receitas)
            {
                _receitasCollection.Add(receita);
            }
        }

        // Configura mensagem quando a lista está vazia
        private void ConfigurarMensagemListaVazia(bool temReceitas)
        {
            var stackPanel = ReceitasItemsControl.Parent as StackPanel;
            if (stackPanel != null)
            {
                var emptyTextBlock = stackPanel.Children
                    .OfType<TextBlock>()
                    .FirstOrDefault(tb => tb.Text.Contains("Nenhuma receita"));

                if (!temReceitas && emptyTextBlock == null)
                {
                    AdicionarMensagemListaVazia(stackPanel);
                }
                else if (temReceitas && emptyTextBlock != null)
                {
                    stackPanel.Children.Remove(emptyTextBlock);
                }
            }
        }

        // Adiciona mensagem de lista vazia
        private void AdicionarMensagemListaVazia(StackPanel stackPanel)
        {
            var emptyText = new TextBlock
            {
                Text = "Nenhuma receita registrada ainda...",
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(148, 163, 184)),
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            stackPanel.Children.Add(emptyText);
        }

        // Remove uma receita
        private async void RemoverReceita_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Transacao transacao)
            {
                try
                {
                    var resultado = MessageBox.Show("Tem certeza que deseja remover esta receita?",
                                                  "Confirmar Remoção",
                                                  MessageBoxButton.YesNo,
                                                  MessageBoxImage.Question);

                    if (resultado == MessageBoxResult.Yes)
                    {
                        await ExcluirReceita(transacao);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao remover receita: {ex.Message}", "Erro",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Exclui uma receita e atualiza o saldo
        private async System.Threading.Tasks.Task ExcluirReceita(Transacao transacao)
        {
            double saldoAntes = SessaoUsuario.UsuarioLogado.saldoDisponivel;

            // Remove a transação e atualiza o saldo
            await transacao.ExcluirTransacaoAsync();

            // Recarrega usuário atualizado do arquivo
            var usuarioAtualizado = await Usuario.BuscarUsuarioPorIdAsync(SessaoUsuario.UsuarioLogado.Id);
            SessaoUsuario.UsuarioLogado = usuarioAtualizado;

            // Recarrega a lista
            CarregarReceitas();

            MessageBox.Show($"Receita removida com sucesso!\n" +
                          $"Saldo antes: R$ {saldoAntes:F2}\n" +
                          $"Saldo atual: R$ {usuarioAtualizado.saldoDisponivel:F2}",
                          "Sucesso",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Mostra/oculta o painel de planejamento futuro
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

        // Salva um planejamento de receita futura
        private void SaveFuturePlan_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCamposPlanejamentoFuturo())
                return;

            MessageBox.Show("Receita futura planejada com sucesso!", "Sucesso",
                          MessageBoxButton.OK, MessageBoxImage.Information);

            LimparCamposPlanejamentoFuturo();
        }

        // Valida os campos do planejamento futuro
        private bool ValidarCamposPlanejamentoFuturo()
        {
            if (string.IsNullOrWhiteSpace(FutureValueTextBox.Text) || FutureValueTextBox.Text == "0,00")
            {
                MessageBox.Show("Por favor, insira um valor válido para o planejamento futuro.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(FutureDateTextBox.Text) || FutureDateTextBox.Text == "dd/MM/aaaa")
            {
                MessageBox.Show("Por favor, insira uma data válida para o planejamento futuro.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // Limpa os campos do planejamento futuro
        private void LimparCamposPlanejamentoFuturo()
        {
            FutureValueTextBox.Text = "0,00";
            FutureDateTextBox.Text = "dd/MM/aaaa";
            FuturePlanningPanel.Visibility = Visibility.Collapsed;
        }

        // Métodos de navegação entre telas

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
                MessageBox.Show($"Erro ao navegar para Finanças: {ex.Message}", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CardsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar tela de Cartões
            MessageBox.Show("Navegar para Cartões", "Navegação",
                          MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show($"Erro ao navegar para Gastos: {ex.Message}", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void IncomeButton_Click(object sender, RoutedEventArgs e)
        {
            // Já está na tela de receitas
            MessageBox.Show("Você já está na tela de Receitas", "Informação",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar tela de Metas
            MessageBox.Show("Navegar para Metas", "Navegação",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}