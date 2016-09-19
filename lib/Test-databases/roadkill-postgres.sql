-- SQL Manager Lite for PostgreSQL 5.7.1.47382
-- ---------------------------------------
-- Host      : localhost
-- Database  : roadkill
-- Version   : PostgreSQL 9.5.4 on x86_64-pc-linux-gnu, compiled by gcc (Debian 4.9.2-10) 4.9.2, 64-bit



SET search_path = public, pg_catalog;
DROP TABLE IF EXISTS public.roadkill_siteconfiguration;
DROP TABLE IF EXISTS public.roadkill_users;
DROP TABLE IF EXISTS public.roadkill_pagecontent;
DROP TABLE IF EXISTS public.roadkill_pages;
SET check_function_bodies = false;
--
-- Structure for table roadkill_pages (OID = 16387) : 
--
CREATE TABLE public.roadkill_pages (
    id serial NOT NULL,
    title text NOT NULL,
    tags text NOT NULL,
    createdby text NOT NULL,
    createdon timestamp(6) without time zone NOT NULL,
    islocked boolean NOT NULL,
    modifiedby text,
    modifiedon timestamp(6) without time zone
)
WITH (oids = false);
--
-- Structure for table roadkill_pagecontent (OID = 16396) : 
--
CREATE TABLE public.roadkill_pagecontent (
    id uuid NOT NULL,
    editedby text NOT NULL,
    editedon timestamp without time zone NOT NULL,
    versionnumber integer NOT NULL,
    text text,
    pageid integer NOT NULL
)
WITH (oids = false);
--
-- Structure for table roadkill_users (OID = 16404) : 
--
CREATE TABLE public.roadkill_users (
    id uuid NOT NULL,
    activationkey text,
    email text NOT NULL,
    firstname text,
    lastname text,
    iseditor boolean NOT NULL,
    isadmin boolean NOT NULL,
    isactivated boolean NOT NULL,
    password text NOT NULL,
    passwordresetkey text,
    salt text NOT NULL,
    username text NOT NULL
)
WITH (oids = false);
--
-- Structure for table roadkill_siteconfiguration (OID = 16412) : 
--
CREATE TABLE public.roadkill_siteconfiguration (
    id uuid NOT NULL,
    version text NOT NULL,
    content text NOT NULL
)
WITH (oids = false);
--
-- Data for table public.roadkill_users (OID = 16404) (LIMIT 0,2)
--
INSERT INTO roadkill_users (id, activationkey, email, firstname, lastname, iseditor, isadmin, isactivated, password, passwordresetkey, salt, username)
VALUES ('aabd5468-1c0e-4277-ae10-a0ce00d2fefc', '', 'admin@localhost', 'Chris', 'Admin', false, true, true, 'C882A7933951FCC4197718B104AECC53564FC205', '', 'J::]T!>k5LR|.{U9', 'admin');

INSERT INTO roadkill_users (id, activationkey, email, firstname, lastname, iseditor, isadmin, isactivated, password, passwordresetkey, salt, username)
VALUES ('4d0bc016-1d47-4ad3-a6fe-a11a013ef9c8', '3d12daea-16d0-4bd6-9e0c-347f14e0d97d', 'editor@localhost', '', '', true, false, true, '7715C929E99254C117657B0937E97926443FDAF6', '', 'fO)M`*QU:eH''Xl_%', 'editor');

--
-- Data for table public.roadkill_siteconfiguration (OID = 16412) (LIMIT 0,8)
--
INSERT INTO roadkill_siteconfiguration (id, version, content)
VALUES ('b960e8e5-529f-4f7c-aee4-28eb23e13dbd', '2.0.0.0', '{
  "AllowedFileTypes": "jpg,png,gif,zip,xml,pdf",
  "AllowUserSignup": true,
  "IsRecaptchaEnabled": false,
  "MarkupType": "Creole",
  "RecaptchaPrivateKey": "recaptcha-private-key",
  "RecaptchaPublicKey": "recaptcha-public-key",
  "SiteUrl": "http://localhost:9876",
  "SiteName": "Acceptance Tests",
  "Theme": "Responsive",
  "OverwriteExistingFiles": false,
  "HeadContent": "",
  "MenuMarkup": "* %mainpage%\\r\\n* %categories%\\r\\n* %allpages%\\r\\n* %newpage%\\r\\n* %managefiles%\\r\\n* %sitesettings%\\r\\n\\r\\n",
  "PluginLastSaveDate": "2013-12-28T16:00:54.408505Z"
}');

