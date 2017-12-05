#!/bin/sh
docker-compose -f docker-compose.yml -f docker-compose.override.yml down #有多个-f参数可做替换操作，down:停止
docker-compose -f docker-compose.yml -f docker-compose.override.yml build #重建镜像
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d #后台运行
#echo "**Run `docker-compose -f docker-compose.yml -f docker-compose.override.yml logs` to see output**"
