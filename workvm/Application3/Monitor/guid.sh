#!/bin/sh
 docker ps --format "{{.ID}}" &> id.txt
