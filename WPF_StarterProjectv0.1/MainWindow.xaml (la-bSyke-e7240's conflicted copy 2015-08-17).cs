using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.Win32;
using PowerShellHelpers;
using System.Configuration;
using Clipboard = System.Windows.Clipboard;
using DataFormats = System.Windows.DataFormats;
using MessageBox = System.Windows.MessageBox;


namespace WPF_StarterProjectv0._1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private PSEngine _psEngine;
        private System.Windows.Threading.DispatcherTimer dispatcherTimer;
        private int secs;
        private int mins;
        private int hrs;

        /// <summary>
        ///  Constructor for MainWindow class
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _psEngine = new PSEngine();
            secs = 0;
            mins = 0;
            hrs = 0;
            this.TasksProgressBar.DataContext = this;
            this.Main_ProgressBar.DataContext = this;
            MainProgress = 0;
            Progress = 0;
            Maximum = 100.0;
            Minimum = 0.0;
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);

        }

        private double _minimum;

        public double Minimum
        {
            get { return _minimum; }

            set
            {
                if (_minimum.Equals(value))
                {
                    return;
                }

                _minimum = value;
                OnPropertyChanged();
            }
        }

        private double _maximum;

        public double Maximum
        {
            get { return _maximum; }

            set
            {
                if (_maximum == value)
                {
                    return;
                }

                _maximum = value;
                OnPropertyChanged();
            }
        }

        private double _progress;

        public double Progress
        {
            get { return _progress; }

            set
            {
                if (_progress == value)
                {
                    return;
                }

                _progress = value;

                OnPropertyChanged();
            }
        }

        private double _mainProgress;

        public double MainProgress
        {
            get { return _mainProgress; }

            set
            {
                if (_mainProgress == value)
                {
                    return;
                }

                _mainProgress = value;

                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var trackTimer = mins.ToString("00") + ":" + secs.ToString("00");

            if (secs < 60) { secs++; }
            if (secs == 60)
            {
                secs = 0;
                mins++;
                if (hrs > 0)
                {
                    trackTimer = hrs.ToString("00") + ":" + mins.ToString("00") + ":" + secs.ToString("00");
                }
                else
                {
                    trackTimer = mins.ToString("00") + ":" + secs.ToString("00");
                }

            }
            if (mins > 59)
            {
                mins = 0;
                hrs++;
                trackTimer = hrs.ToString("00") + ":" + mins.ToString("00") + ":" + secs.ToString("00");
            }

            Main_Timer.Content = trackTimer;
        }


        private async void DepCheckButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dispatcherTimer.Start();
                //create the dictionary of string(key) & object(value) containing arguments to be passed
                //we use the Type "object" for the value so that we can pass anything into it regardless of type
                var args = new Dictionary<string, object>();

                // add "Context" argument with this class as the value, to be passed to the $Context parameter in the script
                args.Add("Context", this);

                // get script path from config file
                var script = ConfigurationManager.AppSettings["CheckPSHostVersion"];

                // Use the TaskFactory.StartNew() method to execute the powershell script as an asynchronous task
                // Documentation on this can be found here: https://msdn.microsoft.com/en-us/library/dd321439(v=vs.110).aspx 
                // Note: we are passing both 1) the powershell script path, and 2) the args Dictionary to executeScript() method
                var scriptTask = Task<ICollection<PSObject>>.Factory.StartNew(() => _psEngine.executeScript(script, args));

                // Add initial message to console (scroll view)
                DepCheckScrollViewer.Content += "Checking dependencies...\r\n";

                // the await keyword is used in C# for asynchronous operations. Notice that it is awaiting the completion of 
                // the Task "scriptTask" that was assigned above. Async/Await documentation: https://msdn.microsoft.com/en-us/library/hh191443.aspx
                await scriptTask;

                // after the Task completes, the Task's "Results" property will contain the object returned from the script execution
                var r = scriptTask.Result.ToArray();

                //loop through the results returned from the PowerShell pipeline
                foreach (PSObject rs in r)
                {
                    // if PSObject is null, continue to next loop iteration
                    if (rs == null) continue;

                    // add result content to the scroll view
                    DepCheckScrollViewer.Content += rs.BaseObject.ToString() + "\r\n";
                }

                // stop the timer
                dispatcherTimer.Stop();

                // get the status code, which should be the final object from the array of returned results
                var statusCode = int.Parse(r[r.Length - 1].BaseObject.ToString());

                if (statusCode == 0)
                {
                    //activate tabs
                    excutetasks_runbt1.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                // display exception message in messagebox window
                MessageBox.Show(ex.Message);
                // add exception message to console (scroll view)
                DepCheckScrollViewer.Content += ex.Message + "\r\n";
                // add exception message to debugger log output
                Debug.WriteLine(ex);
            }
        }


        private async void excutetasks_runbt1_Click(object sender, RoutedEventArgs e)
        {
            // start of try/catch block
            try
            {
                // get the script to run from the AppSettings section in the App.config file
                var script = ConfigurationManager.AppSettings["GetService_test"];

                //create the dictionary of string(key) & object(value) containing arguments to be passed
                //we use the Type "object" for the value so that we can pass anything into it regardless of type
                var args = new Dictionary<string, object>();
                if (String.IsNullOrEmpty(IdentityBox.Text) || String.IsNullOrEmpty(UserNameBox.Text) || PasswordInputBox.SecurePassword.Length < 1)
                {
                    MessageBox.Show("Please provide values for all parameters");
                    return;
                }
                // add values to args dictionary
                args.Add("Identity", IdentityBox.Text);
                args.Add("Creds", new PSCredential(UserNameBox.Text, PasswordInputBox.SecurePassword));
                args.Add("Context", this);

                // Use the TaskFactory.StartNew() method to execute the powershell script as an asynchronous task
                // Documentation on this can be found here: https://msdn.microsoft.com/en-us/library/dd321439(v=vs.110).aspx 
                // Note: we are passing both 1) the powershell script path, and 2) the args Dictionary to executeScript() method
                var scriptTask = Task<ICollection<PSObject>>.Factory.StartNew(() =>
                {
                    var res = _psEngine.executeScript(script, args);

                    return res;
                });

                // log a message to the ScrollView (this is an example of the fact that we can perform other tasks while the 
                // script is executing. So, between "Factory.StartNew()" above, and "await scriptTask" below, you can run other code
                ConsoleResult.Content = "Getting results of command: Get-Service" + "\r\n";

                // the await keyword is used in C# for asynchronous operations. Notice that it is awaiting the completion of 
                // the Task "scriptTask" that was assigned above. Async/Await documentation: https://msdn.microsoft.com/en-us/library/hh191443.aspx
                await scriptTask;

                // after the Task completes, the Task's "Results" property will contain the object returned from the script execution
                var results = scriptTask.Result.ToArray();

                // create a new List of ServiceResult objects (see ServiceResult.cs class)
                var serviceResultList = new List<ServiceResult>();

                // Iterate through the result list to create the ServiceResult objects 
                foreach (PSObject o in results)
                {
                    // in case an object returned is null - continue to next iteration
                    if (o == null) continue;

                    // If you know the property names to look for within the PSObjects returned from the powershell
                    // script, you can access them explicitly using the syntax "o.Members["PropertyName"].Value"
                    // To identify the names to look for, you can just set a breakpoint and inspect the contents of the
                    // "Members" property in the returned PSObjects
                    if (o.Members["ServiceName"] != null) // <-- check if the "ServiceName" property exists
                    {
                        // parse the desired properties from the returned PSObject
                        string name = o.Members["ServiceName"].Value.ToString();
                        string status = o.Members["Status"].Value.ToString();
                        string displayName = o.Members["DisplayName"].Value.ToString();

                        // add a new instance of ServiceResult to the "serviceResultList", containing the values parsed 
                        //from the PSObject above
                        serviceResultList.Add(new ServiceResult(status, name, displayName));
                    }
                    else // <-- the "ServiceName" property does not exist... log it to console
                    {
                        ConsoleResult.Content += o.BaseObject + "\r\n";
                    }
                }

                // add message to the ScrollView
                ConsoleResult.Content += "Complete";

                // To create the data in the DataGrid, we just have to set the ItemsSource property to 
                // an object that implements the IEnumerable interface (such as a List or ArrayList)... 
                // in this case, we can pass in our List<ServiceResult> serviceResultList, and the Grid creates itself
                // from the data contained in the list. Simple!
                ResultGrid.ItemsSource = serviceResultList;
            }
            catch (Exception ex)
            {
                // show messagebox dialog
                MessageBox.Show(ex.Message);
                // add exception message to console
                ConsoleResult.Content += ex.Message + "\r\n";
                // add exception message to debugger log
                Debug.WriteLine(ex);
            }
        }


        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ResultGrid.SelectAllCells();
                ResultGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
                ApplicationCommands.Copy.Execute(null, ResultGrid);
                ResultGrid.UnselectAllCells();
                var result = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
                Clipboard.Clear();

                var sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.Filter = @"CSV files (*.csv)|*.csv";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var writer = new StreamWriter(sfd.OpenFile());
                    writer.WriteLine(result);
                    writer.Close();
                }

                MessageBox.Show("CSV Export complete.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("CSV Export failed. \n" + ex.Message);
            }
        }

    }
}

/* ToDo
-ProgressBar
-Passing script variables to the powershell script
-Parsing the Results: GridView(rows/colums)
Export the Results: txt,csv
-Error Handling and informing the user and getting the errors from powershell
*/