-- You will need to enable identity inserts for your chosen db before running this Script, for example in SQL Server:
-- SET IDENTITY_INSERT roadkill_pages ON;

-- Users
INSERT INTO roadkill_users (id, activationkey, email, firstname, iseditor, isadmin, isactivated, lastname, password, passwordresetkey, salt, username) VALUES ('29a8ad19-b203-46f5-be10-11e0ebf6f812','0953cf95-f357-4e5b-ae2b-7541844d3b6b','user1@localhost','firstname1','0','1','1','lastname1','encrypted1','','salt1','user1');
INSERT INTO roadkill_users (id, activationkey, email, firstname, iseditor, isadmin, isactivated, lastname, password, passwordresetkey, salt, username) VALUES ('e63b0023-329a-49b9-97a4-5094a0e378a2','aa87fe31-9781-4c93-b7e3-9092ed095810','user2@localhost','firstname2','1','0','1','lastname2','encrypted2','','salt2','user2');
INSERT INTO roadkill_users (id, activationkey, email, firstname, iseditor, isadmin, isactivated, lastname, password, passwordresetkey, salt, username) VALUES ('a6ee19ef-c093-47de-97d2-83dec406d92d','b8ef994d-87f5-4543-85de-66b41244a20a','user3@localhost','firstname3','1','0','0','lastname3','encrypted3','','salt3','user3');
