using System.Collections.Generic;
using System.Linq;
using LocalDbApi.Communications;
using LocalDbApi.Models;

// https://github.com/Workshop2/LocalDbApi
namespace LocalDbApi
{
    public class Instance
    {
        private IDbCommunication Command { get; set; }

        public Instance() : this(new DbCommunication()) {}

        public Instance(IDbCommunication command)
        {
            Command = command;
        }
        
        public void Create(string instanceName)
        {
            string arguments = string.Concat("create ", instanceName, " -s");

            Command.Execute(arguments);
        }

        public void Delete(string instanceName)
        {
            StopInstance(instanceName);

            string arguments = string.Concat("delete ", instanceName);

            Command.Execute(arguments);
        }

        public void StartInstance(string instanceName)
        {
            string arguments = string.Concat("start ", instanceName);

            Command.Execute(arguments);
        }

        public void StopInstance(string instanceName)
        {
            string arguments = string.Concat("stop ", instanceName);

            Command.Execute(arguments);
        }

        public IEnumerable<string> ListInstances()
        {
            const string arguments = "info";

            return Command.ExecuteList(arguments);
        }

        public Info GetInstance(string instanceName)
        {
            string arguments = string.Concat("info ", instanceName);

            List<string> instanceRawInfo = Command.ExecuteList(arguments).ToList();

            return new Info(instanceRawInfo);
        }
    }
}