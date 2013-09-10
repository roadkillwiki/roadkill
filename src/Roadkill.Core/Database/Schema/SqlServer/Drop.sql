IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[roadkill_pagecontent_fk_pageid]') AND parent_object_id = OBJECT_ID(N'[dbo].[roadkill_pagecontent]'))
ALTER TABLE [dbo].[roadkill_pagecontent] DROP CONSTRAINT [FK_roadkill_pageid];

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[roadkill_pagecontent]') AND type in (N'U'))
DROP TABLE [dbo].[roadkill_pagecontent];

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[roadkill_pages]') AND type in (N'U'))
DROP TABLE [dbo].[roadkill_pages];

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[roadkill_users]') AND type in (N'U'))
DROP TABLE [dbo].[roadkill_users];

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[roadkill_siteconfiguration]') AND type in (N'U'))
DROP TABLE [dbo].[roadkill_siteconfiguration];