#!/bin/sh
cd ../microservices
sudo docker-compose scale cpu_microservice=$1
