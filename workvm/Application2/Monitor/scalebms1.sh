#!/bin/sh
cd ..
sudo docker-compose scale businessfunction=$1
