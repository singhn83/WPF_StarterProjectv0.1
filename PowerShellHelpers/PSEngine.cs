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
        private String _scriptPath;
        private IDictionary <string,object> _arguments;
        private Runspace _runspace;
        public PSEngine() { }

        public PSEngine(String scriptpath, IDictionary<string, object> arguments)
        {
            _scriptPath = scriptpath;
            _arguments = arguments;
        }

        public Runspace createRunspace()
        {
            var rs = RunspaceFactory.CreateRunspace();
            rs.ThreadOptions = PSThreadOptions.UseCurrentThread;
            rs.Open();
            return rs;
        }

        public ICollection<PSObject> executeScript(string script, IDictionary<string, object> args = null)
        {
            if (_runspace == null)
            {
                _runspace = createRunspace();
            }
            var ps = PowerShell.Create();
            ps.Runspace = _runspace;
            Command psCommand = new Command(script);
            if (args != null)
            {
                foreach (var arg in args)
                {
                    psCommand.Parameters.Add(arg.Key, arg.Value);
                }
            }
            ps.Commands.Commands.Add(psCommand);
            var asyncResult = ps.BeginInvoke();
            asyncResult.AsyncWaitHandle.WaitOne();
            var result = ps.EndInvoke(asyncResult);
            asyncResult.AsyncWaitHandle.Close();
            return result;
        }

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
