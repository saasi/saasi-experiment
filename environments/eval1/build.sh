#/bin/sh
docker build -t regserv:5000/prometheus ./prometheus
docker build -t regserv:5000/grafana ./grafana
cp -f ./autoscaler/config.txt ../../src/autoscaler/config.txt
cd ../../src
docker build -t regserv:5000/autoscaler ./autoscaler
docker build -t regserv:5000/business_web -f ./Saasi.Monolithic.BusinessWeb/Dockerfile ./
docker build -t regserv:5000/docker-swarm-exporter ./docker-swarm-exporter