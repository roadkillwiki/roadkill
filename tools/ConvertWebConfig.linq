<Query Kind="Program">
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Xml.Linq</Namespace>
</Query>

// Converts the settings of a Roadkill 2.x web.config into the appsettings.json format used by Roadkill 3 (.NET 10), then
// lists the XML files it read (no longer used) and the other files to copy from the 2.x site.
// Set the paths below and run (F5).
// Same code as ConvertWebConfig.cs (which runs with "dotnet run ConvertWebConfig.cs -- <web.config> [appsettings.json]").

int Main()
{
	// The web.config of the Roadkill 2.x site
	string webConfigPath = @"C:\inetpub\roadkill\web.config";

	// The appsettings.json to write. Empty: in the folder of this script. If it exists (e.g. the one of the new site), only
	// its "ConnectionStrings:Roadkill" value and "Roadkill" section are replaced, its other settings are kept.
	string appSettingsPath = @"";

	// Util.CurrentQueryPath is null if the query was never saved
	string scriptDirectory = Path.GetDirectoryName(Util.CurrentQueryPath) ?? Environment.CurrentDirectory;
	return Migration.Run(webConfigPath, appSettingsPath, scriptDirectory);
}

static class Migration
{
	// The themes and plugin folders that come with Roadkill: other folders are yours
	static readonly string[] BuiltInThemes = { "BlackBar", "Mediawiki", "Plain", "Responsive" };
	static readonly string[] BuiltInPluginFolders = { "SyntaxHighlighter", "Mermaid" };

	/// <summary>
	/// Converts the web.config, writes appsettings.json (in scriptDirectory if no path is given) and lists what else
	/// to copy from the 2.x site.
	/// </summary>
	public static int Run(string webConfigPath, string? appSettingsPath, string scriptDirectory)
	{
		webConfigPath = Path.GetFullPath(webConfigPath);
		if (!File.Exists(webConfigPath))
		{
			Console.Error.WriteLine($"File not found: {webConfigPath}");
			return 1;
		}

		string oldSite = Path.GetDirectoryName(webConfigPath)!;
		bool isDefaultPath = string.IsNullOrWhiteSpace(appSettingsPath);
		appSettingsPath = Path.GetFullPath(isDefaultPath ? Path.Combine(scriptDirectory, "appsettings.json") : appSettingsPath!);

		var warnings = new List<string>();
		JsonObject converted = WebConfigConverter.Convert(webConfigPath, warnings);

		// Write (or update) appsettings.json, keeping its other settings
		bool exists = File.Exists(appSettingsPath);
		JsonObject root = new JsonObject();
		if (exists)
		{
			var documentOptions = new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
			root = JsonNode.Parse(File.ReadAllText(appSettingsPath), documentOptions: documentOptions) as JsonObject ?? new JsonObject();
		}

		if (root["ConnectionStrings"] is not JsonObject connectionStrings)
		{
			connectionStrings = new JsonObject();
			root["ConnectionStrings"] = connectionStrings;
		}

		connectionStrings["Roadkill"] = converted["ConnectionStrings"]!["Roadkill"]!.DeepClone();
		root["Roadkill"] = converted["Roadkill"]!.DeepClone();

		Directory.CreateDirectory(Path.GetDirectoryName(appSettingsPath)!);
		File.WriteAllText(appSettingsPath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

		// Report
		Console.WriteLine($"{(exists ? "UPDATED" : "CREATED")}: {appSettingsPath}");
		if (isDefaultPath)
			Console.WriteLine("  -> copy it to the root of the new (Roadkill 3) site, replacing its appsettings.json.");

		if (warnings.Count > 0)
		{
			Console.WriteLine();
			Console.WriteLine("WARNINGS");
			foreach (string warning in warnings)
				Console.WriteLine("  - " + warning);
		}

		Console.WriteLine();
		Console.WriteLine("XML CONFIGURATION FILES READ (not used by Roadkill 3: don't copy them, delete them with the 2.x site)");
		foreach (string file in WebConfigConverter.FilesRead)
			Console.WriteLine("  - " + file);

		Console.WriteLine();
		Console.WriteLine($"OTHER FILES TO COPY FROM {oldSite} (same relative paths in the new site)");
		ListFilesToCopy(oldSite, (string)converted["Roadkill"]!["AttachmentsFolder"]!);

		Console.WriteLine();
		Console.WriteLine("DON'T COPY: bin\\, Views\\, App_Data\\Internal\\Search\\ (rebuild the index after the first start: Site settings > Tools), the XML .config files.");
		Console.WriteLine("Details: docs/migration-v2-vers-v3.md (French) or docs/upgrade-v2-to-v3.md (English).");
		return 0;
	}

	static void ListFilesToCopy(string oldSite, string attachmentsFolder)
	{
		// Attachments
		string attachments = attachmentsFolder.StartsWith("~")
			? Path.GetFullPath(Path.Combine(oldSite, attachmentsFolder.TrimStart('~', '/', '\\').Replace('/', Path.DirectorySeparatorChar)))
			: attachmentsFolder;

		if (!attachmentsFolder.StartsWith("~"))
			Report(true, attachments, "attachments: outside the site, nothing to copy (the new site uses the same folder)");
		else if (Directory.Exists(attachments))
			Report(true, attachments, $"attachments ({Directory.GetFiles(attachments, "*", SearchOption.AllDirectories).Length} files): copy");
		else
			Report(false, attachments, "attachments: not found");

		// App_Data files that you may have changed
		ReportIfExists(Path.Combine(oldSite, "App_Data", "customvariables.xml"), "custom tokens: copy if you changed it");
		ReportIfExists(Path.Combine(oldSite, "App_Data", "EmailTemplates"), "email templates: copy if you changed them");
		ReportIfExists(Path.Combine(oldSite, "App_Data", "NLog.config"), "logging settings: optional, the old file works (the new one logs to App_Data\\Logs)");
		ReportIfExists(Path.Combine(oldSite, "App_Data", "Internal", "htmlwhitelist.xml"),
			"HTML whitelist: DON'T copy, unless you changed it: then add your changes to the new file (it allows the GFM Markdown tags)");

		// Themes
		string themes = Path.Combine(oldSite, "Themes");
		if (Directory.Exists(themes))
		{
			foreach (string theme in Directory.GetDirectories(themes))
			{
				string name = Path.GetFileName(theme);
				if (BuiltInThemes.Contains(name, StringComparer.OrdinalIgnoreCase))
				{
					Report(true, theme, "built-in theme: copy only the CSS/images you changed");
				}
				else
				{
					bool hasLayout = File.Exists(Path.Combine(theme, "Theme.cshtml"));
					Report(true, theme, "your theme: copy" + (hasLayout ? ", then change 3 lines of Theme.cshtml (@Html.Action no longer exists, see the guide)" : ""));
				}
			}
		}

		// Plugins
		string plugins = Path.Combine(oldSite, "Plugins");
		if (Directory.Exists(plugins))
		{
			foreach (string plugin in Directory.GetDirectories(plugins).Where(x => !BuiltInPluginFolders.Contains(Path.GetFileName(x), StringComparer.OrdinalIgnoreCase)))
				Report(true, plugin, "your plugin: copy, and rebuild it for .NET 10");
		}
	}

	static void ReportIfExists(string path, string description)
	{
		Report(File.Exists(path) || Directory.Exists(path), path, description);
	}

	static void Report(bool found, string path, string description)
	{
		Console.WriteLine($"  [{(found ? "x" : " ")}] {path}");
		Console.WriteLine($"      {(found ? description : "not found")}");
	}
}

static class WebConfigConverter
{
	/// <summary>The XML files read (the web.config and its configSource files).</summary>
	public static readonly List<string> FilesRead = new();

