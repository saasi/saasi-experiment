#!/bin/sh
docker-compose -f docker-compose.yml -f docker-compose.override.yml scale cpu_microservice=3
