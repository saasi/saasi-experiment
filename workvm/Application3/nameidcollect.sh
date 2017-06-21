#!/bin/sh
sudo docker ps --format "{{.ID}}: {{.Names}} {{.Image}}" &> name.txt
