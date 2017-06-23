#!/bin/sh
timeout -k 0.1s 0.1s $(docker stats $1 &> stats.txt)
