import docker
import utils
import asyncio
import math

import os 
PROMETHEUS_HOST = os.environ.get('AUTOSCALER_PROM_HOST','http://tasks.prometheus:9090')

class MicroserviceMonitoringGroup(object):
    def __init__(self, microservice_name):
        self._microservice_name = microservice_name
        self._swarm = utils.SwarmServiceStatusQuerier(PROMETHEUS_HOST, microservice_name)
        self._res = utils.ResourceUsageQuerier(PROMETHEUS_HOST, microservice_name)
        self._dockerClient = utils.DockerClientHelper()

    @asyncio.coroutine
    def control_loop(self, loop):
        yield from asyncio.sleep(5) # Check every 5 seconds
        print(self._microservice_name,'Checking...')
        try:
            self._check()
        except (KeyboardInterrupt, SystemExit):
            raise
        except Exception as e:
            print(e)
        asyncio.ensure_future(self.control_loop(loop))

    
    def _check(self):
        pass 

class CpuMicroserviceMG(MicroserviceMonitoringGroup):
    CPU_THRESHOLD = 80.0

    def __init__(self, microservice_name = 'cpu_microservice'):
        super().__init__(microservice_name)        
        
    def _check(self):
        cpuTotalFast = self._res.GetCPUUsageSum(timespan='20s')
        cpuTotalSlow = self._res.GetCPUUsageSum(timespan='1m')
        targetScaleFast = math.ceil(cpuTotalFast / self.CPU_THRESHOLD)
        targetScaleSlow = math.ceil(cpuTotalSlow / self.CPU_THRESHOLD)
        currentScale = float(self._swarm.GetScaleTarget())
        if targetScaleFast < 5:
            targetScaleFast = 5 # Lower bound
        if targetScaleSlow < 5:
            targetScaleSlow = 5 # Lower bound
        if ((targetScaleFast -currentScale) / currentScale > 0.05):
            self._do_scale(targetScaleFast)
        elif ((currentScale - targetScaleSlow) / currentScale > 0.05):
            self._do_scale(targetScaleSlow)

    def _do_scale(self, target):
        print('Scaling', self._microservice_name,'from',self._swarm.GetScaleTarget(), 'to', target)
        service = self._dockerClient.get_service_by_name("\\w+_"+self._microservice_name)
        if (service != None):
            newMode = docker.types.ServiceMode('replicated', target)
            service.update(mode = newMode)
            print('updated')

class MemoryMicroserviceMG(MicroserviceMonitoringGroup):
    MEMORY_THRESHOLD = 100 * 1024.0 * 1024.0 # 100MB

    def __init__(self):
        super().__init__('memory_microservice')        
        
    def _check(self):
        memTotal = self._res.GetMemoryUsageSum('2m')
        targetScale = math.ceil(memTotal / self.MEMORY_THRESHOLD)
        currentScale = float(self._swarm.GetScaleTarget())
        if targetScale < 5:
            targetScale = 5 # Lower bound
        if (abs(targetScale-currentScale) / currentScale > 0.05):
            self._do_scale(targetScale)

    def _do_scale(self, target):
        print('Scaling', self._microservice_name,'from',self._swarm.GetScaleTarget(), 'to', target)
        service = self._dockerClient.get_service_by_name("\\w+_"+self._microservice_name)
        if (service != None):
            newMode = docker.types.ServiceMode('replicated', target)
            service.update(mode = newMode)
            print('updated')

class BusinessMicroserviceMG(MicroserviceMonitoringGroup):
    BUSINESS_VIOLATION_RATE_THRESHOLD = 0.20

    def __init__(self):
        super().__init__('business_microservice')        
        
    def _check(self):
        currentRate = self._res.GetBusinessViolationRate()
        currentScale = self._swarm.GetScaleTarget()
        if (currentRate > 0.20):
            targetScale = currentScale + 1
        elif (currentRate < 0.05):
            targetScale = currentScale - 1
        if targetScale < 5:
            targetScale = 5 # Lower bound
        if (targetScale != currentScale):
            self._do_scale(targetScale)

    def _do_scale(self, target):
        print('Scaling', self._microservice_name,'from',self._swarm.GetScaleTarget(), 'to', target)
        service = self._dockerClient.get_service_by_name("\\w+_"+self._microservice_name)
        if (service != None):
            newMode = docker.types.ServiceMode('replicated', target)
            service.update(mode = newMode)
            print('updated')


class DummyMG(MicroserviceMonitoringGroup):
    CPU_THRESHOLD = 80.0

    def __init__(self):
        super().__init__('cpu_microservice')        
        
    def _check(self):
        print('Do Nothing')


globalloop = asyncio.get_event_loop()

def register(obj):
    print('Registering...')
    asyncio.ensure_future(obj.control_loop(globalloop))

def start_event_loop():
    print("Starting event loop...")
    try:
        globalloop.run_forever()
    except KeyboardInterrupt:
        print('Keyboard interrupt')
        globalloop.close()