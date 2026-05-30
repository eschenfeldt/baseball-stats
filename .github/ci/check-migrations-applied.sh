#!/usr/bin/env bash
#
# Verify that every EF Core migration present in the repo has already been
# applied to a target database's __EFMigrationsHistory table.
#
# Used by the migration-gate workflow as a required status check: a PR that adds
# a migration cannot merge into main until that migration has been applied to
# production. This keeps the auto-deploy (which ships code expecting the new
# schema) from racing ahead of the database.
#
# Usage:
#   check-migrations-applied.sh <migrations-dir> <psql-conn-uri>
#
#   <migrations-dir>  Directory holding a context's migrations (non-recursive).
#   <psql-conn-uri>   libpq connection URI, e.g.
#                     postgresql://user:pass@host:5432/dbname
#
set -euo pipefail

if [ "$#" -ne 2 ]; then
    echo "usage: $0 <migrations-dir> <psql-conn-uri>" >&2
    exit 2
fi

migrations_dir="$1"
conn="$2"

if [ ! -d "$migrations_dir" ]; then
    echo "migrations dir not found: $migrations_dir" >&2
    exit 2
fi

# Migration IDs declared in the repo. Each migration is a non-designer,
# non-snapshot .cs file directly in the dir; its MigrationId is the file name
# without the .cs extension (e.g. 20251115063527_ReferencePlayer). -maxdepth 1
# keeps the BaseballContext scan from recursing into the AppIdentityDb subdir.
repo_ids="$(find "$migrations_dir" -maxdepth 1 -type f -name '*.cs' \
    ! -name '*.Designer.cs' ! -name '*ModelSnapshot.cs' \
    -exec basename {} .cs \; | sort)"

# Migration IDs already applied in the target database.
applied_ids="$(psql "$conn" --no-align --tuples-only \
    --command 'SELECT "MigrationId" FROM "__EFMigrationsHistory";' | sort)"

# Anything declared in the repo but not yet applied blocks the merge.
missing="$(comm -23 <(printf '%s\n' "$repo_ids") <(printf '%s\n' "$applied_ids"))"

if [ -n "$missing" ]; then
    echo "::error::Migrations in $migrations_dir are NOT yet applied to the target database:"
    printf '  - %s\n' $missing >&2
    echo "Apply them to production before merging this PR." >&2
    exit 1
fi

count="$(printf '%s\n' "$repo_ids" | grep -c . || true)"
echo "OK: all $count migration(s) in $migrations_dir are applied to the target database."
