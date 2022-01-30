using System;
using System.Net;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Flow.Launcher.Plugin.Kitty.ViewModels;
using Flow.Launcher.Plugin.Kitty.Helper;
using Microsoft.Win32;
namespace Flow.Launcher.Plugin.Kitty
{
    /// <summary>
    /// Interaktionslogik für KittySettings.xaml
    /// </summary>
    public partial class KittySettings : UserControl
    {
        public PluginInitContext context { get; set; }
        public Settings settings { get; }
        private SettingsViewModel vm;
        public string KittyDownloadPath = "";

        public KittySettings(PluginInitContext context, Settings settings, SettingsViewModel vm)
        {
            this.context = context;
            this.settings = settings;
            this.vm = vm;
            DataContext = vm;

            InitializeComponent();
        }

        private void SettingsOpenKittyPath_Click(object sender, RoutedEventArgs e)
        {
            string trExeFile = context.API.GetTranslation("flowlauncher_plugin_kitty_openDialogExe");
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = trExeFile + " (*.exe)| *.exe";
            if (!string.IsNullOrEmpty(settings.KittyExePath))
                openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(settings.KittyExePath);

            if (openFileDialog.ShowDialog() == true)
            {
                settings.KittyExePath = openFileDialog.FileName;
            }

            SettingsKittyExecutablePath.Text = settings.KittyExePath;
        }

        private void KittySettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            SettingsAddKittyExeToResults.IsChecked = settings.AddKittyExeToResults;
            SettingsAddKittyExeToResults.Checked += (o, e) =>
            {
                settings.AddKittyExeToResults = true;
            };
            SettingsAddKittyExeToResults.Unchecked += (o, e) =>
            {
                settings.AddKittyExeToResults = false;
            };

            SettingsIsKittyPortable.IsChecked = settings.IsKittyPortable;
            SettingsIsKittyPortable.Checked += (o, e) =>
            {
                settings.IsKittyPortable = true;
            };
            SettingsIsKittyPortable.Unchecked += (o, e) =>
            {
                settings.IsKittyPortable = false;
            };

            SettingsOpenKittySessionFullscreen.IsChecked = settings.OpenKittySessionFullscreen;
            SettingsOpenKittySessionFullscreen.Checked += (o, e) =>
            {
                settings.OpenKittySessionFullscreen = true;
            };
            SettingsOpenKittySessionFullscreen.Unchecked += (o, e) =>
            {
                settings.OpenKittySessionFullscreen = false;
            };

            SettingsPuttyInsteadOfKitty.IsChecked = settings.PuttyInsteadOfKitty;
            SettingsPuttyInsteadOfKitty.Checked += (o, e) =>
            {
                settings.PuttyInsteadOfKitty = true;
            };
            SettingsPuttyInsteadOfKitty.Unchecked += (o, e) =>
            {
                settings.PuttyInsteadOfKitty = false;
            };

            SettingsKittyExecutablePath.Text = settings.KittyExePath;
        }

        /// <summary>
        /// Handle Kitty download complete event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // now let's set the default values for downloaded portable kitty
            settings.KittyExePath = KittyDownloadPath;
            SettingsKittyExecutablePath.Text = KittyDownloadPath;
            settings.IsKittyPortable = true;
            settings.PuttyInsteadOfKitty = false;
        }

        /// <summary>
        /// Download portable Kitty to plugin directory and set as Kitty instance
        /// </summary>
        private void DownloadKitty_Click(object sender, RoutedEventArgs e)
        {
            // Save to the plugin settings folder.
            string PluginSettingsDir = Paths.ReturnPluginSettingsFolder(context);
            KittyDownloadPath = Path.Combine(PluginSettingsDir, "kitty_portable.exe");

            // download async to not block the main thread and listen to the completed event
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompleted);

                webClient.DownloadFileAsync(new Uri("https://www.9bis.net/kitty/files/kitty_portable.exe"), KittyDownloadPath);
            }
            catch (Exception ex)
            {
                string trError = context.API.GetTranslation("flowlauncher_plugin_kitty_error");
                string trDown = context.API.GetTranslation("flowlauncher_plugin_kitty_download");
                context.API.ShowMsg(trDown + " " + trError +": ", ex.Message, "");
            }
        }
    }
}