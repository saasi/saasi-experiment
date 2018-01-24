from fabric.api import env, run, roles, cd, reboot, settings, parallel, execute, local, get
import requests
import time
from datetime import datetime
from string import Template
import math
import json

OUTPUT_PATH = '/home/ztl8702/saasi-data/users-$users-req-$requests-$ts-$type'


env.user = 'root'
env.roledefs = {
    'manager': ['manager-01'],
    'othermanagers': ['manager-02', 'manager-03'],
    'worker': [
        'worker-01', 'worker-02', 'worker-03', 'worker-04', 'worker-05',
        'worker-06', 'worker-07', 'worker-08', 'worker-09', 'worker-10'
    ],
    'loadgen': ['regserv']
}


class PrometheusClient(object):
    def __init__(self, endpoint="http://manager-01:9090"):
        self._endpoint = endpoint
    
    def GetInstantValue(self, queryString):
        '''
            Get an instant value from Prometheus.
            It is the caller's responsibility to ensure
            the query returns an instant.
        '''
        r = requests.get(self._endpoint + '/api/v1/query', params={'query': queryString})
        r = r.json()
        if r['status'] != 'success':
            return [0,float('nan')]
        else:
            try:
                resultTimestamp = r['data']['result'][0]
                resultValue = r['data']['result'][1] 
                return [resultTimestamp, resultValue]
            except Exception:
                return [0,float('nan')]

class ElasticSearchClient(object):
    def __init__(self, endpoint="http://manager-01:9200"):
        self._endpoint = endpoint
    
    def GetCount(self, queryString):
        '''
            Get an instant value from Prometheus.
            It is the caller's responsibility to ensure
            the query returns an instant.
        '''
        r = requests.get(self._endpoint + '/fluentd-*/_search', params={'q': queryString})
        r = r.json()
        print(json.dumps(r))
        return r['hits']['total']
        

prom = PrometheusClient()
elst = ElasticSearchClient()

@roles('loadgen')
def start_load_eval1(userCount, requestsToRun):
    with settings(warn_only=True):
        with cd('/root/saasi-experiment/environments/eval1'):
            run('locust --host=http://172.17.0.3:8080/api --no-web -r 5 -c '+str(userCount)+' -n '+str(requestsToRun))

@roles('loadgen')
def start_load_eval2(userCount, requestsToRun):
    with settings(warn_only=True):
        with cd('/root/saasi-experiment/environments/eval2'):
            run('locust --host=http://172.17.0.3:80/api --no-web -r 5 -c '+str(userCount)+' -n '+str(requestsToRun))

@roles('loadgen')
def start_load_eval3(userCount, requestsToRun):
    with settings(warn_only=True):
        with cd('/root/saasi-experiment/environments/eval3'):
            run('locust --host=http://172.17.0.3:80/api --no-web -r 5 -c '+str(userCount)+' -n '+str(requestsToRun))


@roles('loadgen')
def clean_load():
    with settings(warn_only=True):
        run('pkill -f -SIGKILL locust')

@roles('manager')
def clean_stack():
    run('docker stack rm eval1')
    run('docker stack rm eval2')
    run('docker stack rm eval3')

@roles('manager')
def build_stack_eval1():
    with cd('/root/saasi-experiment/environments/eval1'):
        run('./build.sh')
        run('./push_to_registry.sh')

@roles('manager')
def build_stack_eval2():
    with cd('/root/saasi-experiment/environments/eval2'):
        run('./build.sh')
        run('./push_to_registry.sh')

@roles('manager')
def build_stack_eval3():
    with cd('/root/saasi-experiment/environments/eval3'):
        run('./build.sh')
        run('./push_to_registry.sh')

@roles('manager')
def deploy_stack_eval1():
    with cd('/root/saasi-experiment/environments/eval1'):
        run('docker stack deploy -c docker-compose.yml eval1')

@roles('manager')
def deploy_stack_eval2():
    with cd('/root/saasi-experiment/environments/eval2'):
        run('docker stack deploy -c docker-compose.yml eval2')

@roles('manager')
def deploy_stack_eval3():
    with cd('/root/saasi-experiment/environments/eval3'):
        run('docker stack deploy -c docker-compose.yml eval3')

@parallel
@roles('worker')
def restart_workers():
    with settings(warn_only=True):
        reboot(use_sudo=False)

@roles('othermanagers')
def restart_othermanagers():
    with settings(warn_only=True):
        reboot(use_sudo=False)

