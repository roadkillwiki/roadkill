-- You will need to enable identity inserts for your chosen db before running this Script, for example in SQL Server:
-- SET IDENTITY_INSERT roadkill_pages ON;

-- Users


-- Configuration
INSERT INTO roadkill_siteconfiguration (id, version, content) VALUES ('b960e8e5-529f-4f7c-aee4-28eb23e13dbd','{AppVersion}','{
  "AllowedFileTypes": ".exe,.vbscript",
  "AllowUserSignup": false,
  "IsRecaptchaEnabled": false,
  "MarkupType": "Creole",
  "RecaptchaPrivateKey": "",
  "RecaptchaPublicKey": "",
  "SiteUrl": "",
  "SiteName": "Your site",
  "Theme": "Mediawiki",
  "OverwriteExistingFiles": false,
  "HeadContent": "",
  "MenuMarkup": "markup ```'''''' \r\n",
  "PluginLastSaveDate": "2013-11-09T00:00:00"
}');
INSERT INTO roadkill_siteconfiguration (id, version, content) VALUES ('7f4f72e9-cbc4-0000-0000-000000000000','1.1','{
  "PluginId": "fake-plugin1",
  "Version": "1.1",
  "IsEnabled": true,
  "Values": [
    {
      "Name": "key1",
      "Value": "value1",
      "FormType": 0
    },
    {
      "Name": "key2",
      "Value": "value2",
      "FormType": 0
    }
  ]
}');
INSERT INTO roadkill_siteconfiguration (id, version, content) VALUES ('dd2072e9-cbc3-0000-0000-000000000000','2.1','{
  "PluginId": "fake-plugin2",
  "Version": "2.1",
  "IsEnabled": false,
  "Values": []
}');
