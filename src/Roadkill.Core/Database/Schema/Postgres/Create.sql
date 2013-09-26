CREATE TABLE roadkill_pages
(
  "id" SERIAL NOT NULL, 
  "title" TEXT NOT NULL, 
  "tags" TEXT NOT NULL, 
  "createdby" TEXT NOT NULL, 
  "createdon" TIMESTAMP(20) WITHOUT TIME ZONE NOT NULL, 
  "islocked" BOOLEAN NOT NULL, 
  "modifiedby" TEXT, 
  "modifiedon" TIMESTAMP(20) WITHOUT TIME ZONE, 
  PRIMARY KEY("id")
);

CREATE TABLE roadkill_pagecontent
(
  "id" UUID NOT NULL, 
  "editedby" TEXT NOT NULL, 
  "editedon" TIMESTAMP WITHOUT TIME ZONE NOT NULL, 
  "versionnumber" INTEGER NOT NULL, 
  "text" TEXT, 
  "pageid" INTEGER NOT NULL, 
  PRIMARY KEY("id")
);

CREATE TABLE roadkill_users
(
  "id" UUID NOT NULL, 
  "activationkey" TEXT, 
  "email" TEXT NOT NULL, 
  "firstname" TEXT, 
  "lastname" TEXT, 
  "iseditor" BOOLEAN NOT NULL, 
  "isadmin" BOOLEAN NOT NULL, 
  "isactivated" BOOLEAN NOT NULL, 
  "password" TEXT NOT NULL, 
  "passwordresetkey" TEXT, 
  "salt" TEXT NOT NULL, 
  "username" TEXT NOT NULL, 
  PRIMARY KEY("id")
);

CREATE TABLE roadkill_siteconfiguration 
(
  "id" UUID NOT NULL, 
  "version" TEXT NOT NULL, 
  "content" TEXT NOT NULL UNIQUE, 
  PRIMARY KEY("id")
);