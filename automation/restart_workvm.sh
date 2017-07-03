#1/bin/sh
echo "Password $1"
for i in 0{2..9} {10..15} ; do
  echo "restarting vm$i"
  sshpass -p "$1" ssh saasi@saasi-vm$i.it.deakin.edu.au "echo \"Connected\" && sudo reboot"
done
