using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.IO;

namespace Ditto
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.trayMenu = new System.Windows.Forms.ContextMenu();
            this.exitMenuItem = new System.Windows.Forms.MenuItem();

            // Initialize trayMenu
            this.trayMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[]{
                this.exitMenuItem
            });

            // Initialize exitMenuItem
            this.exitMenuItem.Index = 0;
            this.exitMenuItem.Text = "E&xit";
            this.exitMenuItem.Click += new EventHandler(this.exitMenuItem_Click);

            // Initialize tray icon
            NotifyIcon trayIcon = new NotifyIcon();
            trayIcon.Icon = new System.Drawing.Icon(iconPath);
            trayIcon.Visible = true;
            trayIcon.DoubleClick +=
                delegate(object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
            trayIcon.ContextMenu = this.trayMenu;

            // Initialize the background worker for synchronizing files
            this.fileSync = new BackgroundWorker();
            this.fileSync.DoWork += this.fileSync_DoWork;
            this.fileSync.ProgressChanged += this.fileSync_ProgressChanged;
            this.fileSync.RunWorkerCompleted += this.fileSync_RunWorkerCompleted;
            this.fileSync.WorkerReportsProgress = true;
            this.fileSync.WorkerSupportsCancellation = true;

            fsWatcher.Changed += new FileSystemEventHandler(OnFileChanged);

            startStopBtn.Content = startBtnStr;
            this.toExit = false;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            // hide window completely instead of minimizing
            if(WindowState == WindowState.Minimized)
            {
                this.Hide();
            }

            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // cancel the close event unless toExit is true
            if (!toExit)
            {
                e.Cancel = true;
            }

            this.Hide();

            base.OnClosing(e);
        }

        private void startStopBtn_Click(object sender, RoutedEventArgs e)
        {
            if(startBtnStr == (String)(this.startStopBtn.Content))
            {
                this.startStopBtn.Content = stopBtnStr;
                this.fileSync.RunWorkerAsync();
            }
            else
            {
                this.startStopBtn.Content = startBtnStr;
                if(this.fileSync.IsBusy)
                {
                    this.fileSync.CancelAsync();
                }
            }
        }

        private void fileSync_DoWork(object sender, DoWorkEventArgs e)
        {
            // Initialize the file system watcher for detecting file changes
            this.fsWatcher = new FileSystemWatcher();
            fsWatcher.EnableRaisingEvents = true;
            fsWatcher.NotifyFilter = NotifyFilters.LastWrite;
        }

        private void fileSync_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Report progress to UI
        }

        private void fileSync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Clean up worker task
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            // set toExit to true before closing so that the close event is not cancelled
            toExit = true;
            this.Close();
        }

        private void OnFileChanged(object source, FileSystemEventArgs e)
        {
            // Copy file to other directory
        }

        private BackgroundWorker fileSync;
        private FileSystemWatcher fsWatcher;

        private System.Windows.Forms.ContextMenu trayMenu;
        private System.Windows.Forms.MenuItem exitMenuItem;

        private Boolean toExit;     // boolean value used to decide whether to close or just minimize the program

        private static String startBtnStr = "Start";
        private static String stopBtnStr = "Stop";
        private static String iconPath = "Main.ico";
    }
}
