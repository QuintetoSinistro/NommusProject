using NommusProject.Data;
using NommusProject.Utils;
using NommusProject.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace NommusProject
{
    // Tela de gerenciamento de receitas (Income)
    public partial class IncomeWindow : Window
    {
        // Controla se o popup do usuário está visível
        private bool _popupAberto = false;

        // Repositório de transações para acesso ao banco de dados
        private TransacaoRepository _transacaoRepo = new TransacaoRepository();

        // Lista em memória das receitas carregadas (usada para exibição e exclusão)
        private List<Transacao> _receitas = new List<Transacao>();

        // Construtor: inicializa os componentes XAML, carrega dados do usuário e a lista de receitas
        public IncomeWindow()
        {
            InitializeComponent();
            Utils.MaskHelper.AplicarMascaraValor(ValueTextBox);
            CarregarDadosUsuario();
            CarregarReceitas();
        }

        // ============================================================
        // DADOS DO USUÁRIO (sidebar e popup)
        // ============================================================

        // Carrega nome e email do usuário logado nos controles da tela
        private void CarregarDadosUsuario()
        {
            var usuario = SessaoUsuario.UsuarioLogado;
            if (usuario == null) return;
            UsuarioNomeText.Text = usuario.Nome;
            PopupNomeText.Text = usuario.Nome;
            PopupEmailText.Text = usuario.Email;

            CarregarFotoSidebar(usuario.FotoPerfil);
        }

        private void CarregarFotoSidebar(string caminhoFoto)
        {
            try
            {
                if (!string.IsNullOrEmpty(caminhoFoto) && System.IO.File.Exists(caminhoFoto))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(caminhoFoto, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    SidebarFotoBrush.ImageSource = bitmap;
                }
                else
                {
                    var defaultUri = new Uri("pack://application:,,,/Views/Images/user.png", UriKind.Absolute);
                    SidebarFotoBrush.ImageSource = new BitmapImage(defaultUri);
                }
            }
            catch { SidebarFotoBrush.ImageSource = null; }
        }

        // ============================================================
        // CARREGAMENTO DA LISTA DE RECEITAS
        // ============================================================

        // Busca todas as receitas do usuário no banco e as exibe no ItemsControl
        private void CarregarReceitas()
        {
            int usuarioId = SessaoUsuario.UsuarioLogado?.Id ?? 0;
            _receitas = _transacaoRepo.GetByUsuarioAndTipo(usuarioId, "Receita");
            ReceitasItemsControl.ItemsSource = _receitas;
        }

        // ============================================================
        // ADIÇÃO DE NOVA RECEITA
        // ============================================================

        // Evento do botão "Adicionar Receita": valida os campos, cria um objeto Transacao
        // e salva no banco de dados.
        private void AddIncome_Click(object sender, RoutedEventArgs e)
        {
            // Validação da descrição
            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Informe a descrição da receita.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validação do valor (deve ser número positivo, formato "100,50")
            if (!double.TryParse(ValueTextBox.Text, out double valor) || valor <= 0)
            {
                MessageBox.Show("Valor inválido. Use formato como 100,50", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validação da data (se inválida, usa a data atual)
            if (!DateTime.TryParse(DateTextBox.Text, out DateTime data))
                data = DateTime.Today;

            // Obtém o ID da categoria selecionada (Tag do ComboBoxItem)
            // O Tag deve conter o IdCategoria vindo do banco. Se não for um número >0, mantém nulo (NULL no banco)
            int? categoriaId = null;
            if (CategoryComboBox.SelectedItem is ComboBoxItem catItem && catItem.Tag is int id && id > 0)
            {
                categoriaId = id;
            }

            // Define o tipo de receita com base nos RadioButtons
            string tipoReceita = FixedRadio.IsChecked == true ? "Fixa" : (VariableRadio.IsChecked == true ? "Variável" : "Extra");

            // Cria o objeto Transacao com os dados do formulário
            var receita = new Transacao
            {
                DescricaoTransacao = DescriptionTextBox.Text,
                TipoTransacao = "Receita",            // Define como Receita
                ValorTransacao = valor,
                DataTransacao = data,
                CategoriaId = categoriaId,            // Pode ser null
                FormaPagamento = "Depósito",          // Valor padrão para receitas
                CondicaoPagamento = tipoReceita,      // "Fixa", "Variável" ou "Extra"
                UsuarioId = SessaoUsuario.UsuarioLogado.Id
            };

            // Salva no banco via repositório
            _transacaoRepo.Add(receita);

            // Recarrega a lista de receitas para exibir a nova
            CarregarReceitas();

            // Limpa o formulário para a próxima entrada
            LimparFormulario();

            MessageBox.Show("Receita adicionada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ============================================================
        // EXCLUSÃO DE RECEITA
        // ============================================================

        // Evento disparado pelo botão "✕" em cada item da lista.
        // O Tag do botão contém o IdTransacao a ser removido.
        private void ExportarRelatorio_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "CSV files (*.csv)|*.csv";
            dialog.FileName = $"Receitas_{DateTime.Now:yyyyMMddHHmmss}.csv";
            if (dialog.ShowDialog() == true)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Descrição;Valor;Data;Categoria");
                foreach (var r in _receitas)
                    sb.AppendLine($"{r.DescricaoTransacao};{r.ValorTransacao:F2};{r.DataTransacao:dd/MM/yyyy};{r.CategoriaId}");
                System.IO.File.WriteAllText(dialog.FileName, sb.ToString());
                MessageBox.Show("Relatório exportado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // O método RemoverReceita_Click já deve existir; certifique-se de que está correto:
        private void RemoverReceita_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                if (MessageBox.Show("Deseja remover esta receita?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _transacaoRepo.Delete(id);
                    CarregarReceitas();
                }
            }
        }

        // ============================================================
        // PLANEJAMENTO DE RECEITAS FUTURAS (simulado)
        // ============================================================

        // Salva um plano de receita futura (apenas simulação – não persiste no banco)
        private void SaveFuturePlan_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(FutureValueTextBox.Text, out double valor) || valor <= 0)
            {
                MessageBox.Show("Informe um valor válido para planejamento futuro.", "Atenção",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Exibe mensagem informativa (aqui você poderia salvar em uma tabela de planejamentos)
            MessageBox.Show($"Planejamento de receita futura no valor de R$ {valor:F2} registrado (simulação).",
                            "Planejamento", MessageBoxButton.OK, MessageBoxImage.Information);

            // Limpa o campo e esconde o painel
            FutureValueTextBox.Text = "0,00";
            FuturePlanningPanel.Visibility = Visibility.Collapsed;
        }

        // Mostra ou esconde o painel de planejamento futuro
        private void PlanFutureIncome_Click(object sender, RoutedEventArgs e)
        {
            bool visivel = (FuturePlanningPanel.Visibility == Visibility.Visible);
            FuturePlanningPanel.Visibility = visivel ? Visibility.Collapsed : Visibility.Visible;
            if (!visivel)
                FutureValueTextBox.Text = "0,00";   // Reseta o campo ao abrir
        }

        // ============================================================
        // UTILITÁRIOS
        // ============================================================

        // Limpa os campos do formulário de adição de receita
        private void LimparFormulario()
        {
            DescriptionTextBox.Text = "";
            ValueTextBox.Text = "0,00";
            DateTextBox.Text = DateTime.Today.ToString("dd/MM/yyyy");
            CategoryComboBox.SelectedIndex = 0;      // Seleciona o primeiro item (ex: "Outros")
            FixedRadio.IsChecked = true;             // Marca "Fixa" como padrão
        }

        // ============================================================
        // POPUP DO USUÁRIO (menu de perfil)
        // ============================================================

        // Alterna a visibilidade do popup e do overlay ao clicar no card do usuário na sidebar
        private void UsuarioCard_Click(object sender, MouseButtonEventArgs e)
        {
            _popupAberto = !_popupAberto;
            UserPopupCard.Visibility = _popupAberto ? Visibility.Visible : Visibility.Collapsed;
            PopupOverlay.Visibility = _popupAberto ? Visibility.Visible : Visibility.Collapsed;
            e.Handled = true;
        }

        // Fecha o popup quando o overlay (fundo escuro) é clicado
        private void FecharPopup_Click(object sender, MouseButtonEventArgs e)
        {
            _popupAberto = false;
            UserPopupCard.Visibility = Visibility.Collapsed;
            PopupOverlay.Visibility = Visibility.Collapsed;
        }

        // Abre a tela de configurações e fecha a atual
        private void PopupConfiguracoes_Click(object sender, RoutedEventArgs e)
        {
            new ConfiguracoesWindow().Show();
            this.Close();
        }

        // Faz logout, limpa a sessão e volta para a tela de login
        private void PopupLogout_Click(object sender, RoutedEventArgs e)
        {
            SessaoUsuario.Logout();
            new UserLogin().Show();
            this.Close();
        }

        // ============================================================
        // NAVEGAÇÃO PELOS BOTÕES DA SIDEBAR
        // ============================================================

        private void FinanceButton_Click(object sender, RoutedEventArgs e) { new MainWindow().Show(); this.Close(); }
        private void CardsButton_Click(object sender, RoutedEventArgs e) { new cartoes().Show(); this.Close(); }
        private void ExpensesButton_Click(object sender, RoutedEventArgs e) { new ExpensesWindow().Show(); this.Close(); }
        private void IncomeButton_Click(object sender, RoutedEventArgs e) { /* já está na tela de receitas */ }
        private void GoalsButton_Click(object sender, RoutedEventArgs e) { new MetasWindow().Show(); this.Close(); }
        private void BackToDashboard_Click(object sender, RoutedEventArgs e) { new MainWindow().Show(); this.Close(); }
    }
}