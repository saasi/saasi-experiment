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

    def __init__(self):
        super().__init__('cpu_microservice')        
        
    def _check(self):
        cpuTotal = self._res.GetCPUUsageSum()
        targetScale = math.ceil(cpuTotal / self.CPU_THRESHOLD)
        currentScale = float(self._swarm.GetScaleTarget())
        if (abs(targetScale-currentScale) / currentScale > 0.05):
            self._do_scale(targetScale)

    def _do_scale(self, target):
        print('Scaling', self._microservice_name,'from',self._swarm.GetScaleTarget(), 'to', target)
        service = self._dockerClient.get_service_by_name(self._microservice_name)
        if (service != None):
            newMode = docker.types.ServiceMode('replicated', target)
            service.update(mode = newMode)

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