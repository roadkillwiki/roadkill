-- You will need to enable identity inserts for your chosen db before running this Script, for example in SQL Server:
-- SET IDENTITY_INSERT roadkill_pages ON;

-- Users


-- Pages
INSERT INTO roadkill_pages (id, title, createdby, createdon, modifiedby, modifiedon, tags, islocked) VALUES ('1','Page 1 title','created-by-user1','2013-01-01 12:00:00','modified-by-user2','2013-01-01 13:00:00','tag1,tag2,tag3','1');
INSERT INTO roadkill_pages (id, title, createdby, createdon, modifiedby, modifiedon, tags, islocked) VALUES ('2','Page 2 title','created-by-user2','2013-01-02 12:00:00','modified-by-user2','2013-01-02 13:00:00','tagA,tagB,tagC','1');
INSERT INTO roadkill_pages (id, title, createdby, createdon, modifiedby, modifiedon, tags, islocked) VALUES ('3','Page 3 title','created-by-user3','2013-01-03 12:00:00','modified-by-user3','2013-01-03 13:00:00','tagX,tagY,tagZ','0');

-- Pages contents
INSERT INTO roadkill_pagecontent (id, pageid, text, editedby, editedon, versionnumber) VALUES ('13a8ad19-b203-46f5-be10-11e0ebf6f812','1','the text ;''''''


								" more text "','modified-by-user1','2013-01-01 13:00:00','1');
INSERT INTO roadkill_pagecontent (id, pageid, text, editedby, editedon, versionnumber) VALUES ('143b0023-329a-49b9-97a4-5094a0e378a2','2','the text ;'''''' #### sdfsdfsdf ####


								" blah text "','modified-by-user2','2013-01-02 13:00:00','1');
INSERT INTO roadkill_pagecontent (id, pageid, text, editedby, editedon, versionnumber) VALUES ('15ee19ef-c093-47de-97d2-83dec406d92d','3','the text ;'''''' #### dddd **dddd** ####			
			

								" pppp text "','modified-by-user3','2013-01-03 13:00:00','1');
