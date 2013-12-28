using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// https://github.com/Workshop2/LocalDbApi
namespace LocalDbApi.Models
{
    public class Info
    {
        private List<string> InstanceInfo { get; set; }

        public string Name { get; set; }
        public Version Version { get; set; }
        public string SharedName { get; set; }
        public string Owner { get; set; }
        public bool AutoCreate { get; set; }
        public State State { get; set; }
        public DateTime LastStartTime { get; set; }
        public string InstancePipeName { get; set; }

        public Info(IEnumerable<string> instanceInfo)
        {
            InstanceInfo = instanceInfo.ToList();

            Name = GetValue(FindLine("Name:"));
            Version = GetVersion(FindLine("Version:"));
            SharedName = GetValue(FindLine("SharedName:"));
            Owner = GetValue(FindLine("Owner:"));
            AutoCreate = GetBool(FindLine("Auto-create:"));
            State = GetState(FindLine("State:"));
            LastStartTime = GetDateTime(FindLine("Last start time:"));
            InstancePipeName = GetValue(FindLine("Instance pipe name:"));
        }

        private string FindLine(string name)
        {
            return InstanceInfo.FirstOrDefault(x => x.StartsWith(name, StringComparison.InvariantCultureIgnoreCase));
        }

        private static string GetValue(string rawLine)
        {
            if (string.IsNullOrEmpty(rawLine))
            {
                return string.Empty;
            }

            string[] split = rawLine.Split(':');
            StringBuilder builder = new StringBuilder();

            for (int i = 1; i < split.Count(); i++)
            {
                builder.Append(builder.Length > 0 ? ":" : string.Empty);
                builder.Append(split[i]);
            }

            return builder.ToString().Trim();
        }

        private static Version GetVersion(string rawLine)
        {
            string value = GetValue(rawLine);

            return new Version(value);
        }

        private static bool GetBool(string rawLine)
        {
            string value = GetValue(rawLine);

            if (value.Equals("Yes", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private State GetState(string rawLine)
        {
            string value = GetValue(rawLine);
            State result = State.Unknown;

            if (value.Equals("Running", StringComparison.InvariantCultureIgnoreCase))
            {
                result = State.Running;
            }
            else if (value.Equals("Stopped", StringComparison.InvariantCultureIgnoreCase))
            {
                result = State.Stopped;
            }

            return result;
        }

        private DateTime GetDateTime(string rawLine)
        {
            string value = GetValue(rawLine);
            return DateTime.Parse(value);
        }
    }
}