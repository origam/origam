#!/bin/bash
psql -c "CREATE DATABASE origam  ENCODING ='UTF8'" 
psql -d origam -c "CREATE SCHEMA origam" 
psql -d origam -c "CREATE EXTENSION pgcrypto SCHEMA origam"
psql -c "CREATE USER origam WITH LOGIN NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE NOREPLICATION PASSWORD '$USER_PASSWORD'"
psql -c "GRANT CONNECT ON DATABASE origam TO origam"
psql -c "GRANT ALL PRIVILEGES ON DATABASE origam TO origam"
psql -d origam -c "GRANT ALL ON SCHEMA origam TO origam WITH GRANT OPTION"
