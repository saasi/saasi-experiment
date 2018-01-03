#/bin/sh
docker build -t regserv:5000/prometheus ./prometheus
docker build -t regserv:5000/grafana ./grafana
cd ../../src
docker build -t regserv:5000/business_microservice ./BusinessMicroservice
docker build -t regserv:5000/business_web -f ./Saasi.Monolithic.BusinessWeb/Dockerfile ./
docker build -t regserv:5000/docker-swarm-exporter ./docker-swarm-exporter
docker build -t regserv:5000/autoscaler ./autoscaler