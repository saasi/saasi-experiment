#!/bin/sh
ansible-playbook -i hosts download-data.yml --ask-pass
