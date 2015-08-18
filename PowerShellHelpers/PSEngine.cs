using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation.Runspaces;
using System.Management.Automation;

namespace PowerShellHelpers
{

    public class PSEngine
    {
        // create private fields 
        private String _scriptPath;
        private IDictionary <string,object> _arguments;
        private Runspace _runspace;

        /// <summary>
        /// Default constructor with 0 parameters
        /// </summary>
        public PSEngine()
        {
            
        }

        /// <summary>
        /// Constructor accepting two parameter arguments
        /// </summary>
        /// <param name="scriptpath"></param>
        /// <param name="arguments"></param>
        public PSEngine(String scriptpath, IDictionary<string, object> arguments)
        {
            _scriptPath = scriptpath;
            _arguments = arguments;
        }

        /// <summary>
        /// This method returns a creates and returns a new Runspace
        /// </summary>
        /// <returns>Runspace</returns>
        public Runspace createRunspace()
        {
            var rs = RunspaceFactory.CreateRunspace();
            rs.ThreadOptions = PSThreadOptions.UseCurrentThread;
            rs.Open();
            return rs;
        }

        /// <summary>
        /// This is the primary method for executing powershell scripts
        /// </summary>
        /// <param name="script"></param>
        /// <param name="args"></param>(optional)
        /// <returns>ICollection<PSObject></returns>
        public ICollection<PSObject> executeScript(string script, IDictionary<string, object> args = null)
        {
            // create runspace if it is null
            if (_runspace == null)
            {
                _runspace = createRunspace();
            }
            
            // The PowerShell class implements a Factory Pattern, offering a Create() method that returns a new PowerShell object
            var ps = PowerShell.Create();
            
            // assign the runspace to the Powershell object
            ps.Runspace = _runspace;

            // create a Command object, initializing it with the script path
            Command psCommand = new Command(script);

            // if the args Dictionary is not null, add them to the Command.Parameters collection
            if (args != null)
            {
                foreach (var arg in args)
                {
                    psCommand.Parameters.Add(arg.Key, arg.Value);
                }
            }

            // add the psCommands object to the Commands property of our PowerShell object
            ps.Commands.Commands.Add(psCommand);

            // Invoke PowerShell asynchronously
            var asyncResult = ps.BeginInvoke();

            // Could perform other tasks here while waiting for script to complete, if needed

            // this is analogous to the "await" keyword in an async method
            asyncResult.AsyncWaitHandle.WaitOne();

            // get the result from PowerShell execution
            var result = ps.EndInvoke(asyncResult);

            // release the resources used by the WaitHandle
            asyncResult.AsyncWaitHandle.Close();

            // return the collection of PSObjects
            return result;
        }

        /// <summary>
        /// method adds a new variable to the PowerShell runspace, which can be accessed from within the script
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void setVariable(string name, object value)
        {
            if (_runspace == null)
            {
                _runspace = createRunspace();
            }
            _runspace.SessionStateProxy.SetVariable(name, value);
        }

    }
}
