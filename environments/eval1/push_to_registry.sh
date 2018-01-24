#!/bin/sh
docker push regserv:5000/prometheus
docker push regserv:5000/grafana
docker push regserv:5000/business_web
docker push regserv:5000/docker-swarm-exporter
docker push regserv:5000/autoscaler