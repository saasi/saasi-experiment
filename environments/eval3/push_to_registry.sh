#!/bin/sh
docker push regserv:5000/prometheus
docker push regserv:5000/grafana
docker push regserv:5000/business_microservice_2
docker push regserv:5000/io_microservice
docker push regserv:5000/cpu_microservice
docker push regserv:5000/memory_microservice
docker push regserv:5000/docker-swarm-exporter
docker push regserv:5000/autoscaler
docker push regserv:5000/fluentd
docker push regserv:5000/traefik