	public static JsonObject Convert(string webConfigPath, List<string> warnings)
	{
		string baseDirectory = Path.GetDirectoryName(Path.GetFullPath(webConfigPath))!;
		XElement configuration = XDocument.Load(webConfigPath).Root!;
		FilesRead.Add(Path.GetFullPath(webConfigPath));

		XElement roadkill = ResolveConfigSource(configuration.Element("roadkill"), baseDirectory)
			?? throw new InvalidOperationException($"{webConfigPath} does not contain a roadkill section");

		var section = new JsonObject
		{
			["Installed"] = GetBool(roadkill, "installed", false),
			["DatabaseName"] = ConvertDatabaseName(GetString(roadkill, "databaseName", GetString(roadkill, "dataStoreType", "")), warnings),
			["AdminRoleName"] = GetString(roadkill, "adminRoleName", "Admin"),
			["EditorRoleName"] = GetString(roadkill, "editorRoleName", "Editor"),
			["AttachmentsFolder"] = GetString(roadkill, "attachmentsFolder", "~/App_Data/Attachments"),
			["AttachmentsRoutePath"] = GetString(roadkill, "attachmentsRoutePath", "Attachments"),
			["ApiKeys"] = GetString(roadkill, "apiKeys", ""),
			["IsPublicSite"] = GetBool(roadkill, "isPublicSite", true),
			["IgnoreSearchIndexErrors"] = GetBool(roadkill, "ignoreSearchIndexErrors", true),
			["UseHtmlWhiteList"] = GetBool(roadkill, "useHtmlWhiteList", true),
			["UseObjectCache"] = GetBool(roadkill, "useObjectCache", true),
			["UseBrowserCache"] = GetBool(roadkill, "useBrowserCache", false),
			["UserServiceType"] = GetString(roadkill, "userServiceType", ""),
			["UseAzureFileStorage"] = GetBool(roadkill, "useAzureFileStorage", false),
			["AzureConnectionString"] = GetString(roadkill, "azureConnectionString", ""),
			["AzureContainer"] = GetString(roadkill, "azureContainer", ""),
			["UiLanguage"] = "en",
			["SmtpHost"] = "",
			["SmtpPort"] = 25,
			["SmtpUsername"] = "",
			["SmtpPassword"] = "",
			["SmtpEnableSsl"] = false,
			["SmtpFrom"] = "signup@roadkillwiki.net",
			["SmtpPickupDirectory"] = "~/App_Data/TempSmtp",
			["IsDemoSite"] = false
		};

		if (GetBool(roadkill, "useWindowsAuthentication", false))
			warnings.Add("Windows authentication is no longer supported: the users stored in the Roadkill database are used instead.");

		// Connection string
		string connectionStringName = GetString(roadkill, "connectionStringName", "Roadkill");
		XElement? connectionStrings = ResolveConfigSource(configuration.Element("connectionStrings"), baseDirectory);
		string connectionString = connectionStrings?.Elements("add")
			.Where(x => (string?)x.Attribute("name") == connectionStringName)
			.Select(x => (string?)x.Attribute("connectionString"))
			.FirstOrDefault() ?? "";

		if (connectionString == "")
			warnings.Add($"The connection string '{connectionStringName}' was not found.");
		else if ((string)section["DatabaseName"]! == "SqlServer" &&
			!connectionString.Contains("TrustServerCertificate", StringComparison.OrdinalIgnoreCase) &&
			!connectionString.Contains("Encrypt", StringComparison.OrdinalIgnoreCase))
			warnings.Add("Microsoft.Data.SqlClient encrypts connections by default: if SQL Server has no trusted certificate, add \"TrustServerCertificate=true\" to the connection string.");

		// UI language (system.web/globalization uiCulture)
		XElement? globalization = ResolveConfigSource(configuration.Element("system.web")?.Element("globalization"), baseDirectory);
		string uiCulture = GetString(globalization, "uiCulture", "");
		if (uiCulture != "" && !uiCulture.StartsWith("auto", StringComparison.OrdinalIgnoreCase))
			section["UiLanguage"] = uiCulture;

		// SMTP (system.net/mailSettings/smtp)
		XElement? smtp = ResolveConfigSource(configuration.Element("system.net")?.Element("mailSettings"), baseDirectory)?.Element("smtp");
		if (smtp != null)
		{
			section["SmtpFrom"] = GetString(smtp, "from", "signup@roadkillwiki.net");

			if (GetString(smtp, "deliveryMethod", "Network").Equals("SpecifiedPickupDirectory", StringComparison.OrdinalIgnoreCase))
			{
				section["SmtpPickupDirectory"] = GetString(smtp.Element("specifiedPickupDirectory"), "pickupDirectoryLocation", "~/App_Data/TempSmtp");
			}
			else
			{
				XElement? network = smtp.Element("network");
				section["SmtpHost"] = GetString(network, "host", "");
				section["SmtpPort"] = int.TryParse(GetString(network, "port", "25"), out int port) ? port : 25;
				section["SmtpUsername"] = GetString(network, "userName", "");
				section["SmtpPassword"] = GetString(network, "password", "");
				section["SmtpEnableSsl"] = GetBool(network, "enableSsl", false);
			}
		}

		return new JsonObject
		{
			["ConnectionStrings"] = new JsonObject { ["Roadkill"] = connectionString },
			["Roadkill"] = section
		};
	}

