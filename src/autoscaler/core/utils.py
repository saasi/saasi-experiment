import docker
import requests
import re
import math
import datetime
from string import Template

class PrometheusClient(object):
    def __init__(self, endpoint="http://prometheus:9090"):
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
            return math.nan
        else:
            try:
                resultTimestamp = r['data']['result'][0]
                resultValue = r['data']['result'][1] 
                return [resultTimestamp, resultValue]
            except Exception:
                return math.nan

class ResourceUsageQuerier(PrometheusClient):
    QUERY_MEMORY = """
        scalar(
            avg(
                avg_over_time(
                    container_memory_rss{container_label_com_docker_swarm_service_name=~"\\\\w+_$msname"}[$timespan]
                )
            )
        )    """

    QUERY_MEMORY_SUM = """
        scalar(
            sum(
                avg_over_time(
                    container_memory_rss{container_label_com_docker_swarm_service_name=~"\\\\w+_$msname"}[$timespan]
                )
            )
        )    """

    QUERY_CPU = """
        scalar(avg(
        rate(
            container_cpu_usage_seconds_total{container_label_com_docker_swarm_service_name=~"\\\\w+_$msname"}[$timespan]
        )
        ) * 100)
    """

    QUERY_CPU_SUM = """
        scalar(sum(
        rate(
            container_cpu_usage_seconds_total{container_label_com_docker_swarm_service_name=~"\\\\w+_$msname"}[$timespan]
        )
        ) * 100)
    """

    QUERY_IO = """
        scalar(
            avg(
                rate(
                    container_fs_writes_bytes_total{container_label_com_docker_swarm_service_name=~"\\\\w+_$msname"}[$timespan]
                )
            )
        )
    """

    QUERY_IO_SUM = """
        scalar(
            sum(
                rate(
                    container_fs_writes_bytes_total{container_label_com_docker_swarm_service_name=~"\\\\w+_$msname"}[$timespan]
                )
            )
        )    
    """

    QUERY_BUSINESS_VIOLATION_RATE = """
        scalar(sum(increase(bms_business_violation_total[$timespan])) / (sum(increase(bms_requests_served[$timespan]))+1) )
    """

    QUERY_NET = """
        scalar(
            avg(
                rate(
                    container_network_transmit_bytes_total{container_label_com_docker_swarm_service_name=~"\\\\w+_$msname"}[$timespan]
                )
            )
        )
    """

    QUERY_NET_SUM = """
        scalar(
            sum(
                rate(
                    container_network_transmit_bytes_total{container_label_com_docker_swarm_service_name=~"\\\\w+_$msname"}[$timespan]
                )
            )
        )    
    """

    def __init__(self, endpoint, microservice_name):
        super().__init__(endpoint)
        self._microservice_name = microservice_name

    def GetMemoryUsage(self, timespan='1m'):
        query = Template(ResourceUsageQuerier.QUERY_MEMORY).substitute({'msname': self._microservice_name, 'timespan': timespan})
        return float(self.GetInstantValue(query)[1])

    def GetMemoryUsageSum(self, timespan='1m'):
        query = Template(ResourceUsageQuerier.QUERY_MEMORY_SUM).substitute({'msname': self._microservice_name, 'timespan': timespan})
        return float(self.GetInstantValue(query)[1])

    def GetCPUUsage(self, timespan='1m'):
        query = Template(ResourceUsageQuerier.QUERY_CPU).substitute({'msname': self._microservice_name, 'timespan': timespan})
        return float(self.GetInstantValue(query)[1])

    def GetCPUUsageSum(self, timespan='1m'):
        query = Template(ResourceUsageQuerier.QUERY_CPU_SUM).substitute({'msname': self._microservice_name, 'timespan': timespan})
        return float(self.GetInstantValue(query)[1])

    def GetIOUsage(self, timespan='1m'):
        query = Template(ResourceUsageQuerier.QUERY_NET).substitute({'msname': self._microservice_name, 'timespan': timespan})
        return float(self.GetInstantValue(query)[1])

    def GetIOUsageSum(self, timespan='1m'):
        query = Template(ResourceUsageQuerier.QUERY_NET_SUM).substitute({'msname': self._microservice_name, 'timespan': timespan})
        return float(self.GetInstantValue(query)[1])

    def GetBusinessViolationRate(self, timespan='30s'):
        query = Template(ResourceUsageQuerier.QUERY_BUSINESS_VIOLATION_RATE).substitute({'timespan': timespan})
        return float(self.GetInstantValue(query)[1])

class SwarmServiceStatusQuerier(PrometheusClient):

    QUERY_TARGET_SCALE = """
        scalar(docker_swarm_service_target_replicas{service_name=~"\\\\w+_$msname"})
    """
    def __init__(self, endpoint, microservice_name):
        super().__init__(endpoint)
        self._microservice_name = microservice_name
        
    def GetScaleTarget(self):
        query = Template(SwarmServiceStatusQuerier.QUERY_TARGET_SCALE).substitute({'msname':self._microservice_name})
        print(query)
        return int(self.GetInstantValue(query)[1])


class DockerClientHelper(object):
    def __init__(self):
        self._client = docker.from_env()

    def get_service_by_name(self, nameRegExp):
        serviceList = self._client.services.list()
        for s in serviceList:
            print('matching', nameRegExp, 'with', s.name)
            if (re.match(nameRegExp, s.name) != None):
                return s
        return None 


class DelayedActionHelper(object):
    def __init__(self):
        self._last_active_time = None
        self._is_active = False

    def setActive(self):
        if (self._is_active == False):
            self._is_active = True
            self._last_active_time = datetime.datetime.now()

    def setInactive(self):
        self._is_active = False
        self._last_active_time = None

    def activeFor(self):
        if self._is_active:
            return datetime.datetime.now() - self._last_active_time
        else:
            return datetime.timedelta(0)

    def isActive(self):
        return self._is_active