#!/bin/sh
docker ps --format "{{.Image}}" &> image.txt
