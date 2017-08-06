#!/bin/sh
cd ..
sudo docker-compose scale cpu_microservice=$1
