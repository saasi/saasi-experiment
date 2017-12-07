#!/bin/sh
echo "=============Building LoadGenerator"
docker build -t loadgenerator . && echo "=============Build completed. Use it with run.sh."