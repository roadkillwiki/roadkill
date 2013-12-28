using System;
using System.Collections.Generic;
using System.Diagnostics;

// https://github.com/Workshop2/LocalDbApi
namespace LocalDbApi.Communications
{
    internal class DbCommunication : IDbCommunication
    {
        private const string ActionName = "SqlLocalDb";
        private ProcessStartInfo Command { get; set; }

        public DbCommunication()
        {
            Command = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = ActionName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
        }

        public void Execute(string arguments)
        {
            Console.Write(ExecuteString(arguments));
        }

        public string ExecuteString(string arguments)
        {
            Command.Arguments = arguments;

            using (var process = new Process())
            {
                process.StartInfo = Command;
                process.Start();
                process.WaitForExit();

                return process.StandardOutput.ReadToEnd();
            }
        }

        public IEnumerable<string> ExecuteList(string arguments)
        {
            Command.Arguments = arguments;

            using (var process = new Process())
            {
                process.StartInfo = Command;
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    yield return process.StandardOutput.ReadLine();
                }
            }
        }
    }
}