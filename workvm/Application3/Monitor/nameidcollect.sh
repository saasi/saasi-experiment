#!/bin/sh
 docker ps --format "{{.ID}} {{.Image}}" &> name.txt
