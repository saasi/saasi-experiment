from .MicroserviceMonitoringGroup import MicroserviceMonitoringGroup
from . import limit_range
from . import utils
import math

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
        print("###CPU", cpuTotal, targetScale, self._scale_up_rule.activeFor().total_seconds(),self._scale_down_rule.activeFor().total_seconds(),(targetScale - currentScale) / currentScale)
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