	static string ConvertDatabaseName(string databaseName, List<string> warnings)
	{
		if (databaseName == "" || databaseName.StartsWith("SqlServer", StringComparison.OrdinalIgnoreCase) || databaseName.StartsWith("SqlAzure", StringComparison.OrdinalIgnoreCase))
			return "SqlServer";

		if (databaseName.StartsWith("Postgres", StringComparison.OrdinalIgnoreCase))
			return "Postgres";

		if (databaseName.Equals("MongoDB", StringComparison.OrdinalIgnoreCase))
			return "MongoDB";

		warnings.Add($"The database '{databaseName}' is no longer supported (only SQL Server, Postgres and MongoDB are).");
		return databaseName;
	}

	static XElement? ResolveConfigSource(XElement? element, string baseDirectory)
	{
		string? configSource = (string?)element?.Attribute("configSource");
		if (string.IsNullOrEmpty(configSource))
			return element;

		string path = Path.GetFullPath(Path.Combine(baseDirectory, configSource.Replace('\\', Path.DirectorySeparatorChar)));
		if (!FilesRead.Contains(path))
			FilesRead.Add(path);

		return XDocument.Load(path).Root;
	}

	static string GetString(XElement? element, string attributeName, string defaultValue)
	{
		XAttribute? attribute = element?.Attributes().FirstOrDefault(x => x.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase));
		return attribute?.Value ?? defaultValue;
	}

	static bool GetBool(XElement? element, string attributeName, bool defaultValue)
	{
		return bool.TryParse(GetString(element, attributeName, ""), out bool result) ? result : defaultValue;
	}
}
