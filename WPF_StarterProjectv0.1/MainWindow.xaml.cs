using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private int _progress = 0;

        public int progress
        {
            get { return _progress; }
            set
            {
                if (value == _progress) return;
                _progress = value;
                OnPropertyChanged("progress");
            }
        }

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
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

            DepCheckScrollViewer.Content += "Checking dependencies...\r\n";

            await scriptTask;
            var r = scriptTask.Result.ToArray();
            
            foreach(PSObject rs in r)
            {
                if (rs == null) continue;
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


        private async void excutetasks_runbt1_Click(object sender, RoutedEventArgs e)
        {
            progress = 20;
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
            args.Add("class", this);

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
    }
}

/* ToDo
ProgressBar
Passing script variables to the powershell script
Parsing the Results: GridView(rows/colums)
Export the Results: txt,csv
Error Handling and informing the user and getting the errors from powershell
*/