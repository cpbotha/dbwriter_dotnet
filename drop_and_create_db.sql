-- as user postgres:
-- psql < drop_and_create_db.sql
-- on macos with homebrew postgres, just do: psql postgres < drop_and_create_db.sql
create user dbwriter with password 'blehbleh';
drop database dbwriter_dotnet;
create database dbwriter_dotnet;
grant all privileges on database dbwriter_dotnet to dbwriter;
