using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO.Packaging;
using Microsoft.Win32;

namespace WpfApp2 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
        }

        //   Variables section

        public bool isModified = false;
        public static string settingsFilePath =
            Environment.GetEnvironmentVariable("LOCALAPPDATA") + 
            @"\Packages\" +
            @"Microsoft.WindowsTerminal_8wekyb3d8bbwe" + 
            @"\LocalState\profiles.json";
        public static string applicationName = "Windows Terminal Settings";
        

        // internal functions

        private bool DiscardChanges() {
            string message = "Settings content was modified. Are you sure you want to abondon changes?";
            var response = MessageBox.Show(message, "Settings modified", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return response == MessageBoxResult.Yes;
        }

        private bool ValidateJSON(string str) {
            try {
                var obj = JsonSerializer.Deserialize<Object>(str);
                return true;
            } catch {
                return false;
            }
        }

        private void ChangeStatuses(string statusBarText, bool buttonStatus) {
            // update status bar text
            FileSettingsStatus.Text = statusBarText;
            
            // update buttons statuses and control variable
            LoadButton.IsEnabled = buttonStatus;
            SaveButton.IsEnabled = buttonStatus;
            isModified = buttonStatus;
        }

        // GUI events handling

        private void LoadButton_Click(object sender, RoutedEventArgs e) {
            if (isModified && DiscardChanges()) {
                isModified = false;
            }

            if (!isModified) {
                TextBox.Text = System.IO.File.ReadAllText(settingsFilePath);
                ChangeStatuses("Settings file loaded.", false);
                TextBox.IsEnabled = true;
            }

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            ChangeStatuses("Modifying Settings file.", true);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            if (!ValidateJSON(TextBox.Text)) {
                MessageBox.Show("Not a valid JSON.", applicationName, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // create a backup file
            string TimeStamp = DateTime.Now.ToString(@"ddMMMyyyy.HHmmss");
            string NewName = System.IO.Path.ChangeExtension(settingsFilePath, TimeStamp + ".json");
            File.Copy(settingsFilePath, NewName);
            // save new settings
            File.WriteAllText(settingsFilePath, TextBox.Text);
            // set status
            MessageBox.Show("File saved! Backup made.", applicationName, MessageBoxButton.OK,MessageBoxImage.Information);
            ChangeStatuses("Settings file saved.", false);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e) {
            if (!isModified || DiscardChanges()) {
                Application.Current.Shutdown();
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            FileNameStatus.Text = settingsFilePath;

            if (!(File.Exists(settingsFilePath))) {
                var message = "Settings file " + settingsFilePath + " does not exist. Click Yes to Exit or No to select a file";
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "JSON files|*.json";
                var response = MessageBox.Show(message, "Missing config file", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (response == MessageBoxResult.Yes || openFileDialog.ShowDialog() == false) {
                    Application.Current.Shutdown();
                } else {
                    settingsFilePath = openFileDialog.FileName;
                }                
            }
        }
    }
}