INSERT INTO roadkill_siteconfiguration (id, version, content)
VALUES ('8050978c-80fb-0000-0000-000000000000', '1.0', '{
  "PluginId": "ClickableImages",
  "Version": "1.0",
  "IsEnabled": false,
  "Values": []
}');

INSERT INTO roadkill_siteconfiguration (id, version, content)
VALUES ('208af9dc-80fb-0000-0000-000000000000', '1.0', '{
  "PluginId": "ResizeImages",
  "Version": "1.0",
  "IsEnabled": false,
  "Values": []
}');

INSERT INTO roadkill_siteconfiguration (id, version, content)
VALUES ('b35f5545-80fb-0000-0000-000000000000', '1.0', '{
  "PluginId": "ExternalLinksInNewWindow",
  "Version": "1.0",
  "IsEnabled": false,
  "Values": []
}');

INSERT INTO roadkill_siteconfiguration (id, version, content)
VALUES ('b20d067e-80fb-0000-0000-000000000000', '1.0', '{
  "PluginId": "Jumbotron",
  "Version": "1.0",
  "IsEnabled": false,
  "Values": []
}');

INSERT INTO roadkill_siteconfiguration (id, version, content)
VALUES ('b970504f-80fb-0000-0000-000000000000', '1.0', '{
  "PluginId": "MathJax",
  "Version": "1.0",
  "IsEnabled": false,
  "Values": []
}');

INSERT INTO roadkill_siteconfiguration (id, version, content)
VALUES ('598fdb04-80fb-0000-0000-000000000000', '1.0', '{
  "PluginId": "ToC",
  "Version": "1.0",
  "IsEnabled": false,
  "Values": []
}');

INSERT INTO roadkill_siteconfiguration (id, version, content)
VALUES ('92e641c4-80fb-0000-0000-000000000000', '1.0', '{
  "PluginId": "SyntaxHighlighter",
  "Version": "1.0",
  "IsEnabled": false,
  "Values": []
}');

--
-- Definition for index roadkill_pages_pkey (OID = 16394) : 
--
ALTER TABLE ONLY roadkill_pages
    ADD CONSTRAINT roadkill_pages_pkey
    PRIMARY KEY (id);
--
-- Definition for index roadkill_pagecontent_pkey (OID = 16402) : 
--
ALTER TABLE ONLY roadkill_pagecontent
    ADD CONSTRAINT roadkill_pagecontent_pkey
    PRIMARY KEY (id);
--
-- Definition for index roadkill_users_pkey (OID = 16410) : 
--
ALTER TABLE ONLY roadkill_users
    ADD CONSTRAINT roadkill_users_pkey
    PRIMARY KEY (id);
--
-- Definition for index roadkill_siteconfiguration_pkey (OID = 16418) : 
--
ALTER TABLE ONLY roadkill_siteconfiguration
    ADD CONSTRAINT roadkill_siteconfiguration_pkey
    PRIMARY KEY (id);
--
-- Definition for index roadkill_siteconfiguration_content_key (OID = 16420) : 
--
ALTER TABLE ONLY roadkill_siteconfiguration
    ADD CONSTRAINT roadkill_siteconfiguration_content_key
    UNIQUE (content);
--
-- Data for sequence public.roadkill_pages_id_seq (OID = 16385)
--
SELECT pg_catalog.setval('roadkill_pages_id_seq', 1, false);
--
-- Comments
--
COMMENT ON SCHEMA public IS 'standard public schema';
