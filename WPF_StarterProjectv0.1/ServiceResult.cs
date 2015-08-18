using System;

namespace WPF_StarterProjectv0._1
{
    /// <summary>
    /// This class represents details for an individual Service
    /// </summary>
    public class ServiceResult
    {
        // create properties
        public String Status { get; set; }
        public String Name { get; set; }
        public String DisplayName { get; set; }

        /// <summary>
        /// Default constructor, accepting 0 parameters
        /// </summary>
        public ServiceResult()
        {
        }

        /// <summary>
        /// Constructor accepting 3 arguments
        /// </summary>
        /// <param name="status"></param>
        /// <param name="name"></param>
        /// <param name="displayname"></param>
        public ServiceResult(string status, string name, string displayname)
        {
            Status = status;
            Name = name;
            DisplayName = displayname;
        }
    }
}
