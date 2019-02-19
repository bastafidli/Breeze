﻿using GameLauncher.ViewModels;
using GameLauncher.Views;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GameLauncher.Properties;
using MaterialDesignThemes.Wpf;
using System.Windows.Data;
using System.Net;

namespace GameLauncher
{
    public partial class MainWindow : Window
    {
        public static AddGames DialogAddGames = new AddGames();
        public static EditGames DialogEditGames = new EditGames();
        public static ExeSelection DialogExeSelection = new ExeSelection();
        private BannerViewModel bannerViewModel;
        private ListViewModel listViewModel;
        private PosterViewModel posterViewModel;
        private SettingsViewModel settingsViewModel;
        private ExesViewModel exesViewModel;
        public Views.PosterView pv = new Views.PosterView();
        public Views.BannerView bv = new Views.BannerView();
        public Views.ListView lv = new Views.ListView();
        public CollectionViewSource cvs;
        public string dialogOpen;
        public string DLGameTitle;
        public string DLImgType;
        public bool isDownloadOpen;

        public MainWindow()
        {
            this.Height = (System.Windows.SystemParameters.PrimaryScreenHeight * 0.75);
            this.Width = (System.Windows.SystemParameters.PrimaryScreenWidth * 0.75);
            LoadAllGames lag = new LoadAllGames();
            LoadSearch ls = new LoadSearch();
            MakeDirectories();
            MakeDefaultGenres();
            lag.LoadGenres();
            InitializeComponent();
            bannerViewModel = new BannerViewModel();
            posterViewModel = new PosterViewModel();
            listViewModel = new ListViewModel();
            exesViewModel = new ExesViewModel();
            posterViewModel.LoadGames();
            posterViewModel.LoadGenres();
            DataContext = posterViewModel;
            isDownloadOpen = false;
            LoadSettings();

        }
        public void MakeDirectories()
        {
            if (!Directory.Exists("./Resources/")) { Directory.CreateDirectory("./Resources/"); }
            if (!Directory.Exists("./Resources/img/")) { Directory.CreateDirectory("./Resources/img/"); }
            if (!Directory.Exists("./Resources/shortcuts/")) { Directory.CreateDirectory("./Resources/shortcuts/"); }
        }

        public void MakeDefaultGenres()
        {
            if (!File.Exists("./Resources/GenreList.txt"))
            {
                TextWriter tsw = new StreamWriter(@"./Resources/GenreList.txt", true);
                Guid gameGuid = Guid.NewGuid();
                tsw.WriteLine("Action|" + Guid.NewGuid());
                tsw.WriteLine("Adventure|" + Guid.NewGuid());
                tsw.WriteLine("Casual|" + Guid.NewGuid());
                tsw.WriteLine("Emulator|" + Guid.NewGuid());
                tsw.WriteLine("Horror|" + Guid.NewGuid());
                tsw.WriteLine("Indie|" + Guid.NewGuid());
                tsw.WriteLine("MMO|" + Guid.NewGuid());
                tsw.WriteLine("Open World|" + Guid.NewGuid());
                tsw.WriteLine("Platform|" + Guid.NewGuid());
                tsw.WriteLine("Racing|" + Guid.NewGuid());
                tsw.WriteLine("Retro|" + Guid.NewGuid());
                tsw.WriteLine("RPG|" + Guid.NewGuid());
                tsw.WriteLine("Simulation|" + Guid.NewGuid());
                tsw.WriteLine("Sport|" + Guid.NewGuid());
                tsw.WriteLine("Strategy|" + Guid.NewGuid());
                tsw.WriteLine("VR|" + Guid.NewGuid());
                tsw.Close();
            }
        }
        private void OpenAddGameWindow_OnClick(object sender, RoutedEventArgs e)
        {
            OpenAddGameDialog();
            RefreshGames();
        }

        public void OpenAddGameDialog()
        {
            DialogFrame.Visibility = Visibility.Visible;
            DialogFrame.Content = DialogAddGames;
            dialogOpen = "add";
            DialogAddGames.AddGameDialog.IsOpen = true;
        }

        public void OpenExeSearchDialog()
        {
            DataContext = exesViewModel;
            exesViewModel.SearchExe();
            DialogFrame.Visibility = Visibility.Visible;
            DialogFrame.Content = DialogExeSelection;
            dialogOpen = "exeSelection";
            DialogExeSelection.ExeSelectionDialog.IsOpen = true;
        }

