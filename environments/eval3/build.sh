#/bin/sh
docker build -t regserv:5000/prometheus ./prometheus
docker build -t regserv:5000/grafana ./grafana
docker build -t regserv:5000/fluentd ./fluentd

cp -f ./autoscaler/config.txt ../../src/autoscaler/config.txt
cd ../../src
docker build -t regserv:5000/business_microservice ./BusinessMicroservice
docker build -t regserv:5000/io_microservice -f ./Saasi.Microservices/Saasi.Microservices.Io/Dockerfile ./
docker build -t regserv:5000/cpu_microservice -f ./Saasi.Microservices/Saasi.Microservices.Cpu/Dockerfile ./
docker build -t regserv:5000/memory_microservice -f ./Saasi.Microservices/Saasi.Microservices.Memory/Dockerfile ./
docker build -t regserv:5000/docker-swarm-exporter ./docker-swarm-exporter
docker build -t regserv:5000/autoscaler ./autoscaler