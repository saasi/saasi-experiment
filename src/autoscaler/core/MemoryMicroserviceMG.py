from .MicroserviceMonitoringGroup import MicroserviceMonitoringGroup
from . import limit_range
from . import utils
import math

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
