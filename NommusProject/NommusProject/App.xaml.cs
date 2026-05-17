using Microsoft.Data.Sqlite;
using NommusProject.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
namespace NommusProject;

using NommusProject.Utils;
using System.Threading.Tasks;
using System.Windows.Threading;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private System.Windows.Threading.DispatcherTimer _backupTimer;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Captura exceções não tratadas na UI thread
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        // Captura exceções em threads secundárias
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        // Captura exceções em tarefas assíncronas
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        DatabaseInitializer.Initialize();
        // --- BACKUP AO INICIAR ---
        DatabaseBackup.CriarBackup();

        // Configura backup diário (24 horas = 86400000 ms)
        _backupTimer = new System.Windows.Threading.DispatcherTimer();
        _backupTimer.Interval = TimeSpan.FromHours(24);
        _backupTimer.Tick += (s, args) => DatabaseBackup.CriarBackup();
        _backupTimer.Start();
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Erro inesperado: {e.Exception.Message}\n\nDetalhes: {e.Exception}",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true; // Impede o fechamento do aplicativo
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        MessageBox.Show($"Erro grave: {ex?.Message}\n\n{ex?.StackTrace}",
                        "Erro Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        MessageBox.Show($"Erro em tarefa assíncrona: {e.Exception.Message}",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        e.SetObserved();
    }
}