@parallel
@roles('manager')
def restart_cheifmanager():
    with settings(warn_only=True):
        reboot(use_sudo=False)


def restart_cluster():
    execute(restart_othermanagers)
    execute(restart_workers)
    execute(restart_cheifmanager)

def ensure_servers_running():
    pass

def ensure_deployed():
    pass

def test_elasticsearch():
    try:
        r = requests.get('http://manager-01:9200/_cat/health', params={'h': 'status'})
    except Exception as e:
        return False
    return r.content.strip() in ['green','yellow']

def test_business_web():
    try:
        r = requests.get('http://manager-01:8080/api/status')
    except Exception as e:
        return False
    return r.content.strip() == "OK"

def test_business_web_eval2():
    try:
        r1 = requests.get('http://manager-01:80/api/status?operationId=0')
        r2 = requests.get('http://manager-01:80/api/status?operationId=2')
        r3 = requests.get('http://manager-01:80/api/status?operationId=4')
    except Exception as e:
        return False
    return r1.content.strip() == "OK" and r2.content.strip() == "OK" and r3.content.strip() == "OK"

def test_business_microservice():
    try:
        r = requests.get('http://manager-01:80/api/status')
    except Exception as e:
        return False
    return r.content.strip() == "OK"

def ensure_elasticsearch_healthy():
    while test_elasticsearch()!=True:
        print('Waiting for Elasticsearch')
        time.sleep(3)
        pass
    print('ElasticSearch OK!')

def ensure_business_web_healthy():
    while test_business_web()!=True:
        print('Waiting for Business Web')
        time.sleep(3)
        pass
    print('Business Web OK!')

def ensure_business_web_healthy_eval2():
    while test_business_web_eval2()!=True:
        print('Waiting for Business Web 1 2 3')
        time.sleep(3)
        pass
    print('Business Web 1 2 3 OK!')

def ensure_business_microservice_healthy():
    while test_business_microservice()!=True:
        print('Waiting for Business Microservice')
        time.sleep(3)
        pass
    print('Business Microservice OK!')

@roles('manager')
def export_data_eval1(timespan, outputPath):
    with cd('/root/saasi-experiment/environments/eval1'):
        run('./export_data.sh '+timespan+' /root/data')
    get('/root/data', outputPath)

@roles('manager')
def export_data_eval2(timespan, outputPath):
    with cd('/root/saasi-experiment/environments/eval2'):
        run('./export_data.sh '+timespan+' /root/data')
    get('/root/data', outputPath)

@roles('manager')
def export_data_eval3(timespan, outputPath):
    with cd('/root/saasi-experiment/environments/eval3'):
        run('./export_data.sh '+timespan+' /root/data')
    get('/root/data', outputPath)

def run_eval1(users='10',reqs='20'):
    requestsInt = int(reqs)
    usersInt = int(users)
    print("="*20)
    print('Evaluation 1, '+str(usersInt)+' users run for '+str(requestsInt)+ ' requests')
    print("="*20)
    outputPath = Template(OUTPUT_PATH).substitute({'users': users, 'requests': reqs, 'ts':datetime.now().strftime('%d%H%M%S'), 'type': 'eval1'})
    local("mkdir "+outputPath)

    execute(clean_load)
    retry = True
    while (retry):
        try:
            execute(clean_stack)
            execute(deploy_stack_eval1)
            retry = False
        except SystemExit:
            print('Retrying')
            retry = True

    ensure_elasticsearch_healthy()
    ensure_business_web_healthy()

    startTime = datetime.now()
    execute(start_load_eval1,userCount=usersInt, requestsToRun=requestsInt)
    execute(clean_load)
    # wait for the cluster to chill off
    while True:
        try:
            s = prom.GetInstantValue("scalar(sum(bms_active_transactions))")[1]
            print(s)
            p = int(s)
        except:
            p = 0
        if p<5:
            break
        print("Still",p,"requests running... Waiting for them to finish")
        time.sleep(10)

    endTime = datetime.now()
    timeSpent = endTime - startTime
    minutesSpent = int(math.ceil(timeSpent.seconds/60.0))
    print("From "+str(startTime)+' to '+str(endTime)+', thats '+str(minutesSpent)+' minutes.')
    # collect data
    result = collect_data_eval1(minutesSpent)

    print(result)
    with open(outputPath+'/data.json', 'w') as outfile:
        json.dump(result, outfile, sort_keys=True, indent=4)

    execute(export_data_eval1, str(minutesSpent)+'m', outputPath)
    
    execute(clean_stack)
    execute(restart_cluster)

