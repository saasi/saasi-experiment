# Evaluation 2

## Components
These are the actual workload (负载).

- Business microservices:
  - `src/BusinessService`
- API microservices
  - `src/Saasi.Microservices/Saasi.Microservices.Io`
  - `src/Saasi.Microservices/Saasi.Microservices.Cpu`
  - `src/Saasi.Microservices/Saasi.Microservices.Memory`

## Prerequisites
- Docker Swarm is set up. Assume `<manager ip>` is the IP address of an arbitary manager node.
- A private Docker registry is set up on `regserv:5000`. All other nodes in the cluster can resolve the hostname `regserv` and is configure to trust this _unsafe_ Docker registry


## Procedures

### Build
On your laptop or on one of the servers, build the microservices and push them to the private Docker registry (`regserv:5000`).

```bash
git clone https://github.com/ztl8702/saasi-experiment.git
cd saasi-experiment/environments/eval3
chmod +x ./build.sh
chmod +x ./push_to_registry.sh
./build.sh
./push_to_registry.sh
```

Check for any errors before proceeding.

> This step is optional and can be replaced by Jenkins pipeline. (Work in progress)

### Deploy

Log in to any of the **manager** nodes of the Docker Swarm cluster. Run:

```bash
git clone https://github.com/ztl8702/saasi-experiment.git
cd saasi-experiment/environments/eval3
docker stack deploy -c docker-compose my_deployment1
```

> Check if any previous deployments are running by using `docker stack ls` and remove them by `docker stack rm xxx`

### Web Access

Web access is available at the IP of any manager node. Suppose `111.111.111.111` is one of the manager nodes.

- A visualiser is available at "http://111.111.111.111"

- Business Microservice is exposed at `http://111.111.111.111:8080`.
  - This is an loadbalanced endpoint. Requests will be routed to one of the containers running `business_microservice`
 
### Generate Traffic
On your laptop or one of the servers (preferrably a server that is not running the tasks).

First install `locust`:

```
pip install locustio
```

Then:

```bash 
git clone https://github.com/ztl8702/saasi-experiment.git
cd saasi-experiment/environments/eval3
locust --host=http://111.111.111.111:8080
```

Open up your browser and navigate to `localhost:8089` to access the Locust Web UI. Then you can specify the number of concurrent users.

### Data Collection

We need to collect: 

> Work in progress