#!/bin/sh

docker stack rm eval2
docker stack deploy -c docker-compose.yml eval2