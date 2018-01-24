from .MicroserviceMonitoringGroup import MicroserviceMonitoringGroup
from . import limit_range

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

