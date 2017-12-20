#!/bin/sh
docker push regserv:5000/business_microservice
docker push regserv:5000/io_microservice
docker push regserv:5000/cpu_microservice
docker push regserv:5000/memory_microservice
