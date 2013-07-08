DROP TABLE IF EXISTS roadkill_siteconfiguration;
CREATE TABLE roadkill_siteconfiguration 
(
  "id" UUID NOT NULL, 
  "version" TEXT NOT NULL, 
  "content" TEXT NOT NULL UNIQUE, 
  PRIMARY KEY("id")
);