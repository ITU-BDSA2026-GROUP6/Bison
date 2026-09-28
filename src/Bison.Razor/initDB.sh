#!/usr/bin/env bash
sqlite3 /bison.db < data/schema.sql
sqlite3 /bison.db < data/dump.sql
