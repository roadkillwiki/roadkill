CREATE TABLE roadkill_pages 
(
	[id] integer primary key autoincrement, 
	[title] TEXT, 
	[tags] TEXT, 
	[createdby] TEXT, 
	[createdon] DATETIME, 
	[islocked] BOOL, 
	[modifiedby] TEXT, 
	[modifiedon] DATETIME
);

CREATE TABLE roadkill_pagecontent 
(
	[id] CHAR(36) not null, 
	[editedby] TEXT, 
	[editedon] DATETIME, 
	[versionnumber] INT, 
	[text] NTEXT, 
	[pageid] INT, 
	PRIMARY KEY (Id)
	/*,constraint fk_roadkillpageid foreign key (pageid) references roadkill_pages*/
);

CREATE INDEX pageid on roadkill_pagecontent (pageid);

CREATE TABLE roadkill_users 
(
	[id] CHAR(36) not null, 
	[activationkey] TEXT, 
	[email] NTEXT, 
	[firstname] NTEXT, 
	[iseditor] BOOL, 
	[isadmin] BOOL, 
	[isactivated] BOOL, 
	[lastname] NTEXT, 
	[password] NTEXT, 
	[passwordresetkey] NTEXT, 
	[salt] NTEXT, 
	[username] NTEXT, 
	PRIMARY KEY (Id)
);

CREATE INDEX email on roadkill_users (email);

CREATE TABLE [roadkill_siteconfiguration] 
(
  [id] CHAR(36) NOT NULL, 
  [version] TEXT, 
  [content] NTEXT, 
  PRIMARY KEY (Id)
);