        public void CloseExeSearchDialog()
        {
            DataContext = settingsViewModel;
            DialogFrame.Visibility = Visibility.Hidden;
            DialogExeSelection.ExeSelectionDialog.IsOpen = false;
        }

        public void OpenEditGameDialog(string guid)
        {
            DialogFrame.Visibility = Visibility.Visible;
            DialogFrame.Content = DialogEditGames;
            dialogOpen = "edit";
            DialogEditGames.currentGuid(guid);
            DialogEditGames.EditGameDialog.IsOpen = true;
        }
        public void OpenImageDL(string gametitle, string searchstring, string imagetype)
        {
            ImageDownload DialogImageDL = new ImageDownload(gametitle, searchstring, imagetype);
            DLGameTitle = gametitle;
            DLImgType = imagetype;
            if (DialogFrame.Content.ToString() == "GameLauncher.EditGames" || DialogFrame.Content.ToString() == "GameLauncher.AddGames") {
            DialogFrame.Visibility = Visibility.Visible;
            DialogFrame.Content = DialogImageDL;
            DialogAddGames.AddGameDialog.IsOpen = false;
            DialogEditGames.EditGameDialog.IsOpen = false;
            DialogImageDL.DownloadDialog.IsOpen = true;
            isDownloadOpen = true;
            }
            else if (DialogFrame.Content.ToString() == "GameLauncher.Views.ImageDownload")
            {
                if (dialogOpen == "edit")
                {
                    DialogFrame.Content = DialogEditGames;
                    DialogEditGames.EditGameDialog.IsOpen = true;
                    DialogImageDL.DownloadDialog.IsOpen = false;
                    isDownloadOpen = false;
                }
                else if (dialogOpen == "add")
                {
                    DialogFrame.Content = DialogAddGames;
                    DialogAddGames.AddGameDialog.IsOpen = true;
                    DialogImageDL.DownloadDialog.IsOpen = false;
                    isDownloadOpen = false;
                }
                else { Console.WriteLine("Not sure which dialog is open, whoops!"); }
            }
        }

