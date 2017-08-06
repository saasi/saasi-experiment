#!/bin/sh
cd ..
sudo docker-compose scale io_microservice=$1

