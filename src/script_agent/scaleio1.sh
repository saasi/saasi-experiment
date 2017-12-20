#!/bin/sh
cd ../microservices
sudo docker-compose scale io_microservice=$1

