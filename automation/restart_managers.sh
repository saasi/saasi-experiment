#!/bin/bash

for i in 0{1..3} ; do
  echo "restarting manager-$i"
  ssh root@manager-$i "echo \"Connected\" && sudo reboot"
done

