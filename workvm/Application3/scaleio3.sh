#!/bin/sh
docker-compose -f docker-compose.yml -f docker-compose.override.yml scale io_microservice=3
