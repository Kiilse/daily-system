-- Bootstrap of the product databases and their roles (ADR-006). Run once, as a superuser, on an empty
-- instance: locally by the postgres image (mounted in /docker-entrypoint-initdb.d/), on Railway by #10.
--
-- Per database {db}:
--   {db}_migrator  owns the database and its public schema, runs the migrations (DDL)
--   {db}_app       used by the running service: SELECT, INSERT, UPDATE, DELETE only, owns nothing
-- No other role can connect: CONNECT and TEMPORARY are revoked from PUBLIC.
--
-- Passwords are never written here. psql reads them from environment variables named after the role
-- in upper case: MENU_DB_APP_PASSWORD, MENU_DB_MIGRATOR_PASSWORD... (see infra/.env.example).
-- \getenv needs psql 15 or later.

\set ON_ERROR_STOP on

\getenv bff_db_migrator_password BFF_DB_MIGRATOR_PASSWORD
\getenv bff_db_app_password BFF_DB_APP_PASSWORD
\getenv account_db_migrator_password ACCOUNT_DB_MIGRATOR_PASSWORD
\getenv account_db_app_password ACCOUNT_DB_APP_PASSWORD
\getenv menu_db_migrator_password MENU_DB_MIGRATOR_PASSWORD
\getenv menu_db_app_password MENU_DB_APP_PASSWORD
\getenv calendar_db_migrator_password CALENDAR_DB_MIGRATOR_PASSWORD
\getenv calendar_db_app_password CALENDAR_DB_APP_PASSWORD

-- ===== bff_db: Data Protection keys and server-side sessions (#14) =====

CREATE ROLE bff_db_migrator LOGIN PASSWORD :'bff_db_migrator_password';
CREATE ROLE bff_db_app LOGIN PASSWORD :'bff_db_app_password';
CREATE DATABASE bff_db OWNER bff_db_migrator;
REVOKE ALL ON DATABASE bff_db FROM PUBLIC;
GRANT CONNECT ON DATABASE bff_db TO bff_db_app;

\connect bff_db
ALTER SCHEMA public OWNER TO bff_db_migrator;
REVOKE ALL ON SCHEMA public FROM PUBLIC;
GRANT USAGE ON SCHEMA public TO bff_db_app;
ALTER DEFAULT PRIVILEGES FOR ROLE bff_db_migrator IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO bff_db_app;
ALTER DEFAULT PRIVILEGES FOR ROLE bff_db_migrator IN SCHEMA public
    GRANT USAGE, SELECT ON SEQUENCES TO bff_db_app;
\connect postgres

-- ===== account_db =====

CREATE ROLE account_db_migrator LOGIN PASSWORD :'account_db_migrator_password';
CREATE ROLE account_db_app LOGIN PASSWORD :'account_db_app_password';
CREATE DATABASE account_db OWNER account_db_migrator;
REVOKE ALL ON DATABASE account_db FROM PUBLIC;
GRANT CONNECT ON DATABASE account_db TO account_db_app;

\connect account_db
ALTER SCHEMA public OWNER TO account_db_migrator;
REVOKE ALL ON SCHEMA public FROM PUBLIC;
GRANT USAGE ON SCHEMA public TO account_db_app;
ALTER DEFAULT PRIVILEGES FOR ROLE account_db_migrator IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO account_db_app;
ALTER DEFAULT PRIVILEGES FOR ROLE account_db_migrator IN SCHEMA public
    GRANT USAGE, SELECT ON SEQUENCES TO account_db_app;
\connect postgres

-- ===== menu_db =====

CREATE ROLE menu_db_migrator LOGIN PASSWORD :'menu_db_migrator_password';
CREATE ROLE menu_db_app LOGIN PASSWORD :'menu_db_app_password';
CREATE DATABASE menu_db OWNER menu_db_migrator;
REVOKE ALL ON DATABASE menu_db FROM PUBLIC;
GRANT CONNECT ON DATABASE menu_db TO menu_db_app;

\connect menu_db
ALTER SCHEMA public OWNER TO menu_db_migrator;
REVOKE ALL ON SCHEMA public FROM PUBLIC;
GRANT USAGE ON SCHEMA public TO menu_db_app;
ALTER DEFAULT PRIVILEGES FOR ROLE menu_db_migrator IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO menu_db_app;
ALTER DEFAULT PRIVILEGES FOR ROLE menu_db_migrator IN SCHEMA public
    GRANT USAGE, SELECT ON SEQUENCES TO menu_db_app;
\connect postgres

-- ===== calendar_db =====

CREATE ROLE calendar_db_migrator LOGIN PASSWORD :'calendar_db_migrator_password';
CREATE ROLE calendar_db_app LOGIN PASSWORD :'calendar_db_app_password';
CREATE DATABASE calendar_db OWNER calendar_db_migrator;
REVOKE ALL ON DATABASE calendar_db FROM PUBLIC;
GRANT CONNECT ON DATABASE calendar_db TO calendar_db_app;

\connect calendar_db
ALTER SCHEMA public OWNER TO calendar_db_migrator;
REVOKE ALL ON SCHEMA public FROM PUBLIC;
GRANT USAGE ON SCHEMA public TO calendar_db_app;
ALTER DEFAULT PRIVILEGES FOR ROLE calendar_db_migrator IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO calendar_db_app;
ALTER DEFAULT PRIVILEGES FOR ROLE calendar_db_migrator IN SCHEMA public
    GRANT USAGE, SELECT ON SEQUENCES TO calendar_db_app;
\connect postgres
