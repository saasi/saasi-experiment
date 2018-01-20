#!/bin/bash

for i in 0{1..9} {10..10} ; do
  echo "restarting worker-$i"
  ssh root@worker-$i "echo \"Connected\" && sudo reboot"
done

