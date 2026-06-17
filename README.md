# baseball-stats
Webapp displaying statistics and photos from baseball games. Live version at https://baseball.eschenfeldt.me

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
   assembly version.
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
