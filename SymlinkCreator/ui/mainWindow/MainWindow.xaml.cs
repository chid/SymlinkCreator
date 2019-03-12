﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using SymlinkCreator.core;
using SymlinkCreator.ui.utility;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SymlinkCreator.ui.mainWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region constructor

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        #endregion


        #region window event handles

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = new MainWindowViewModel();
        }

        protected override void OnSourceInitialized(EventArgs eventArgs)
        {
            WindowMaximizeButton.DisableMaximizeButton(this);
            this.CreateSymlinksButtonImage.Source = NativeAdminShieldIcon.GetNativeShieldIcon();
            base.OnSourceInitialized(eventArgs);
        }

        #endregion


        #region control event handles

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            bool? result = fileDialog.ShowDialog();

            if (result == true)
            {
                AddToSourceFileList(fileDialog.FileNames);
            }
        }

        private void DestinationPathBrowseButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel mainWindowViewModel = this.DataContext as MainWindowViewModel;
            if (mainWindowViewModel == null) return;

            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    mainWindowViewModel.DestinationPath = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void DeleteSelectedButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel mainWindowViewModel = this.DataContext as MainWindowViewModel;
            if (mainWindowViewModel == null) return;

            List<string> selectedFileList = SourceFileListView.SelectedItems.Cast<string>().ToList();
            foreach (var selectedItem in selectedFileList)
            {
                mainWindowViewModel.FileList.Remove(selectedItem);
            }
        }

        private void ClearListButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel mainWindowViewModel = this.DataContext as MainWindowViewModel;

            mainWindowViewModel?.FileList.Clear();
        }

        private void SourceFileListView_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFileList = (string[]) e.Data.GetData(DataFormats.FileDrop);
                AddToSourceFileList(droppedFileList);
            }
        }

        private void DestinationPathTextBox_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] pathList = (string[]) e.Data.GetData(DataFormats.FileDrop);
                if (pathList != null)
                {
                    string droppedDestinationPath = pathList[0];
                    AssignDestinationPath(droppedDestinationPath);
                }
            }
        }

        private void DestinationPathTextBox_OnPreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void CreateSymlinksButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel mainWindowViewModel = this.DataContext as MainWindowViewModel;
            if (mainWindowViewModel == null) return;

            SymlinkAgent symlinkAgent = new SymlinkAgent(
                mainWindowViewModel.FileList,
                mainWindowViewModel.DestinationPath,
                mainWindowViewModel.ShouldUseRelativePath,
                mainWindowViewModel.ShouldRetainScriptFile);

            symlinkAgent.CreateSymlinks();

            MessageBox.Show("Execution completed.", "Done!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AboutButton_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                $"{ApplicationConfiguration.ApplicationName} v{ApplicationConfiguration.ApplicationVersion}\n" +
                "Developed by Arnob Paul. Thank you for using this application! :)\n\n" +
                $"Do you want to visit the developer's website?\n{ApplicationConfiguration.CompanyWebAddress}",
                "About", MessageBoxButton.YesNo,
                MessageBoxImage.Asterisk);

            if (result == MessageBoxResult.Yes)
                Process.Start(ApplicationConfiguration.CompanyWebAddress);
        }

        #endregion


        #region helper methods

        private void AddToSourceFileList(IEnumerable<string> fileList)
        {
            MainWindowViewModel mainWindowViewModel = this.DataContext as MainWindowViewModel;
            if (mainWindowViewModel == null) return;

            foreach (string file in fileList)
            {
                if (!mainWindowViewModel.FileList.Contains(file))
                {
                    if (File.Exists(file))
                        mainWindowViewModel.FileList.Add(file);
                }
            }
        }

        private void AssignDestinationPath(string destinationPath)
        {
            MainWindowViewModel mainWindowViewModel = this.DataContext as MainWindowViewModel;
            if (mainWindowViewModel == null) return;

            if (Directory.Exists(destinationPath))
                mainWindowViewModel.DestinationPath = destinationPath;
        }

        #endregion
    }
}