using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using PowerShellHelpers;
using System.Configuration;


namespace WPF_StarterProjectv0._1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        private PSEngine _psEngine;
        private System.Windows.Threading.DispatcherTimer dispatcherTimer;
        private int secs;
        private int mins;
        private int hrs;
        
        /// <summary>
        ///  Constructor Class
        /// </summary>

        public MainWindow()
        {
            InitializeComponent();
            _psEngine = new PSEngine();
            secs = 0;
            mins = 0;
            hrs = 0;
            _psEngine.setVariable("ProgressBar", this.Main_ProgressBar);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

            var trackTimer = mins.ToString("00") + ":" + secs.ToString("00");

            if (secs < 60) { secs++; }
            if (secs == 60)
            {
                secs = 0;
                mins++;
                if(hrs > 0)
                {
                    trackTimer = hrs.ToString("00") + ":" + mins.ToString("00") + ":" + secs.ToString("00");
                }
                else
                {
                    trackTimer = mins.ToString("00") + ":" + secs.ToString("00");
                }
                
            }
            if(mins > 59)
            {
                mins = 0;
                hrs++;
                trackTimer = hrs.ToString("00") + ":" + mins.ToString("00") + ":" + secs.ToString("00");
            }

            Main_Timer.Content = trackTimer;
        }

        private void checkProgress()
        {
           
        }

        private async void DepCheckButton_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            
            var appSettings = ConfigurationManager.AppSettings;
            var script = appSettings["CheckPSHostVersion"];
            var scriptTask = Task<ICollection<PSObject>>.Factory.StartNew(() => _psEngine.executeScript(script));
            dispatcherTimer.Start();

            await scriptTask;
            var r = scriptTask.Result.ToArray();
            
            foreach(PSObject rs in r)
            {
                DepCheckScrollViewer.Content += rs.BaseObject.ToString() + "\r\n";
            }
            dispatcherTimer.Stop();

            var statusCode = int.Parse(r[r.Length - 1].BaseObject.ToString());

            if(statusCode == 0)
            {
                //activate tabs
                excutetasks_runbt1.IsEnabled = true;
            }
    }
    }
}

/* ToDo
ProgressBar
Passing script variables to the powershell script
Parsing the Results: GridView(rows/colums)
Export the Results: txt,csv
Error Handling and informing the user and getting the errors from powershell
*/