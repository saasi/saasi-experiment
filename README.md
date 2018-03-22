## SmartVM Prototype Experiments

This repository contains the code and configuration of the SmartVM prototype components and evaluation applications.

## Folder structure
- `src` All the code of SmartVM components and evaluation applications
  - `autoscaler` The Autoscaler
  - `docker-swarm-exporter` Prometheus exporter for Docker Swarm
  - `BusinessMicroservice` (_Deprecated_) Java implementation of the monolithic application
  - `Saasi.Microservices` Evaluation application implemented as microservices
  - `Saasi.Monolithic.BusinessWeb` Evaluation application implemented as a monolithic application
  - `Saasi.Shared` Shared application logic of `Saasi.Microservices` and `Saasi.Monolithic.BusinessWeb`
- `automation` Automation code
- `environments`
  - `eval1` Configuration files and scripts for Evaluation 1 (Monolithic)
  - `eval2` Configuration files and scripts for Evaluation 2 (Uniform microservices)
  - `eval3` Configuration files and scripts for Evaluation 3 (SmartVM)
- `results` Previous experimental data. New data is located at `saasi/saasi-data`
- `docs` (_Deprecated_) Documentation 

## Experiment Setup
### Requirements

The sample setup consist of 14 VMs, running in the same Local Area Network. They can be divided into the following roles:

- Docker Registry Server & Load Generator.
- Manager Nodes
- Worker Nodes

Each VM should be configure with the hostname listed in the table below, and every VM must be reachable by its hostname.

| Type | hostname | Additional Setup |
|------|----------|------------------|
| Registry Server & Load Generator | `regserv` | - Docker Registry running on port:5000 <br/> - `pip install fabric` |
| Managers | `manager-01` - `manager-03` | - Join Docker Swarm and promote to manager node. <br/> - Trust registry `regserv:5000`.|
| Workers | `worker-01` - `worker-10` | - Join Docker Swarm. <br/> - Trust registry `regserv:5000`.|

Every VM should also have the following programs installed:
 - Git
 - Docker
 - Docker Compose
 - Python

Generate SSH key pair on `regserv` and upload the public key to all other VMs.

### Running the experiments
1. Clone this repository on `manager-01` to `manager-03` and `regserv`.
```
cd /root
git clone https://github.com/saasi/saasi-experiment
```

2. Edit `automation/fabfile.py` on `regserv` to change the paths and other configuration.

3. Now we can run the experiments. Experiments will be initiated on `regserv`, which in this sample setup also serve as the load generator and deployer.
   
   All steps of the experiment is implemented in `automation/fabfile.py`, here are some the essential ones:
     - `fab clean_stack` Clears the running containers on all Managers and Workers
     - `fab restart_cluster` Restarts all VMs
     - `fab build_stack_eval<number>` Builds the components for Evaluation `<number>`
     - `fab run_eval<number>:<c>,<r>` Run Evaluation `<number>`, generate load to simulate `<c>` concurrent users and `<r>` requests each.

  For example, the run Evaluation 2 (Uniform microservices) with two test cases `{Users = 10, Requests = 300}` and `{Users = 25, Requests = 750}`, run:

```
cd saasi-experiment/automation
fab clean_stack
fab build_stack_eval2
fab run_eval2:10,300
fab run_eval2:25,750
fab restart_cluster
```

  The data will be automatically collected and downloaded. Modify `fabfile.py` to change where to save the data.
  
  It is recommended to restart the VMs after each experiment, to ensure consistent results.