        public void DownloadImage(string url)
        {
            if (!File.Exists(@"Resources/img/" + DLGameTitle + "-" + DLImgType + ".png")){
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        client.UseDefaultCredentials = true;
                        client.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                        client.DownloadFile(new Uri(url), @"Resources\img\" + DLGameTitle + "-" + DLImgType + ".png");
                        SetPath(DLGameTitle, DLImgType, dialogOpen);
                    }catch(Exception e) { Console.WriteLine("Error:" + e); MessageBox.Show("Sorry! That's failed, Try again or try another image"); }
                } }
            else if (File.Exists(@"Resources/img/" + DLGameTitle + "-" + DLImgType + ".png")){
                File.Delete(@"Resources/img/" + DLGameTitle + "-" + DLImgType + ".png");
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        client.UseDefaultCredentials = true;
                        client.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                        client.DownloadFile(new Uri(url), @"Resources\img\" + DLGameTitle + "-" + DLImgType + ".png");
                        SetPath(DLGameTitle, DLImgType, dialogOpen);
                    }
                    catch (Exception e) { Console.WriteLine("Error: " + e); }
                }
            }
        }

        public void SetPath(string title, string imagetype, string dialogType)
        {
            string imgpath = AppDomain.CurrentDomain.BaseDirectory + "Resources\\img\\" + DLGameTitle + "-" + DLImgType + ".png";
            if (imagetype == "icon")
            {
                if (dialogType == "edit")
                {
                    DialogEditGames.EditIcon.Text = imgpath;
                    OpenImageDL("closes", "the", "dialog");
                }
                else if (dialogType == "add")
                {
                    DialogAddGames.NewGameIcon.Text = imgpath;
                    OpenImageDL("closes", "the", "dialog");
                }
            }
            else if (imagetype == "poster")
            {
                if (dialogType == "edit")
                {
                    DialogEditGames.EditPoster.Text = imgpath;
                    OpenImageDL("closes", "the", "dialog");
                }
                else if (dialogType == "add")
                {
                    DialogAddGames.NewGamePoster.Text = imgpath;
                    OpenImageDL("closes", "the", "dialog");
                }
            }
            else if (imagetype == "banner")
            {
                if (dialogType == "edit")
                {
                    DialogEditGames.EditBanner.Text = imgpath;
                    OpenImageDL("closes", "the", "dialog");
                }
                else if (dialogType == "add")
                {
                    DialogAddGames.NewGameBanner.Text = imgpath;
                    OpenImageDL("closes", "the", "dialog");
                }
            }
        }

        //Apply the selected genre filter
        private void ApplyGenreFilter_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext == settingsViewModel)
                DataContext = posterViewModel;
            string genreToFilter = ((Button)sender).Tag.ToString();
            pv.GenreToFilter(genreToFilter);
            pv.RefreshList2(cvs);
            bv.GenreToFilter(genreToFilter);
            bv.RefreshList2(cvs);
            lv.GenreToFilter(genreToFilter);
            lv.RefreshList2(cvs);
            MenuToggleButton.IsChecked = false; //hide hamburger
        }

        //Poster button
        private void PosterButton_OnClick(object sender, RoutedEventArgs e)
        {
            PosterViewActive();
        }

        public void PosterViewActive()
        {
            posterViewModel = new PosterViewModel();
            posterViewModel.LoadGames();
            posterViewModel.LoadGenres();
            DataContext = posterViewModel;
            Properties.Settings.Default.viewtype = "Poster";
            Properties.Settings.Default.Save();
        }

        //Banner button
        private void BannerButton_OnClick(object sender, RoutedEventArgs e)
        {
            BannerViewActive();
        }

        public void BannerViewActive()
        {
            bannerViewModel = new BannerViewModel();
            bannerViewModel.LoadGames();
            bannerViewModel.LoadGenres();
            DataContext = bannerViewModel;
            Properties.Settings.Default.viewtype = "Banner";
            Properties.Settings.Default.Save();
        }

        //List button
        private void ListButton_OnClick(object sender, RoutedEventArgs e)
        {
            ListViewActive();
        }

        public void ListViewActive()
        {
            listViewModel = new ListViewModel();
            listViewModel.LoadGames();
            listViewModel.LoadGenres();
            DataContext = listViewModel;
            Properties.Settings.Default.viewtype = "List";
            Properties.Settings.Default.Save();
        }

        //Settings button
        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsViewActive();
        }

        private void SettingsViewActive()
        {
            settingsViewModel = new SettingsViewModel();
            settingsViewModel.LoadGenres();
            DataContext = settingsViewModel;
        }

        //Refresh button
        private void RefreshGames_OnClick(object sender, RoutedEventArgs e)
        {
            //RefreshGames();
            if (DataContext == posterViewModel) { posterViewModel.LoadList(); }
            if (DataContext == bannerViewModel) { bannerViewModel.LoadList(); }
            if (DataContext == listViewModel) { listViewModel.LoadList(); }
            RefreshGames();
        }

        public void RefreshGames()
        {
            if (DataContext == listViewModel) { ListViewActive(); }
            else if (DataContext == posterViewModel) { PosterViewActive(); }
            else if (DataContext == bannerViewModel) { BannerViewActive(); }
            else if (DataContext == settingsViewModel) { SettingsViewActive(); }
        }

        private void MWSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (isDownloadOpen == true)
            {
                ImageDownload.ChangeWindowSize(this.ActualWidth * 0.8, this.ActualHeight * 0.8);
            }
        }

        //Load settings
        public void LoadSettings()
        {
            //Theme Light or Dark
            if (Settings.Default.theme.ToString() == "Dark")
            {
                ThemeAssist.SetTheme(Application.Current.MainWindow, BaseTheme.Dark);
            }
            else if (Settings.Default.theme.ToString() == "Light")
            {
                ThemeAssist.SetTheme(Application.Current.MainWindow, BaseTheme.Light);
            }
            if (Settings.Default.primary.ToString() != "")
            {
                new PaletteHelper().ReplacePrimaryColor(Settings.Default.primary.ToString());
            }
            if (Settings.Default.accent.ToString() != "")
            {
                new PaletteHelper().ReplaceAccentColor(Settings.Default.accent.ToString());
            }
            if (Settings.Default.viewtype.ToString() == "Poster")
            {
                PosterViewActive();
            }
            if (Settings.Default.viewtype.ToString() == "Banner")
            {
                BannerViewActive();
            }
            if (Settings.Default.viewtype.ToString() == "List")
            {
                ListViewActive();
            }
        }
    }
}