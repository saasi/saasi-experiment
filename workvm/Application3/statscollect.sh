#!/bin/sh
timeout -k 0.6s 0.5s sudo docker stats $(sudo docker ps --format={{.Names}}) &> stats.txt
