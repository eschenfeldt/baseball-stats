# baseball-stats
Webapp displaying statistics and photos from baseball games. Live version at https://baseball.eschenfeldt.me

### Running the tests

All three suites run in CI (`.github/workflows/ci.yml`) on every pull request.

**API (.NET).** Runs in the dev container against the `db` compose service:

```
docker compose build baseball-api-dev   # after changing API source
docker compose --profile test run --remove-orphans --rm baseball-api-dev \
  dotnet test /source --filter="Category!=Media"
```

Dropping the filter includes the slow media tests (S3, ImageMagick, ffmpeg).

**Importer (Swift).** The tests create a private scratch database per test from
`iScoreImport/Tests/iScoreImportTests/Resources/schema.sql`, so nothing needs seeding —
they just need a Postgres to talk to. With the compose database up, the defaults
(`localhost:5433`, user `baseball owner`, password from `baseball_password.txt`) need no
configuration:

```
docker compose --profile test up -d db
cd iScoreImport && swift test
```

Opening `iScoreImport/Package.swift` in Xcode and hitting ⌘U does the same thing. To run
them the way CI does — containerized, in UTC, against the `db` service directly:

```
docker compose --profile swift-test run --remove-orphans --rm swift-tests
```

Each setting can be overridden by an environment variable when pointing somewhere else:
`ISCORE_TEST_PG_HOST`, `ISCORE_TEST_PG_PORT`, `ISCORE_TEST_PG_USER`,
`ISCORE_TEST_PG_ADMIN_DB`, and `ISCORE_TEST_PG_PASSWORD` or
`ISCORE_TEST_PG_PASSWORD_FILE`. If no password resolves, the database-backed suites skip
rather than fail.

#### Keeping schema.sql current

The Swift tests build their scratch databases from a committed copy of the API's schema.
After adding an EF migration, regenerate it — the `Swift Test Schema Up To Date` CI job
fails otherwise:

```
dotnet ef migrations script --idempotent --context BaseballContext \
  --project BaseballApi/Api --output iScoreImport/Tests/iScoreImportTests/Resources/schema.sql
```

### Running an import

`iScoreImport` is a Swift command-line tool that copies teams, players, and games out of
an iScore SQLite export into the app's database. Imports are ad-hoc: the source file has
to be pulled out of the iOS app by hand first.

```
cd iScoreImport
swift run -c release iScoreImport \
  --source-file ~/Programming/Scorecards/data/iScoreDbs/2026-05-07/iScore_baseball.sqlite \
  --target-config config/prod.postgres_config \
  --included-types Games \
  --since 2025-08-01
```

`--included-types` takes any of `Teams`, `Players`, `Games` and may be repeated; `--since`
(`yyyy-MM-dd`) only applies to games. `swift run iScoreImport --help` lists everything.

Connection details come from a JSON config file. `*.postgres_config` is gitignored, since
these hold live credentials, so create `config/prod.postgres_config` yourself:

```json
{
    "hostname": "",
    "database": "",
    "username": "",
    "password": "",
    "port": 5432
}
```

`port` is optional and defaults to 5432.

### Deployment

Deploys happen automatically on merge to `main` via the **Deploy** workflow
(`.github/workflows/deploy.yml`): a GitHub-hosted runner joins the tailnet, SSHes
to the server (authenticated by Tailscale SSH — no keys), and runs

```
git pull --ff-only
export GIT_SHA="$(git rev-parse --short HEAD)"  # stamped into telemetry as service.version
docker compose --profile prod down
docker compose --profile prod up -d --build   # --build rebuilds all prod images from the pulled source
docker image prune -f                          # reclaim the now-dangling old images
```

This depends on the runner reaching the tailnet (`tag:ci-cd`) and Tailscale SSH
being enabled on the server for the deploy user.

Database migrations are **not** applied by the deploy. Apply any new migration to
production manually *before* merging its PR — the **Migration gate** workflow
(`.github/workflows/migration-gate.yml`) is a required status check that blocks
the merge until every migration in the PR is present in production's
`__EFMigrationsHistory`. The **CD Health** check (`.github/workflows/cd-health.yml`)
runs on every PR to confirm the deployment path (tailnet, Tailscale SSH, and
read-only DB access) still works before you rely on it at merge time.

#### Manual deploy (fallback)

1. SSH into server and pull latest on `~/source/baseball-stats`
2. Run `docker compose --profile prod down` to stop running containers
3. Remove any images that need to be updated: `docker image rm <image name> --force`
4. Optionally `export GIT_SHA="$(git rev-parse --short HEAD)"` so telemetry
   reports the right `service.version`; without it the build falls back to the
   assembly version. Likewise optionally
   `export HOST_RESOURCE_ATTRS="$(sh .github/ci/otel-resource-attributes.sh)"`;
   without it this deploy's telemetry has no host attribution (fine in a
   pinch — Grafana's host views just show a gap until the next normal deploy).
5. Run `docker compose --profile prod up -d`, which builds the containers. (Omit the `-d` to remain attached and see logs.)
   

#### Telemetry (OpenTelemetry → Grafana Cloud)

The API exports logs, traces, and metrics over OTLP when enabled. Configuration
lives in the server-side `BaseballApi/appsettings.Production.json` alongside the
other production secrets (locally: the same keys in user secrets; leave 
`OTEL_EXPORTER_OTLP_ENDPOINT` unset to keep telemetry off):

```json
{
  "OTEL_EXPORTER_OTLP_ENDPOINT": "https://otlp-gateway-prod-us-east-2.grafana.net/otlp",
  "OTEL_EXPORTER_OTLP_PROTOCOL": "http/protobuf",
  "OTEL_EXPORTER_OTLP_HEADERS": "Authorization=Basic <base64 of instanceId:token>"
}
```

The flat `OTEL_*` keys are read by the OTLP exporter directly from
`IConfiguration`, so they work from any config source. Notes:

- The Grafana Cloud OTLP gateway only supports `http/protobuf` (the SDK
  defaults to gRPC, so the protocol key is required).
- The auth header is HTTP Basic: base64 of `<instanceId>:<token>`, where the
  numeric instance ID comes from the cloud portal's **OpenTelemetry →
  Configure** page (it differs from the Prometheus/Loki/Tempo instance IDs)
  and the token is a Cloud Access Policy token with `metrics:write`,
  `logs:write`, and `traces:write` scopes.

Host/cloud resource attributes are not in this config. `deploy.yml` reads the
DigitalOcean droplet's [metadata service](https://docs.digitalocean.com/reference/api/metadata/)
(`http://169.254.169.254/metadata/v1`) on the box and assembles
`OTEL_RESOURCE_ATTRIBUTES` (`cloud.provider`, `cloud.region`, `host.id`,
`host.name`, `grafana.host.id`), which the SDK merges into the resource. This
keeps the app and `compose.yaml` cloud-agnostic. Grafana Cloud associates
telemetry with a host via `grafana.host.id` (and `host.name` + `cloud.provider`);
plain `host.id` is ignored by Grafana but kept to match OTel conventions. If the
metadata service is unavailable, deploy falls back to the systemd machine id and
emits only `host.name`/`host.id`/`grafana.host.id` (no `cloud.*`).

Dashboards for consuming this telemetry are committed in
[`docs/observability/dashboards/`](docs/observability/dashboards/) — import the
JSON into Grafana; the folder README covers importing, the metric-name mapping,
and known quirks.
