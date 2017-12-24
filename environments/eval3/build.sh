#/bin/sh
docker build -t regserv:5000/prometheus ./prometheus
docker build -t regserv:5000/grafana ./grafana
cd ../../src
docker build -t regserv:5000/business_microservice ./BusinessMicroservice
docker build -t regserv:5000/io_microservice ./Saasi.Microservices/Saasi.Microservices.Io
docker build -t regserv:5000/cpu_microservice ./Saasi.Microservices/Saasi.Microservices.Cpu
docker build -t regserv:5000/memory_microservice ./Saasi.Microservices/Saasi.Microservices.Memory