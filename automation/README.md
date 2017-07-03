# Automated Deployment

## 1. Install Ansible on your laptop
```
sudo apt-get install ansible
```
> No need to install anything on the server

## 2. Push code to the `deploy` branch
```bash
git checkout deploy
git merge master
git push origin deploy
```

## 3. `cd` to this directory

## 4. Run Ansible on your laptop

> Ensure the servers are reachable from your laptop. Modify the server list in the `hosts` file.

```bash
ansible-playbook --i hosts workvm.yml --ask-pass
```

This will deploy the lastest code and run the microservices.
