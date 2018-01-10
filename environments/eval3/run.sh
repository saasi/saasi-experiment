#!/bin/sh

docker stack rm eval3
docker stack deploy -c docker-compose.yml eval3