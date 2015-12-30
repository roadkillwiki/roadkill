using System;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;

namespace Roadkill.Tests.Unit.Logging
{
	public class LoggingTests
	{
		private DebugTarget GetDebugTarget(LogLevel level)
		{
			DebugTarget debugTarget = new DebugTarget();
			debugTarget.Layout = "${message}";
			SimpleConfigurator.ConfigureForTargetLogging(debugTarget, level);
			return debugTarget;
		}

		[Test]
		public void Debug_should_log_debug_level_messages()
		{
			// Arrange
			DebugTarget debugTarget = GetDebugTarget(LogLevel.Debug);

			// Act
			Log.Debug("Test debug message");

			// Assert
			Assert.That(debugTarget.LastMessage, Is.EqualTo("Test debug message"));
		}

		[Test]
		public void Information_should_log_info_level_messages()
		{
			// Arrange
			DebugTarget debugTarget = GetDebugTarget(LogLevel.Info);

			// Act
			Log.Information("Test info message");

			// Assert
			Assert.That(debugTarget.LastMessage, Is.EqualTo("Test info message"));
		}

		[Test]
		public void Warn_should_log_info_level_messages()
		{
			// Arrange
			DebugTarget debugTarget = GetDebugTarget(LogLevel.Warn);

			// Act
			Log.Warn("Test warn message");

			// Assert
			Assert.That(debugTarget.LastMessage, Is.EqualTo("Test warn message"));
		}

		[Test]
		public void Error_should_log_info_level_messages()
		{
			// Arrange
			DebugTarget debugTarget = GetDebugTarget(LogLevel.Error);

			// Act
			Log.Error("Test error message");

			// Assert
			Assert.That(debugTarget.LastMessage, Is.EqualTo("Test error message"));
		}

		[Test]
		public void ConfigureLogging_should_replace_tilde_with_base_directory_and_set_nlog_config_path()
		{
			// Arrange
			string expectedPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\Roadkill.Tests.dll.config";
			var appSettings = new ApplicationSettings()
			{
				NLogConfigFilePath = @"~\Roadkill.Tests.dll.config"
			};

			// Act
			Log.ConfigureLogging(appSettings);

			// Assert
			LogManager.Configuration.Reload();
			Assert.That(Log.NLogConfigPath, Is.EqualTo(expectedPath));
			Assert.That(LogManager.Configuration.AllTargets.Count, Is.EqualTo(1), "Should have 1 target (ConsoleTarget from the app.config");
		}

		[Test]
		public void ConfigureLogging_should_throw_exception_when_nlog_path_is_empty()
		{
			// Arrange
			var appSettings = new ApplicationSettings()
			{
				NLogConfigFilePath = ""
			};

			// Act + Assert
			Assert.Throws<ConfigurationException>(() => Log.ConfigureLogging(appSettings));
		}

		[Test]
		public void ConfigureLogging_should_throw_exception_when_nlog_path_does_not_exist()
		{
			// Arrange
			var appSettings = new ApplicationSettings()
			{
				NLogConfigFilePath = "~/Bob/Loblaw/Config.config"
			};

			// Act + Assert
			Assert.Throws<ConfigurationException>(() => Log.ConfigureLogging(appSettings));
		}
	}
}
