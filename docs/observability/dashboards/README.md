# Grafana dashboards

Repo-committed dashboards for the telemetry the API exports (see the Telemetry
section of the top-level README for how that telemetry gets to Grafana Cloud).
These are the intended starting point for consuming it — the auto-generated
Grafana views mostly don't know what our custom metrics mean; these boards do,
and each panel's description (the ⓘ on hover) explains how to read it and its
known quirks.

| File | Board | Answers |
| --- | --- | --- |
| `api-health.json` | Baseball API — Service Health | Is the site healthy? Traffic, 5xx, latency, GC memory and thread-pool pressure, recent error logs. |
| `background-jobs.json` | Baseball API — Background Jobs | Did the jobs run and succeed? Is the media import pipeline moving? Is reference matching healthy, and what's fixable? |

## Importing

Grafana → **Dashboards → New → Import → Upload dashboard JSON file**. The
boards don't hardcode datasource ids; after import, pick your stack's
Prometheus and Loki datasources in the **Metrics** / **Logs** dropdowns at the
top of the board (the choice sticks once the dashboard is saved). The
**Service** dropdown self-populates from `target_info` and should offer
`baseball-api`.

If you edit a board in the Grafana UI and want to keep the change, export it
(**Share → Export → Save to file**) and commit the JSON back here — this folder
is the source of truth, Grafana is just where it runs.

## If a panel is empty

1. **Check the exact metric name.** OTLP names get translated on ingest
   (dots → underscores, counters get `_total`, base units appended), and the
   translation has changed across Grafana releases. Explore → Prometheus →
   metric browser, search for the prefix (`media_import`, `reference_update`,
   `http_server`, `dotnet`), and fix the query to match. The mapping the boards
   assume:

   | In code (`Observability/`) | Assumed Grafana name |
   | --- | --- |
   | `media_import.outcomes` (counter, tag `status`) | `media_import_outcomes_total` |
   | `media_import.queue.depth` (gauge) | `media_import_queue_depth` |
   | `reference_update.player_match` (counter, tag `outcome`) | `reference_update_player_match_total` |
   | `reference_update.players` (gauge, tag `matched`) | `reference_update_players` |
   | `reference_update.ambiguous_name_groups` (gauge) | `reference_update_ambiguous_name_groups` |
   | `reference_update.fixable_unmatched` (gauge) | `reference_update_fixable_unmatched` |

   Runtime metrics: on .NET 8 the `OpenTelemetry.Instrumentation.Runtime`
   package emits the old `process.runtime.dotnet.*` names (→
   `process_runtime_dotnet_gc_heap_size_bytes`,
   `process_runtime_dotnet_exceptions_count_total`, …); the newer `dotnet.*`
   names only exist on .NET 9+. There is **no CPU or working-set metric** on
   .NET 8 — that would need the (beta) `OpenTelemetry.Instrumentation.Process`
   package or a .NET 9 upgrade, at which point the memory/saturation panels
   here can be upgraded too.

2. **Resource attributes show up as labels on `target_info`**, not on every
   series — query `target_info{job="baseball-api"}` in Explore (the same `job`
   label the boards' Service dropdown filters on) to see what the server is
   actually sending (service version, host attributes).
   This is the first stop when telemetry looks misattributed rather than
   missing.

3. **The job panels need span metrics, and span metrics need CONSUMER spans.**
   They're built on `traces_spanmetrics_calls_total` /
   `traces_spanmetrics_latency`, which Grafana Cloud generates server-side
   (enabled under Application Observability) — but **only for SERVER and
   CONSUMER span kinds** (INTERNAL requires a Grafana support ticket). The
   `job.*` spans are emitted as CONSUMER via `Telemetry.StartJob` for exactly
   this reason; runs recorded before that change deployed were INTERNAL and
   never generated metrics, so the panels only have data from the deploy
   onward. Debugging order: spans in Tempo (Explore, `{name =~ "job\\..*"}`)
   → span kind on the span → `traces_spanmetrics_calls_total{span_name=~"job.*"}`
   in the metric browser.

4. **Gauges are silent right after a deploy.** The `reference_update.*` gauges
   report nothing until the first player run completes (~30+ minutes after
   start), which is why those panels query `last_over_time(...[2h])`. The
   queue-depth gauge resets to zero instead (in-memory queue).

## Known metric quirks (by design, don't "fix" the data)

- `media_import.outcomes{status="skipped"}` is mostly duplicate noise from the
  hourly retrigger job re-queueing an already-running task — alert-worthy
  signal lives in `failed`, not `skipped`.
- Queue depth counts queue *entries*, not remaining work: one multi-hour
  import shows a +1/hour staircase (retrigger duplicates) that drains at once.
  Read it as "work pending: yes/no".
- Job spans deliberately don't record shutdown cancellations as errors, so a
  deploy mid-job doesn't show as a failure.
