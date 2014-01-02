DROP TABLE IF EXISTS roadkill_siteconfiguration;
CREATE TABLE roadkill_siteconfiguration
(
	Id VARCHAR(36) NOT NULL,
	Version NVARCHAR(255) NOT NULL,
	Content MEDIUMTEXT NOT NULL,
	PRIMARY KEY (Id)
);