def run_eval2(users='10',reqs='20'):
    requestsInt = int(reqs)
    usersInt = int(users)
    print("="*20)
    print('Evaluation 2, '+str(usersInt)+' users run for '+str(requestsInt)+ ' requests')
    print("="*20)
    outputPath = Template(OUTPUT_PATH).substitute({'users': users, 'requests': reqs, 'ts':datetime.now().strftime('%d%H%M%S'), 'type': 'eval2'})
    local("mkdir "+outputPath)

    execute(clean_load)
    retry = True
    while (retry):
        try:
            execute(clean_stack)
            execute(deploy_stack_eval2)
            retry = False
        except SystemExit:
            print('Retrying')
            retry = True

    ensure_elasticsearch_healthy()
    ensure_business_web_healthy_eval2()

    startTime = datetime.now()
    execute(start_load_eval2, userCount=usersInt, requestsToRun=requestsInt)
    execute(clean_load)
    # wait for the cluster to chill off
    while True:
        try:
            s = prom.GetInstantValue("scalar(sum(bms_active_transactions))")[1]
            print(s)
            p = int(s)
        except:
            p = 0
        if p<5:
            break
        print("Still",p,"requests running... Waiting for them to finish")
        time.sleep(10)

    endTime = datetime.now()
    timeSpent = endTime - startTime
    minutesSpent = int(math.ceil(timeSpent.seconds/60.0))
    print("From "+str(startTime)+' to '+str(endTime)+', thats '+str(minutesSpent)+' minutes.')
    # collect data
    result = collect_data_eval2(minutesSpent) # same as 3

    print(result)
    with open(outputPath+'/data.json', 'w') as outfile:
        json.dump(result, outfile, sort_keys=True, indent=4)

    execute(export_data_eval2, str(minutesSpent)+'m', outputPath)
    
    execute(clean_stack)
    execute(restart_cluster)

def run_eval3(users='10',reqs='20'):
    requestsInt = int(reqs)
    usersInt = int(users)
    print("="*20)
    print('Evaluation 3, '+str(usersInt)+' users run for '+str(requestsInt)+ ' requests')
    print("="*20)
    outputPath = Template(OUTPUT_PATH).substitute({'users': users, 'requests': reqs, 'ts':datetime.now().strftime('%d%H%M%S'), 'type': 'eval3'})
    local("mkdir "+outputPath)

    execute(clean_load)
    retry = True
    while (retry):
        try:
            execute(clean_stack)
            execute(deploy_stack_eval3)
            retry = False
        except SystemExit:
            print('Retrying')
            retry = True

    ensure_elasticsearch_healthy()
    ensure_business_microservice_healthy()

    startTime = datetime.now()
    execute(start_load_eval3, userCount=usersInt, requestsToRun=requestsInt)
    execute(clean_load)
    # wait for the cluster to chill off
    while True:
        try:
            s = prom.GetInstantValue("scalar(sum(bms_active_transactions))")[1]
            print(s)
            p = int(s)
        except:
            p = 0
        if p<5:
            break
        print("Still",p,"requests running... Waiting for them to finish")
        time.sleep(10)

    endTime = datetime.now()
    timeSpent = endTime - startTime
    minutesSpent = int(math.ceil(timeSpent.seconds/60.0))
    print("From "+str(startTime)+' to '+str(endTime)+', thats '+str(minutesSpent)+' minutes.')
    # collect data
    result = collect_data_eval3(minutesSpent)

    print(result)
    with open(outputPath+'/data.json', 'w') as outfile:
        json.dump(result, outfile, sort_keys=True, indent=4)

    execute(export_data_eval3, str(minutesSpent)+'m', outputPath)
    
    execute(clean_stack)
    execute(restart_cluster)

