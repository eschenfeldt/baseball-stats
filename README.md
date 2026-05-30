# baseball-stats
Webapp displaying statistics and photos from baseball games. Live version at https://baseball.eschenfeldt.me

### Deployment

Deploys happen automatically on merge to `main` via the **Deploy** workflow
(`.github/workflows/deploy.yml`): a GitHub-hosted runner joins the tailnet, SSHes
to the server (authenticated by Tailscale SSH — no keys), and runs

```
git pull --ff-only
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
4. Run `docker compose --profile prod up -d`, which build the containers. (Omit the `-d` to remain attached and see logs.)
