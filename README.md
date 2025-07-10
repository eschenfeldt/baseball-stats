# baseball-stats
Webapp displaying statistics and photos from baseball games. Live version at https://baseball.eschenfeldt.me

### Current build/run process

1. SSH into server and pull latest on `~/source/baseball-stats`
2. Run `docker compose --profile prod down` to stop running containers
3. Remove any images that need to be updated: `docker image rm <image name> --force`
4. Run `docker compose --profile prod up -d`, which build the containers. (Omit the `-d` to remain attached and see logs.)
