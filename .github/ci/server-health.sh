#!/usr/bin/env sh
#
# Read-only health check of the deploy target, run over SSH by cd-health.yml to
# validate the CD path before a real deploy: confirms the SSH login, that docker
# compose is available, the repo checkout's state, and that the production app
# settings file is present (its absence would fail the image build during deploy).
#
# Piped to the droplet over stdin (ssh ... 'sh -s' < this), so the version that
# runs is the PR's copy, not whatever is currently deployed.
set -eu

echo "Logged in as:   $(whoami)"
echo "docker compose: $(docker compose version --short)"
cd ~/source/baseball-stats
echo "Repo HEAD:      $(git rev-parse --short HEAD) on $(git rev-parse --abbrev-ref HEAD)"
git status --short --branch
if [ -f BaseballApi/appsettings.Production.json ]; then
    echo "prod config:    present"
else
    echo "prod config:    MISSING — BaseballApi/appsettings.Production.json (build would fail)"
    exit 1
fi
