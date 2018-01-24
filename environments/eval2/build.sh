#/bin/sh
docker build -t regserv:5000/prometheus ./prometheus
docker build -t regserv:5000/grafana ./grafana
docker build -t regserv:5000/fluentd ./fluentd
docker build -t regserv:5000/traefik ./traefik

cp -f ./autoscaler/config.txt ../../src/autoscaler/config.txt
cd ../../src
docker build -t regserv:5000/business_microservice_2 -f ./Saasi.Microservices/Saasi.Microservices.Business/Dockerfile ./
docker build -t regserv:5000/combined_microservice -f ./Saasi.Microservices/Saasi.Microservices.Combined/Dockerfile ./
docker build -t regserv:5000/docker-swarm-exporter ./docker-swarm-exporter
docker build -t regserv:5000/autoscaler ./autoscaler