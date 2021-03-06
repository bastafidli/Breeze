﻿using GameLauncher.Models;
using GameLauncher.Properties;
using GameLauncher.ViewModels;
using GameLauncher.Views;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GameLauncher
{
    public partial class MainWindow : Window
    {
        public bool isDownloadOpen = false;
        public bool isExeSearchOpen = false;
        public static AddGames DialogAddGames = new AddGames();
        public static EditGames DialogEditGames = new EditGames();
        public static ExeSelection DialogExeSelection = new ExeSelection();
        public static ObservableCollection<GameList> GameListMW { get; set; }
        public static ObservableCollection<GenreList> GenreListMW { get; set; }
        public LoadAllGames lag = new LoadAllGames();
        private BannerViewModel bannerViewModel = new BannerViewModel();
        private ListViewModel listViewModel = new ListViewModel();
        private PosterViewModel posterViewModel = new PosterViewModel();
        private SettingsViewModel settingsViewModel = new SettingsViewModel();
        private ExesViewModel exesViewModel = new ExesViewModel();
        private Loading loadingdialog = new Loading();
        public Loading loadingprogressdialog = new Loading();
        public PosterView pv = new PosterView();
        public BannerView bv = new BannerView();
        public Views.ListView lv = new Views.ListView();
        public CollectionViewSource cvs;
        public bool isDialogOpen;
        public bool isLoading;
        public string dialogOpen;
        public string DLGameTitle;
        public string DLImgType;
        public string view;
        public string newText;
        public BackgroundWorker lagbw;

        public MainWindow()
        {
            lagbw = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            lagbw.ProgressChanged += LagBWProgressChanged;
            lagbw.DoWork += LagBWDoWork;
            lagbw.RunWorkerCompleted += LagBWRunWorkerCompleted;
            Trace.Listeners.Clear();
            FixFilePaths();
            InitTraceListen();
            this.Height = (SystemParameters.PrimaryScreenHeight * 0.75);
            this.Width = (SystemParameters.PrimaryScreenWidth * 0.75);
            LoadAllGames lag = new LoadAllGames();
            LoadSearch ls = new LoadSearch();
            MakeDirectories();
            MakeDefaultGenres();
            lag.LoadGenres();
            InitializeComponent();
            LoadAllViews();
            DataContext = null;
            isDownloadOpen = false;
            LoadSettings();
            Trace.WriteLine(DateTime.Now + ": New Session started");
        }
        public void LagBWDoWork(object sender, DoWorkEventArgs e)
        {
            lag.LoadGenres();
            lag.LoadGames();
        }
        public void LagBWProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine(e);
        }
        public void LagBWRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            GameListMW = lag.Games;
            GenreListMW = lag.Genres;
            posterViewModel.LoadGames();
            listViewModel.LoadGames();
            bannerViewModel.LoadGames();
            if (Settings.Default.viewtype.ToString() == "Poster") { PosterViewActive(); }
            if (Settings.Default.viewtype.ToString() == "Banner") { BannerViewActive(); }
            if (Settings.Default.viewtype.ToString() == "List") { ListViewActive(); }
            DialogFrame.Visibility = Visibility.Hidden;
        }
        public void MakeDirectories()
        {
            if (!Directory.Exists("./Resources/")) { Directory.CreateDirectory("./Resources/"); }
            if (!Directory.Exists("./Resources/img/")) { Directory.CreateDirectory("./Resources/img/"); }
            if (!Directory.Exists("./Resources/shortcuts/")) { Directory.CreateDirectory("./Resources/shortcuts/"); }
        }
        public void LoadAllViews()
        {
            DialogFrame.Visibility = Visibility.Visible;
            DialogFrame.Content = loadingdialog;
            try
            {
                try { GenreListMW.Clear(); } catch { }
                try { GameListMW.Clear(); } catch { }
                lagbw.RunWorkerAsync();
            }
            catch { }
        }
        public void OpenLoadingProgressDialog()
        {
            DialogFrame.Visibility = Visibility.Visible;
            DialogFrame.Content = loadingprogressdialog;
        }
        public void CloseLoadingProgressDialog()
        {
            DialogFrame.Visibility = Visibility.Hidden;
            DialogFrame.Content = null;
        }
        public void RefreshDataContext()
        {
            if (view == "poster")
            {
                DataContext = posterViewModel;
            }
            else if (view == "list")
            {
                DataContext = listViewModel;
            }
            else if (view == "banner")
            {
                DataContext = bannerViewModel;
            }
            if (view == "settings")
            {
                DataContext = settingsViewModel;
            }
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
        public void InitTraceListen()
        {
            string appdir = AppDomain.CurrentDomain.BaseDirectory;
            string logfile = appdir + "\\log\\logfile.log";
            if (!Directory.Exists(appdir + "\\log"))
            {
                Directory.CreateDirectory(appdir + "\\log");
            }
            if (File.Exists(logfile))
            {
                Directory.Delete(appdir + "\\log", true);
                Directory.CreateDirectory(appdir + "\\log");
                var log = File.Create(logfile);
                log.Close();
            }
            else { var log = File.Create(logfile); }
            TextWriterTraceListener twtl = new TextWriterTraceListener(logfile)
            {
                TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime
            };
            ConsoleTraceListener ctl = new ConsoleTraceListener(false)
            {
                TraceOutputOptions = TraceOptions.DateTime
            };

            Trace.Listeners.Add(twtl);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;
        }
        public void OpenAddGameWindow_OnClick(object sender, RoutedEventArgs e)
        {
            OpenAddGameDialog();
        }
        public void OpenAddGameDialog()
        {
            DialogFrame.Visibility = Visibility.Visible;
            DialogFrame.Content = DialogAddGames;
            dialogOpen = "add";
            isDialogOpen = true;
            DialogAddGames.AddGameDialog.IsOpen = true;
        }
        public void UpdateObsCol(string title, string exe)
        {
            exesViewModel.UpdateObsCol(title, exe);
        }
        public bool CheckBinding(string title)
        {
            bool result = exesViewModel.CheckBinding(title);
            if (result == true) { return true; }
            else { return false; }
        }
        public void OpenExeSearchDialog()
        {
            DataContext = exesViewModel;
            exesViewModel.SearchExe();
            DialogFrame.Visibility = Visibility.Visible;
            DialogFrame.Content = DialogExeSelection;
            isDialogOpen = true;
            dialogOpen = "exeSelection";
            DialogExeSelection.ExeSelectionDialog.IsOpen = true;
            isExeSearchOpen = true;
        }
        public void CloseExeSearchDialog()
        {
            DataContext = settingsViewModel;
            DialogFrame.Visibility = Visibility.Hidden;
            isDialogOpen = false;
            DialogExeSelection.ExeSelectionDialog.IsOpen = false;
            isExeSearchOpen = false;
        }
        public void OpenEditGameDialog(string guid)
        {
            DialogFrame.Visibility = Visibility.Visible;
            DialogFrame.Content = DialogEditGames;
            dialogOpen = "edit";
            isDialogOpen = true;
            DialogEditGames.currentGuid(guid);
            DialogEditGames.EditGameDialog.IsOpen = true;
        }
        public void OpenImageDL(string gametitle, string searchstring, string imagetype)
        {
            ImageDownload DialogImageDL = new ImageDownload(gametitle, searchstring, imagetype);
            DLGameTitle = gametitle;
            DLImgType = imagetype;
            if (DialogFrame.Content.ToString() == "GameLauncher.EditGames" || DialogFrame.Content.ToString() == "GameLauncher.AddGames")
            {
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
                    GC.Collect();
                }
                else if (dialogOpen == "add")
                {
                    DialogFrame.Content = DialogAddGames;
                    DialogAddGames.AddGameDialog.IsOpen = true;
                    DialogImageDL.DownloadDialog.IsOpen = false;
                    isDownloadOpen = false;
                    GC.Collect();
                }
                else { Trace.WriteLine(DateTime.Now + ": -System unsure which dialog currently open"); }
            }
        }
        public void DownloadImage(string url)
        {
            if (!File.Exists(@"Resources/img/" + DLGameTitle + "-" + DLImgType + ".png"))
            {
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        client.UseDefaultCredentials = true;
                        client.Proxy.Credentials = CredentialCache.DefaultCredentials;
                        client.DownloadFile(new Uri(url), @"Resources\img\" + DLGameTitle + "-" + DLImgType + ".png");
                        SetPath(DLGameTitle, DLImgType, dialogOpen);
                    }
                    catch (Exception e) { Trace.WriteLine(DateTime.Now + ": DownloadImage:" + e); MessageBox.Show("Sorry! That's failed, Try again or try another image"); }
                }
            }
            else if (File.Exists(@"Resources/img/" + DLGameTitle + "-" + DLImgType + ".png"))
            {
                File.Delete(@"Resources/img/" + DLGameTitle + "-" + DLImgType + ".png");
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        client.UseDefaultCredentials = true;
                        client.Proxy.Credentials = CredentialCache.DefaultCredentials;
                        client.DownloadFile(new Uri(url), @"Resources\img\" + DLGameTitle + "-" + DLImgType + ".png");
                        SetPath(DLGameTitle, DLImgType, dialogOpen);
                    }
                    catch (Exception e) { Trace.WriteLine(DateTime.Now + ": DownloadImage2: " + e); }
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
                    DialogEditGames.EditIcon.Text = title + "-" + DLImgType + ".png";
                    OpenImageDL("", "", "");
                }
                else if (dialogType == "add")
                {
                    DialogAddGames.NewGameIcon.Text = title + "-" + DLImgType + ".png";
                    OpenImageDL("", "", "");
                }
            }
            else if (imagetype == "poster")
            {
                if (dialogType == "edit")
                {
                    DialogEditGames.EditPoster.Text = title + "-" + DLImgType + ".png";
                    OpenImageDL("", "", "");
                }
                else if (dialogType == "add")
                {
                    DialogAddGames.NewGamePoster.Text = title + "-" + DLImgType + ".png";
                    OpenImageDL("", "", "");
                }
            }
            else if (imagetype == "banner")
            {
                if (dialogType == "edit")
                {
                    DialogEditGames.EditBanner.Text = title + "-" + DLImgType + ".png";
                    OpenImageDL("", "", "");
                }
                else if (dialogType == "add")
                {
                    DialogAddGames.NewGameBanner.Text = title + "-" + DLImgType + ".png";
                    OpenImageDL("", "", "");
                }
            }
        }
        public void ApplyGenreFilter_OnClick(object sender, RoutedEventArgs e)
        {
            string genreToFilter = ((Button)sender).Tag.ToString();
            if (DataContext == settingsViewModel)
            {
                if (Settings.Default.viewtype == "Poster") { DataContext = posterViewModel; }
                if (Settings.Default.viewtype == "Banner") { DataContext = bannerViewModel; }
                if (Settings.Default.viewtype == "List") { DataContext = listViewModel; }
            }
            pv.GenreToFilter(genreToFilter);
            pv.RefreshList2(cvs);
            bv.GenreToFilter(genreToFilter);
            bv.RefreshList2(cvs);
            lv.GenreToFilter(genreToFilter);
            lv.RefreshList2(cvs);
            MenuToggleButton.IsChecked = false;
        }
        public void PosterViewActive()
        {
            view = "poster";
            DataContext = posterViewModel;
            Settings.Default.viewtype = "Poster";
            Settings.Default.Save();
        }
        public void BannerViewActive()
        {
            view = "banner";
            DataContext = bannerViewModel;
            Settings.Default.viewtype = "Banner";
            Settings.Default.Save();
        }
        public void ListViewActive()
        {
            view = "list";
            DataContext = listViewModel;
            Settings.Default.viewtype = "List";
            Settings.Default.Save();
        }
        public void SettingsViewActive()
        {
            view = "settings";
            settingsViewModel = new SettingsViewModel();
            settingsViewModel.LoadGenres();
            DataContext = settingsViewModel;
        }
        public void IncreaseExeSearch()
        {
            DialogExeSelection.IncreaseImages();
        }

        public void FixFilePaths()
        {
            string file = "./Resources/GamesList.txt";
            string fileout = "./Resources/GamesList2.txt";
            var contents = File.ReadAllLines(file);
            Array.Sort(contents);
            File.WriteAllLines(fileout, contents);
            File.Delete("./Resources/GamesList.txt");
            File.Move("./Resources/GamesList2.txt", "./Resources/GamesList.txt");
            bool textModified = false;
            string installPath = AppDomain.CurrentDomain.BaseDirectory;
            if (File.Exists("./Resources/GamesList.txt"))
            {
                string text = File.ReadAllText("./Resources/GamesList.txt");

                if (text.Contains(installPath + "Resources/img/"))
                {
                    newText = text.Replace(installPath + "Resources/img/", "");
                    textModified = true;
                }
                if (text.Contains(installPath + "Resources\\img\\"))
                {
                    if (textModified)
                    {
                        newText = newText.Replace(installPath + "Resources\\img\\", "");
                    }
                    else
                    {
                        newText = text.Replace(installPath + "Resources\\img\\", "");
                        textModified = true;
                    }
                }
                if (text.Contains(installPath + "Resources/shortcuts/"))
                {
                    if (textModified)
                    {
                        newText = newText.Replace(installPath + "Resources/shortcuts/", "");
                    }
                    else
                    {
                        newText = text.Replace(installPath + "Resources/shortcuts/", "");
                        textModified = true;
                    }
                }
                else if (text.Contains(installPath + "Resources\\shortcuts\\"))
                {
                    if (textModified)
                    {
                        newText = newText.Replace(installPath + "Resources\\shortcuts\\", "");
                    }
                    else
                    {
                        newText = text.Replace(installPath + "Resources\\shortcuts\\", "");
                    }
                }
                if (newText != null)
                {
                    File.WriteAllText("./Resources/GamesList2.txt", newText);
                    File.Delete("./Resources/GamesList.txt");
                    File.Move("./Resources/GamesList2.txt", "./Resources/GamesList.txt");
                }
            }
        }
        public void RefreshGames()
        {
            DialogFrame.Visibility = Visibility.Visible;
            DialogFrame.Content = loadingdialog;
            try
            {
                try { GenreListMW.Clear(); } catch { }
                try { GameListMW.Clear(); } catch { }
                lagbw.RunWorkerAsync();
            }
            catch { }
            if (view == "list") { ListViewActive(); }
            else if (view == "poster") { PosterViewActive(); }
            else if (view == "banner") { BannerViewActive(); }
            else if (view == "settings") { SettingsViewActive(); }
        }
        private void PosterButton_OnClick(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine(DateTime.Now + ": Poster View Active");
            PosterViewActive();
        }
        private void BannerButton_OnClick(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine(DateTime.Now + ": Banner View Active");
            BannerViewActive();
        }
        private void ListButton_OnClick(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine(DateTime.Now + ": List View Active");
            ListViewActive();
        }
        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine(DateTime.Now + ": Settings View Active");
            SettingsViewActive();
        }
        private void RefreshGames_OnClick(object sender, RoutedEventArgs e)
        {
            RefreshGames();
        }
        private void MWSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Resized();
        }
        public void Resized()
        {
            if (isDownloadOpen == true)
            {
                ImageDownload.ChangeWindowSize(this.ActualWidth * 0.8, this.ActualHeight * 0.8);
            }
            if (isExeSearchOpen == true)
            {
                ExeSelection.ChangeWindowSize(this.ActualWidth * 0.9, this.ActualHeight * 0.9);
            }
        }
        public void StoreSize()
        {
            Top = Settings.Default.Top;
            Left = Settings.Default.Left;
            Height = Settings.Default.Height;
            Width = Settings.Default.Width;
            if (Settings.Default.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
        }
        public void Window_Closed(object sender, CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Settings.Default.Top = RestoreBounds.Top;
                Settings.Default.Left = RestoreBounds.Left;
                Settings.Default.Height = RestoreBounds.Height;
                Settings.Default.Location = RestoreBounds.Location.ToString();
                Settings.Default.Maximized = true;
            }
            else
            {
                Settings.Default.Top = Top;
                Settings.Default.Left = Left;
                Settings.Default.Height = Height;
                Settings.Default.Width = Width;
                Settings.Default.Location = RestoreBounds.Location.ToString();
                Settings.Default.Maximized = false;
            }
            
            Settings.Default.Save();
        }
        public void LoadSettings()
        {
            Top = Settings.Default.Top;
            Left = Settings.Default.Left;
            Height = Settings.Default.Height;
            Width = Settings.Default.Width;
            if (Settings.Default.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
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
        }
    }
}