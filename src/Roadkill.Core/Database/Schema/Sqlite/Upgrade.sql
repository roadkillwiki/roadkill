DROP TABLE IF EXISTS roadkill_siteconfiguration;
CREATE TABLE [roadkill_siteconfiguration] 
(
  [id] UNIQUEIDENTIFIER NOT NULL, 
  [version] TEXT, 
  [content] NTEXT, 
  PRIMARY KEY (Id)
);