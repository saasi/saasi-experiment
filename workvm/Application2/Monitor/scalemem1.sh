#!/bin/sh
cd ..
sudo docker-compose scale memory_microservice=$1
