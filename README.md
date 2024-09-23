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
4. Run `docker compose up`, which will pull `baseball-app` from docker hub and build the other two containers. Trying to build the angular project on the server has overloaded it.

TODO: figure out correct way to set the DB path on the server so it's pointing to localhost rather than the dns entry. At the same time probably try to figure out a better way to handle the actual secrets.
