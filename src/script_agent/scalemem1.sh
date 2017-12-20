#!/bin/sh
cd ../microservices
sudo docker-compose scale memory_microservice=$1