def collect_data_eval1(minutesSpent):
    # COST
    avg_scale = prom.GetInstantValue(Template("""
        scalar(
            avg_over_time(
                docker_swarm_service_running_replicas{service_name=~\"\\\\w+_business_web\"}[$timespan]
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    max_scale = prom.GetInstantValue(Template("""
        scalar(
            max_over_time(
                docker_swarm_service_running_replicas{service_name=~\"\\\\w+_business_web\"}[$timespan]
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    # UTILISATION
    avg_cpu_per_container_per_microservice = prom.GetInstantValue(Template("""
        scalar(
            avg_over_time(microservice_cpu_average{container_label_com_docker_swarm_service_name=~\"\\\\w+_business_web\"}[$timespan])
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    avg_mem_per_container_per_microservice = prom.GetInstantValue(Template("""
        scalar(
            avg_over_time(microservice_memory_average{container_label_com_docker_swarm_service_name=~\"\\\\w+_business_web\"}[$timespan])
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    avg_network_in_per_container_per_microservice = prom.GetInstantValue(Template("""
        scalar(
            avg_over_time(microservice_network_io_in_average{container_label_com_docker_swarm_service_name=~\"\\\\w+_business_web\"}[$timespan])
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    avg_network_out_per_container_per_microservice = prom.GetInstantValue(Template("""
        scalar(
            avg_over_time(microservice_network_io_out_average{container_label_com_docker_swarm_service_name=~\"\\\\w+_business_web\"}[$timespan])
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    # OVERHEAD
    workload_total_avg_cpu = prom.GetInstantValue(Template("""
        scalar(
            sum(
                avg_over_time(container_cpu_total{container_label_saasi_group=~\"workload\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    controller_total_avg_cpu = prom.GetInstantValue(Template("""
        scalar(
            sum(
                avg_over_time(container_cpu_total{container_label_saasi_group=~\"controller\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    workload_total_avg_mem = prom.GetInstantValue(Template("""
        scalar(
            sum(
                avg_over_time(container_memory_total{container_label_saasi_group=~\"workload\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    controller_total_avg_mem = prom.GetInstantValue(Template("""
        scalar(
            sum(
                avg_over_time(container_memory_total{container_label_saasi_group=~\"controller\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    workload_total_network_in = prom.GetInstantValue(Template("""
        scalar(
            sum(
                increase(container_network_io_in_total{container_label_saasi_group=~\"workload\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    workload_total_network_out = prom.GetInstantValue(Template("""
        scalar(
            sum(
                increase(container_network_io_out_total{container_label_saasi_group=~\"workload\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    controller_total_network_in = prom.GetInstantValue(Template("""
        scalar(
            sum(
                increase(container_network_io_in_total{container_label_saasi_group=~\"controller\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    controller_total_network_out = prom.GetInstantValue(Template("""
        scalar(
            sum(
                increase(container_network_io_out_total{container_label_saasi_group=~\"controller\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    business_requests_total = elst.GetCount("container_name: *business** AND log: *finished* AND log: Transcation")
    business_violation_total = elst.GetCount("container_name: *business** AND log: *violation* AND log: Transcation")


    return {
        'MaxScale': max_scale,
        'AvgScale': avg_scale,
        'CpuAverage': avg_cpu_per_container_per_microservice,
        'MemAverage': avg_mem_per_container_per_microservice,
        'NetInAverage': avg_network_in_per_container_per_microservice,
        'NetOutAverage': avg_network_out_per_container_per_microservice,
        'WorkloadTotalCpu': workload_total_avg_cpu,
        'WorkloadTotalMemory': workload_total_avg_mem,
        'ControllerTotalCpu': controller_total_avg_cpu,
        'ControllerTotalMemory': controller_total_avg_mem,
        'WorkloadNetInTotal': workload_total_network_in,
        'WorkloadNetOutTotal': workload_total_network_out,
        'ControllerNetInTotal': controller_total_network_in,
        'ControllerNetOutTotal': controller_total_network_out,
        'TotalRequests': business_requests_total,
        'TotalViolations': business_violation_total
    }

AVG_SCALE_MICROSERVICE = """
    scalar(
        avg_over_time(
            docker_swarm_service_running_replicas{service_name=~\"\\\\w+_$msname\"}[$timespan]
        )
    )
"""

MAX_SCALE_MICROSERVICE = """
        scalar(
            max_over_time(
                docker_swarm_service_running_replicas{service_name=~\"\\\\w+_$msname\"}[$timespan]
            )
        )
"""

AVG_CPU = """
    scalar(
        avg_over_time(microservice_cpu_average{container_label_com_docker_swarm_service_name=~\"\\\\w+_$msname\"}[$timespan])
    )
"""

AVG_MEM = """
    scalar(
        avg_over_time(microservice_memory_average{container_label_com_docker_swarm_service_name=~\"\\\\w+_$msname\"}[$timespan])
    )
"""

AVG_NET_IN = """
    scalar(
        avg_over_time(microservice_network_io_in_average{container_label_com_docker_swarm_service_name=~\"\\\\w+_$msname\"}[$timespan])
    )
"""

AVG_NET_OUT = """
    scalar(
        avg_over_time(microservice_network_io_out_average{container_label_com_docker_swarm_service_name=~\"\\\\w+_$msname\"}[$timespan])
    )
"""

def collect_data_eval2(minutesSpent):
    # COST
    avg_scale = {}
    max_scale = {}
    for service in ['business_web_1', 'business_web_2', 'business_web_3']:
        avg_scale[service] = prom.GetInstantValue(Template(AVG_SCALE_MICROSERVICE).substitute({'msname': service,'timespan': str(minutesSpent)+'m'}))[1]
        max_scale[service] = prom.GetInstantValue(Template(MAX_SCALE_MICROSERVICE).substitute({'msname': service, 'timespan': str(minutesSpent)+'m'}))[1]

    # UTILISATION
    avg_cpu_per_container_per_microservice = {}
    avg_mem_per_container_per_microservice = {}
    avg_network_in_per_container_per_microservice = {}
    avg_network_out_per_container_per_microservice = {}

    for service in ['business_web_1', 'business_web_2', 'business_web_3']:
        avg_cpu_per_container_per_microservice[service] = prom.GetInstantValue(Template(AVG_CPU).substitute({'msname': service, 'timespan': str(minutesSpent)+'m'}))[1]
        avg_mem_per_container_per_microservice[service] = prom.GetInstantValue(Template(AVG_MEM).substitute({'msname': service, 'timespan': str(minutesSpent)+'m'}))[1]
        avg_network_in_per_container_per_microservice[service] = prom.GetInstantValue(Template(AVG_NET_IN).substitute({'msname': service, 'timespan': str(minutesSpent)+'m'}))[1]
        avg_network_out_per_container_per_microservice[service] = prom.GetInstantValue(Template(AVG_NET_OUT).substitute({'msname': service, 'timespan': str(minutesSpent)+'m'}))[1]

    # OVERHEAD
    workload_total_avg_cpu = prom.GetInstantValue(Template("""
        scalar(
            sum(
                avg_over_time(container_cpu_total{container_label_saasi_group=~\"workload\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    controller_total_avg_cpu = prom.GetInstantValue(Template("""
        scalar(
            sum(
                avg_over_time(container_cpu_total{container_label_saasi_group=~\"controller\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    workload_total_avg_mem = prom.GetInstantValue(Template("""
        scalar(
            sum(
                avg_over_time(container_memory_total{container_label_saasi_group=~\"workload\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    controller_total_avg_mem = prom.GetInstantValue(Template("""
        scalar(
            sum(
                avg_over_time(container_memory_total{container_label_saasi_group=~\"controller\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    workload_total_network_in = prom.GetInstantValue(Template("""
        scalar(
            sum(
                increase(container_network_io_in_total{container_label_saasi_group=~\"workload\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    workload_total_network_out = prom.GetInstantValue(Template("""
        scalar(
            sum(
                increase(container_network_io_out_total{container_label_saasi_group=~\"workload\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    controller_total_network_in = prom.GetInstantValue(Template("""
        scalar(
            sum(
                increase(container_network_io_in_total{container_label_saasi_group=~\"controller\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    controller_total_network_out = prom.GetInstantValue(Template("""
        scalar(
            sum(
                increase(container_network_io_out_total{container_label_saasi_group=~\"controller\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    business_requests_total = elst.GetCount("container_name: *business** AND log: *finished* AND log: Transcation")
    business_violation_total = elst.GetCount("container_name: *business** AND log: *violation* AND log: Transcation")


    return {
        'MaxScale': max_scale,
        'AvgScale': avg_scale,
        'CpuAverage': avg_cpu_per_container_per_microservice,
        'MemAverage': avg_mem_per_container_per_microservice,
        'NetInAverage': avg_network_in_per_container_per_microservice,
        'NetOutAverage': avg_network_out_per_container_per_microservice,
        'WorkloadTotalCpu': workload_total_avg_cpu,
        'WorkloadTotalMemory': workload_total_avg_mem,
        'ControllerTotalCpu': controller_total_avg_cpu,
        'ControllerTotalMemory': controller_total_avg_mem,
        'WorkloadNetInTotal': workload_total_network_in,
        'WorkloadNetOutTotal': workload_total_network_out,
        'ControllerNetInTotal': controller_total_network_in,
        'ControllerNetOutTotal': controller_total_network_out,
        'TotalRequests': business_requests_total,
        'TotalViolations': business_violation_total
    }



def collect_data_eval3(minutesSpent):
    # COST
    avg_scale = {}
    max_scale = {}
    for service in ['business_microservice', 'io_microservice', 'cpu_microservice', 'memory_microservice']:
        avg_scale[service] = prom.GetInstantValue(Template(AVG_SCALE_MICROSERVICE).substitute({'msname': service,'timespan': str(minutesSpent)+'m'}))[1]
        max_scale[service] = prom.GetInstantValue(Template(MAX_SCALE_MICROSERVICE).substitute({'msname': service, 'timespan': str(minutesSpent)+'m'}))[1]

    # UTILISATION
    avg_cpu_per_container_per_microservice = {}
    avg_mem_per_container_per_microservice = {}
    avg_network_in_per_container_per_microservice = {}
    avg_network_out_per_container_per_microservice = {}

    for service in ['business_microservice', 'io_microservice', 'cpu_microservice', 'memory_microservice']:
        avg_cpu_per_container_per_microservice[service] = prom.GetInstantValue(Template(AVG_CPU).substitute({'msname': service, 'timespan': str(minutesSpent)+'m'}))[1]
        avg_mem_per_container_per_microservice[service] = prom.GetInstantValue(Template(AVG_MEM).substitute({'msname': service, 'timespan': str(minutesSpent)+'m'}))[1]
        avg_network_in_per_container_per_microservice[service] = prom.GetInstantValue(Template(AVG_NET_IN).substitute({'msname': service, 'timespan': str(minutesSpent)+'m'}))[1]
        avg_network_out_per_container_per_microservice[service] = prom.GetInstantValue(Template(AVG_NET_OUT).substitute({'msname': service, 'timespan': str(minutesSpent)+'m'}))[1]

    # OVERHEAD
    workload_total_avg_cpu = prom.GetInstantValue(Template("""
        scalar(
            sum(
                avg_over_time(container_cpu_total{container_label_saasi_group=~\"workload\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    controller_total_avg_cpu = prom.GetInstantValue(Template("""
        scalar(
            sum(
                avg_over_time(container_cpu_total{container_label_saasi_group=~\"controller\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    workload_total_avg_mem = prom.GetInstantValue(Template("""
        scalar(
            sum(
                avg_over_time(container_memory_total{container_label_saasi_group=~\"workload\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    controller_total_avg_mem = prom.GetInstantValue(Template("""
        scalar(
            sum(
                avg_over_time(container_memory_total{container_label_saasi_group=~\"controller\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    workload_total_network_in = prom.GetInstantValue(Template("""
        scalar(
            sum(
                increase(container_network_io_in_total{container_label_saasi_group=~\"workload\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    workload_total_network_out = prom.GetInstantValue(Template("""
        scalar(
            sum(
                increase(container_network_io_out_total{container_label_saasi_group=~\"workload\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    controller_total_network_in = prom.GetInstantValue(Template("""
        scalar(
            sum(
                increase(container_network_io_in_total{container_label_saasi_group=~\"controller\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    controller_total_network_out = prom.GetInstantValue(Template("""
        scalar(
            sum(
                increase(container_network_io_out_total{container_label_saasi_group=~\"controller\"}[$timespan])
            )
        )
    """).substitute({'timespan': str(minutesSpent)+'m'}))[1]

    business_requests_total = elst.GetCount("container_name: *business** AND log: *finished* AND log: Transcation")
    business_violation_total = elst.GetCount("container_name: *business** AND log: *violation* AND log: Transcation")


    return {
        'MaxScale': max_scale,
        'AvgScale': avg_scale,
        'CpuAverage': avg_cpu_per_container_per_microservice,
        'MemAverage': avg_mem_per_container_per_microservice,
        'NetInAverage': avg_network_in_per_container_per_microservice,
        'NetOutAverage': avg_network_out_per_container_per_microservice,
        'WorkloadTotalCpu': workload_total_avg_cpu,
        'WorkloadTotalMemory': workload_total_avg_mem,
        'ControllerTotalCpu': controller_total_avg_cpu,
        'ControllerTotalMemory': controller_total_avg_mem,
        'WorkloadNetInTotal': workload_total_network_in,
        'WorkloadNetOutTotal': workload_total_network_out,
        'ControllerNetInTotal': controller_total_network_in,
        'ControllerNetOutTotal': controller_total_network_out,
        'TotalRequests': business_requests_total,
        'TotalViolations': business_violation_total
    }

