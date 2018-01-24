import docker
import utils
import asyncio
import math

import os 
PROMETHEUS_HOST = os.environ.get('AUTOSCALER_PROM_HOST','http://tasks.prometheus:9090')

def limit_range(number, lower_bound, upper_bound):
    if (number < lower_bound):
        number = lower_bound
    if (number > upper_bound):
        number = upper_bound
    return number

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

    def _do_scale(self, target):
        print('Scaling', self._microservice_name,'from',self._swarm.GetScaleTarget(), 'to', target)
        service = self._dockerClient.get_service_by_name("\\w+_"+self._microservice_name)
        if (service != None):
            newMode = docker.types.ServiceMode('replicated', target)
            service.update(mode = newMode) #args=['--stop-grace-period=1h'])
            print('updated')

class CpuMicroserviceMG(MicroserviceMonitoringGroup):
    

    def __init__(self, microservice_name = 'cpu_microservice', min_scale = 1, max_scale = 10, threshold= 60.0 * 2 ): # 2 cores
        super().__init__(microservice_name)
        self._min_scale = min_scale
        self._max_scale = max_scale
        self._scale_up_rule = utils.DelayedActionHelper()
        self._scale_down_rule = utils.DelayedActionHelper()
        self.CPU_THRESHOLD = threshold
    def _check(self):
        cpuTotal = self._res.GetCPUUsageSum(timespan='30s')
        targetScale = math.ceil(cpuTotal / self.CPU_THRESHOLD)
        targetScale = limit_range(targetScale, self._min_scale, self._max_scale)
        currentScale = float(self._swarm.GetScaleTarget())
        print("###CPU", cpuTotal, targetScale, self._scale_up_rule.activeFor().total_seconds(),(targetScale - currentScale) / currentScale)
        if ((targetScale - currentScale) / currentScale > 0.05):
            # scale up rule
            self._scale_up_rule.setActive()
            if (self._scale_up_rule.activeFor().total_seconds() > 30):
                self._do_scale(targetScale)
                self._scale_up_rule.setInactive()
        else:
            self._scale_up_rule.setInactive()

        if ((currentScale - targetScale) / currentScale > 0.05):
            # scale down rule
            self._scale_down_rule.setActive()
            if (self._scale_down_rule.activeFor().total_seconds() > 70):
                self._do_scale(targetScale)
                self._scale_down_rule.setInactive()
        else:
            self._scale_down_rule.setInactive()

class IoMicroserviceMG(MicroserviceMonitoringGroup):
    IO_THRESHOLD = 10 * 1024.0 * 1024.0

    def __init__(self, microservice_name = 'io_microservice', min_scale = 2, max_scale = 20):
        super().__init__(microservice_name)
        self._min_scale = min_scale
        self._max_scale = max_scale
        self._scale_up_rule = utils.DelayedActionHelper()
        self._scale_down_rule = utils.DelayedActionHelper()
        
    def _check(self):
        ioTotal = self._res.GetIOUsageSum(timespan='30s')
        targetScale = math.ceil(ioTotal / self.IO_THRESHOLD)
        targetScale = limit_range(targetScale, self._min_scale, self._max_scale)
        currentScale = float(self._swarm.GetScaleTarget())
        print("###IO", ioTotal, targetScale, self._scale_up_rule.activeFor().total_seconds(),(targetScale - currentScale) / currentScale)
        if ((targetScale - currentScale) / currentScale > 0.05):
            # scale up rule
            self._scale_up_rule.setActive()
            if (self._scale_up_rule.activeFor().total_seconds() > 30):
                self._do_scale(targetScale)
                self._scale_up_rule.setInactive()
        else:
            self._scale_up_rule.setInactive()

        if ((currentScale - targetScale) / currentScale > 0.05):
            # scale down rule
            self._scale_down_rule.setActive()
            if (self._scale_down_rule.activeFor().total_seconds() > 70):
                self._do_scale(targetScale)
                self._scale_down_rule.setInactive()
        else:
            self._scale_down_rule.setInactive()

