from prometheus_client import start_http_server
from prometheus_client.core import GaugeMetricFamily, REGISTRY
import docker
import requests
import sys
import time


client = docker.from_env()
api_client = docker.APIClient(base_url='unix://var/run/docker.sock')

def GetServicesStatus(services, nodes, tasks):
    running = {}
    tasksNoShutdown = {}
    activeNodes = {}

    # Find out which nodes are active
    for node in nodes:
        if node.attrs['Status']['State'] != 'down':
            activeNodes[node.id] = True

    # Find out which tasks are running
    for task in tasks:
        if task['DesiredState'] != 'shutdown':
            tasksNoShutdown[task['ServiceID']] = tasksNoShutdown.get(task['ServiceID'], 0) + 1
        if (task['NodeID'] in activeNodes and task['Status']['State'] == 'running'):
            running[task['ServiceID']] = running.get(task['ServiceID'], 0) + 1

    info = {}

    for service in services:
        info[service.id] = {}
        if ('Replicated' in service.attrs['Spec']['Mode'] and 'Replicas' in service.attrs['Spec']['Mode']['Replicated']):
            info[service.id] = {
                'Mode': 'replicated',
                'Running': running[service.id],
                'Target': service.attrs['Spec']['Mode']['Replicated']['Replicas']
            }
        elif ('Global' in service.attrs['Spec']['Mode']):
            info[service.id] = {
                'Mode': 'global',
                'Running': running[service.id],
                'Target': tasksNoShutdown[service.id]
            }
    return info
    
class SwarmServiceCollector(object):
  def collect(self):
    # Fetch the JSON
    serviceList = client.services.list()

    # Number of Docker Swarm services
    metric = GaugeMetricFamily('docker_swarm_services_total',
        'Number of Docker Swarm services')
    metric.add_metric(labels=[], value=len(serviceList))
    yield metric


    serviceStatus = GetServicesStatus(serviceList, client.nodes.list(), api_client.tasks())
    # 
    metric = GaugeMetricFamily('docker_swarm_service_target_replicas',
        'Target Replicas',
        labels = ['service_name', 'service_id'])
    for serv in serviceList:
        try: 
            target = serviceStatus[serv.id]['Target']
        except KeyError:
            target = 0
        metric.add_metric(labels = [serv.attrs['Spec']['Name'], serv.id], value = target)
    yield metric

    # 
    metric = GaugeMetricFamily('docker_swarm_service_running_replicas',
        'Running Replicas',
        labels = ['service_name', 'service_id'])
    for serv in serviceList:
        try: 
            rr = serviceStatus[serv.id]['Running']
        except KeyError:
            rr = 0
        metric.add_metric(labels = [serv.attrs['Spec']['Name'], serv.id], value = rr)
    yield metric

if __name__ == '__main__':
    start_http_server(5051)
    REGISTRY.register(SwarmServiceCollector())

    while True: 
        time.sleep(1)