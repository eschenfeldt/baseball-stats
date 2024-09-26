# baseball-stats
Webapp displaying statistics from baseball games

### Current build/run process

1. Build docker containers locally for multiple platforms with the following command.
```
docker buildx bake --set "*.platform=linux/amd64,linux/arm64"
```
2. Push `baseball-app` to docker hub private repository with this command
```
docker push eschenfeldt/baseball-app
```
3. SSH into server and pull latest on `~/source/baseball-stats`
4. Run `docker compose down` to stop running containers
5. Remove any images that need to be updated: `docker image rm <image name> --force`
6. Run `docker compose up -d`, which will pull `baseball-app` from docker hub and build the other two containers. Trying to build the angular project on the server has overloaded it. (Omit the `-d` to remain attached and see logs.)

#### TODO build/run steps

- Better secret management and/or CD