class MemoryMicroserviceMG(MicroserviceMonitoringGroup):

    def __init__(self, microservice_name = 'memory_microservice', min_scale = 1, max_scale = 40, threshold =  150 * 1024.0 * 1024.0 ):
        super().__init__(microservice_name)
        self._min_scale = min_scale
        self._max_scale = max_scale
        self._scale_up_rule = utils.DelayedActionHelper()
        self._scale_down_rule = utils.DelayedActionHelper()
        self.MEMORY_THRESHOLD = threshold
        
    def _check(self):
        memTotal = self._res.GetMemoryUsage('30s')
        currentScale = self._swarm.GetScaleTarget()
         
        print("#### MEM",memTotal, self.MEMORY_THRESHOLD,(memTotal - self.MEMORY_THRESHOLD) / self.MEMORY_THRESHOLD)
        if ((memTotal - self.MEMORY_THRESHOLD) / self.MEMORY_THRESHOLD > 0.1):
            # scale up rule
            self._scale_up_rule.setActive()
            if (self._scale_up_rule.activeFor().total_seconds() > 10):
                targetScale = currentScale + 2
                targetScale = limit_range(targetScale, self._min_scale, self._max_scale)
                self._do_scale(targetScale)
                self._scale_up_rule.setInactive()
        else:
            self._scale_up_rule.setInactive()

        if ((self.MEMORY_THRESHOLD - memTotal) / self.MEMORY_THRESHOLD > 0.05):
            # scale down rule
            self._scale_down_rule.setActive()
            if (self._scale_down_rule.activeFor().total_seconds() > 30):
                targetScale = currentScale - 1
                targetScale = limit_range(targetScale, self._min_scale, self._max_scale)
                self._do_scale(targetScale)
                self._scale_down_rule.setInactive()
        else:
            self._scale_down_rule.setInactive()

class MemoryMicroserviceMGStub(MemoryMicroserviceMG):
    def __init__(self, scaleout_func, microservice_name = 'memory_microservice', min_scale = 1, max_scale = 40,  threshold =  150 * 1024.0 * 1024.0):
        super().__init__(microservice_name,min_scale,max_scale,threshold)
        self._scaleout_func = scaleout_func

    def _do_scale(self, targetScale):
        print("Passing scale to ...")
        self._scaleout_func(targetScale)

class CpuMicroserviceMGStub(CpuMicroserviceMG):

    def __init__(self, scaleout_func, microservice_name = 'cpu_microservice', min_scale = 1, max_scale = 10, threshold=48.0*2):
        super().__init__(microservice_name, min_scale, max_scale,threshold)
        self._scaleout_func = scaleout_func
 
    def _do_scale(self, targetScale):
        print("Passing scale to ...")
        self._scaleout_func(targetScale)

class BusinessMicroserviceMG(MicroserviceMonitoringGroup):
    BUSINESS_VIOLATION_RATE_THRESHOLD = 0.15

    def __init__(self, min_scale = 1, max_scale = 20):
        super().__init__('business_microservice')        
        self._min_scale = min_scale
        self._max_scale = max_scale
        self._scale_up_rule = utils.DelayedActionHelper()
        self._scale_down_rule = utils.DelayedActionHelper()

    def _check(self):
        currentRate = self._res.GetBusinessViolationRate()
        currentScale = self._swarm.GetScaleTarget()
        
        if (currentRate > 0.15):
            self._scale_up_rule.setActive()
            if (self._scale_up_rule.activeFor().total_seconds() > 15):
                targetScale = currentScale + 2
                targetScale = limit_range(targetScale, self._min_scale, self._max_scale)
                self._do_scale(targetScale)
                self._scale_up_rule.setInactive()
        else:
            self._scale_up_rule.setInactive()

        if (currentRate < 0.05):
            self._scale_down_rule.setActive()
            if (self._scale_down_rule.activeFor().total_seconds() > 30):
                targetScale = currentScale - 1
                targetScale = limit_range(targetScale, self._min_scale, self._max_scale)
                self._do_scale(targetScale)
                self._scale_down_rule.setInactive()
        else:
            self._scale_down_rule.setInactive()

class BusinessWebMG(MicroserviceMonitoringGroup):
    def __init__(self, microservice_name = 'business_web', min_scale = 1, max_scale = 40):
        super().__init__(microservice_name)
        self._min_scale = min_scale
        self._max_scale = max_scale
        self._cpu_mg = CpuMicroserviceMGStub(self._cpu_callback, microservice_name='business_web', min_scale=min_scale, max_scale=max_scale, threshold= 48.0*2)
        self._memory_mg = MemoryMicroserviceMGStub(self._memory_callback,  microservice_name='business_web', min_scale=min_scale, max_scale=max_scale, threshold=200 * 1024.0 * 1024.0)
        self._cpu_scale = 0
        self._memory_scale = 0

    def _check(self):
        self._cpu_mg._check()
        self._memory_mg._check()

    def _cpu_callback(self, targetScale):
        self._cpu_scale = targetScale
        self._do_check()

    def _memory_callback(self, targetScale):
        self._memory_scale = targetScale 
        self._do_check()

    def _do_check(self):
        targetScale = self._swarm.GetScaleTarget()
        targetScale = limit_range(self._cpu_scale, targetScale, self._max_scale)
        targetScale = limit_range(self._memory_scale, targetScale, self._max_scale)
        targetScale = limit_range(targetScale, self._min_scale, self._max_scale)
        if targetScale != self._swarm.GetScaleTarget():
            self._do_scale(targetScale)
    
class DummyMG(MicroserviceMonitoringGroup):
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