#!/bin/sh
ansible-playbook -i hosts cleanup.yml --ask-pass
