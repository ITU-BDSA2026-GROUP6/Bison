#!/usr/bin/env bash
DB="${BISONDBPATH:-$(dirname $(mktemp -u))/bison.db}"
sqlite3 "$DB" < data/schema.sql && sqlite3 "$DB" < data/dump.sql