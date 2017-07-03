#!/bin/sh

 +docker-compose -f docker-compose.yml -f docker-compose.override.yml stop
  docker-compose -f docker-compose.yml -f docker-compose.override.yml build
  docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
  echo "**Run `docker-compose -f docker-compose.yml -f docker-compose.override.yml logs` to see output**"
