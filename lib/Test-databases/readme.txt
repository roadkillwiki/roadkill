The database files in this folder all have schema and data in them, used for testing.

SQL Server 2012 LocalDB is now used as the dev and acceptance test server - SQLCE won't be supported as of version 2.0.

To get LocalDB working in a dev environment:

- Open a command prompt
- Type SqlLocalDB create Roadkill -s
- Open SQL Server Management Studio
- Connect to "(LocalDB)\Roadkill"
- Open and execute the Roadkill-sqlserver-localdb.sql
- Copy and replace the configs from the Lib/Configs folder into the Site root.





