#!/bin/sh
cd ../microservices
sudo docker-compose scale businessfunction=$1
