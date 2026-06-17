#!/usr/bin/env sh
#
# Emit the OpenTelemetry resource attributes describing the host this runs on, as
# a comma-separated OTEL_RESOURCE_ATTRIBUTES value on stdout.
#
# Prod runs on a DigitalOcean droplet; this mirrors the collector
# resourcedetectionprocessor's "digitalocean" detector by reading the droplet
# metadata service (link-local 169.254.169.254, served by the hypervisor) and
# emitting cloud.provider / cloud.region / host.id / host.name. We also set
# grafana.host.id (= droplet id): Grafana Cloud associates telemetry with a host
# via grafana.host.id (and host.name + cloud.provider); plain host.id is ignored
# by Grafana but kept to match OTel conventions.
#
# If the metadata service is unavailable (any field empty), fall back to the
# systemd machine id so host info still resolves rather than emitting a partial
# or empty attribute set. Keeping this logic here keeps the app and compose.yaml
# cloud-agnostic — "digitalocean" appears only in this script.
#
# Used by deploy.yml (on the droplet, after git pull) and exercised by
# cd-health.yml (piped over SSH to the live droplet on every PR to main), so the
# metadata calls and fallback are validated before a real deploy.
set -eu

META="http://169.254.169.254/metadata/v1"

# -f drops the body on HTTP errors (so an error page can't become an attribute
# value); -sS stays quiet but still reports real failures; the tight timeouts
# keep a metadata hiccup from stalling the deploy; tr strips any trailing newline.
meta() { curl -fsS --connect-timeout 2 --max-time 5 "$1" | tr -d '\n'; }

id="$(meta "$META/id" || true)"
region="$(meta "$META/region" || true)"
name="$(meta "$META/hostname" || true)"

if [ -n "$id" ] && [ -n "$region" ] && [ -n "$name" ]; then
    printf 'cloud.provider=digitalocean,cloud.region=%s,host.id=%s,host.name=%s,grafana.host.id=%s' \
        "$region" "$id" "$name" "$id"
else
    id="$(cat /etc/machine-id)"
    printf 'host.name=%s,host.id=%s,grafana.host.id=%s' "$(hostname)" "$id" "$id"
fi
