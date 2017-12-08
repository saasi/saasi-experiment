#!/bin/sh

echo "=== run.sh ==="

echo "Starting Script Agent"
cd ./script_agent
pkill script_agent
./script_agent &
cd ..

echo "Building Controllers"

cd ./controllers
docker-compose build
cd ..

echo "Building Microservices"
cd ./microservices
docker-compose build
cd ..

echo "Starting Microservices"
cd ./microservices
docker-compose down
docker-compose up -d
cd ..

echo "Starting Controllers"
cd ./controllers
docker-compose down
docker-compose up -d
cd ..

echo "=== END OF run.sh ==="