﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for InstallFinished.xaml
    /// </summary>
    public partial class InstallFinished : RelhaxWindow
    {
        public InstallFinished()
        {
            InitializeComponent();
        }

        private void InstallationCompleteStartWoTButton_Click(object sender, RoutedEventArgs e)
        {
            if(!Utils.StartProcess(new ProcessStartInfo()
            {
                WorkingDirectory = Settings.WoTDirectory,
                FileName = Path.Combine(Settings.WoTDirectory, "WorldOfTanks.exe")
            }))
            {
                MessageBox.Show(Translations.GetTranslatedString("CouldNotStartProcess"));
            }
            DialogResult = true;
            Close();
        }

        private void InstallationCompleteStartGameCenterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Utils.StartProcess(new ProcessStartInfo()
            {
                WorkingDirectory = Settings.WoTDirectory,
                FileName = Path.Combine(Settings.WoTDirectory, "WoTLauncher.exe")
            }))
            {
                MessageBox.Show(Translations.GetTranslatedString("CouldNotStartProcess"));
            }
            DialogResult = true;
            Close();
        }

        private void InstallationCompleteOpenXVMButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Utils.StartProcess(string.Format("https://www.modxvm.com/{0}/", Translations.GetTranslatedString("xvmUrlLocalisation"))))
            {
                MessageBox.Show(Translations.GetTranslatedString("CouldNotStartProcess"));
            }
            DialogResult = true;
            Close();
        }

        private void InstallationCompleteCloseAppButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Application.Current.Shutdown();
        }
    }
}