// Converts the settings of a Roadkill 2.x web.config into the appsettings.json format used by Roadkill 3 (.NET 10).
//
// Usage (.NET 10 SDK, no project needed):
//   dotnet run ConvertWebConfig.cs -- <path to web.config> [path to appsettings.json to update]
//
// Without the second argument the JSON is printed. With it, the "ConnectionStrings:Roadkill" value and the "Roadkill"
// section of that file are replaced, and its other settings are kept.
// The configSource files (e.g. configSource="Roadkill.config") are read from the web.config's folder.
// The same code is in ConvertWebConfig.linq, for LINQPad.

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

if (args.Length < 1)
{
	Console.Error.WriteLine("Usage: dotnet run ConvertWebConfig.cs -- <web.config> [appsettings.json]");
	return 1;
}

var warnings = new List<string>();
JsonObject converted = WebConfigConverter.Convert(args[0], warnings);

foreach (string warning in warnings)
	Console.Error.WriteLine("WARNING: " + warning);

var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

if (args.Length < 2)
{
	Console.WriteLine(converted.ToJsonString(jsonOptions));
	return 0;
}

string appSettingsPath = args[1];
JsonObject root = new JsonObject();
if (File.Exists(appSettingsPath))
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

File.WriteAllText(appSettingsPath, root.ToJsonString(jsonOptions));
Console.WriteLine($"Updated {appSettingsPath}");
return 0;

static class WebConfigConverter
{
	public static JsonObject Convert(string webConfigPath, List<string> warnings)
	{
		string baseDirectory = Path.GetDirectoryName(Path.GetFullPath(webConfigPath))!;
		XElement configuration = XDocument.Load(webConfigPath).Root!;

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
		else if ((string)section["DatabaseName"]! == "SqlServer2008" &&
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
			return "SqlServer2008";

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

		string path = Path.Combine(baseDirectory, configSource.Replace('\\', Path.DirectorySeparatorChar));
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
