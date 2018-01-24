from .MicroserviceMonitoringGroup import MicroserviceMonitoringGroup
from .MemoryMicroserviceMGStub import MemoryMicroserviceMGStub
from .CpuMicroserviceMGStub import CpuMicroserviceMGStub
from .IoMicroserviceMGStub import IoMicroserviceMGStub
from . import limit_range

class BusinessWebMG(MicroserviceMonitoringGroup):
    def __init__(self, microservice_name = 'business_web', min_scale = 1, max_scale = 40):
        super().__init__(microservice_name)
        self._min_scale = min_scale
        self._max_scale = max_scale
        self._cpu_mg = CpuMicroserviceMGStub(self._cpu_callback, microservice_name='business_web', min_scale=min_scale, max_scale=max_scale, threshold= 30.0)
        self._memory_mg = MemoryMicroserviceMGStub(self._memory_callback,  microservice_name='business_web', min_scale=min_scale, max_scale=max_scale, threshold=400 * 1024.0 * 1024.0)
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
