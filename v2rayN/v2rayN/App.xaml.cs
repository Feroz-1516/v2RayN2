﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using v2rayN.Handler;
using v2rayN.Models;

namespace v2rayN
{

    public partial class App : Application
    {
        public static EventWaitHandle ProgramStarted;
        private static Config _config;

        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var exePathKey = Utils.GetMD5(Utils.GetExePath());

            var rebootas = (e.Args ?? new string[] { }).Any(t => t == Global.RebootAs);
            ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, exePathKey, out bool bCreatedNew);
            if (!rebootas && !bCreatedNew)
            {
                ProgramStarted.Set();
                Environment.Exit(0);
                return;
            }

            Logging.Setup();
            Init();
            Logging.LoggingEnabled(_config.guiItem.enableLog);
            Logging.SaveLog($"v2rayN start up | {Utils.GetVersion()} | {Utils.GetExePath()}");
            Logging.SaveLog($"{Environment.OSVersion} - {(Environment.Is64BitOperatingSystem ? 64 : 32)}");
            Logging.ClearLogs();

            Thread.CurrentThread.CurrentUICulture = new(_config.uiItem.currentLanguage);

            base.OnStartup(e);
        }

        private void Init()
        {
            if (ConfigHandler.LoadConfig(ref _config) != 0)
            {
                UI.Show($"Loading GUI configuration file is abnormal,please restart the application{Environment.NewLine}加载GUI配置文件异常,请重启应用");
                Environment.Exit(0);
                return;
            }

            if (Environment.OSVersion.Version.Major < 10)
            {
                Environment.SetEnvironmentVariable("DOTNET_EnableWriteXorExecute", "0", EnvironmentVariableTarget.User);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logging.SaveLog("App_DispatcherUnhandledException", e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject != null)
            {
                Logging.SaveLog("CurrentDomain_UnhandledException", (Exception)e.ExceptionObject!);
            }
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Logging.SaveLog("TaskScheduler_UnobservedTaskException", e.Exception);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logging.SaveLog("OnExit");
            base.OnExit(e);
            Process.GetCurrentProcess().Kill();
        }
    }
}