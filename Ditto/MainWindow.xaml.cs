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
            this.fileSync.RunWorkerCompleted += this.fileSync_RunWorkerCompleted;  //Tell the user how the process went
            this.fileSync.WorkerReportsProgress = true;
            this.fileSync.WorkerSupportsCancellation = true; //Allow for the process to be cancelled

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
            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(1000);
                this.fileSync.ReportProgress(i);

                //Check if there is a request to cancel the process
                if (this.fileSync.CancellationPending)
                {
                    e.Cancel = true;
                    this.fileSync.ReportProgress(0);
                    return;
                }
            }
            //If the process exits the loop, ensure that progress is set to 100%
            //Remember in the loop we set i < 100 so in theory the process will complete at 99%
            this.fileSync.ReportProgress(100);
        }

        private void fileSync_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.textBox.Text = e.ProgressPercentage.ToString();
        }

        private void fileSync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            // set toExit to true before closing so that the close event is not cancelled
            toExit = true;
            this.Close();
        }

        private BackgroundWorker fileSync;
        private System.Windows.Forms.ContextMenu trayMenu;
        private System.Windows.Forms.MenuItem exitMenuItem;
        private Boolean toExit;     // boolean value used to decide whether to close or just minimize the program

        private static String startBtnStr = "Start";
        private static String stopBtnStr = "Stop";
        private static String iconPath = "Main.ico";
    }
}
