#!/bin/sh
cd ..
cd ..
sudo docker-compose scale businessfunction=$1
