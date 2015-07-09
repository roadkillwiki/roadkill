using System;

namespace Roadkill.Tests.Setup
{
	public class EnvironmentalVariables
	{
		public static string GetVariable(string name)
		{
			string value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);
			
			if (string.IsNullOrEmpty(value))
				value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);

			if (string.IsNullOrEmpty(value))
				value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);

			return value;
		}
	}
}