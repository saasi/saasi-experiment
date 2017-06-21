#!/bin/sh
 docker ps --format "{{.Names}}" &> name.txt
