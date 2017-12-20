#/bin/sh
docker build -t regserv:5000/business_microservice ./BusinessMicroservice
docker build -t regserv:5000/io_microservice ./IoMicroservice
docker build -t regserv:5000/cpu_microservice ./CpuMicroservice
docker build -t regserv:5000/memory_microservice ./MemoryMicroservice