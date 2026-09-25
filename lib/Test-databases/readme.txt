Schema and data of a Roadkill 2.x database, used by the integration tests (they create the tables in throwaway
SQL Server and Postgres containers, see src/Roadkill.Tests/TestDatabases.cs) and to try the upgrade from 2.x:

- roadkill-sqlserver.sql: SQL Server (admin account: admin@localhost / password)
- roadkill-postgres.sql: Postgres
