#!/bin/sh
 
docker-compose -f docker-compose.yml -f docker-compose.override.yml scale cpu_microservice=1
docker-compose -f docker-compose.yml -f docker-compose.override.yml scale io_microservice=1
docker-compose -f docker-compose.yml -f docker-compose.override.yml scale mem_microservice=1