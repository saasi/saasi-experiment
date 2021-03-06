
from .MemoryMicroserviceMG import MemoryMicroserviceMG

class MemoryMicroserviceMGStub(MemoryMicroserviceMG):
    def __init__(self, scaleout_func, microservice_name = 'memory_microservice', min_scale = 1, max_scale = 40,  threshold =  150 * 1024.0 * 1024.0):
        super().__init__(microservice_name,min_scale,max_scale,threshold)
        self._scaleout_func = scaleout_func

    def _do_scale(self, targetScale):
        print("Passing scale to ...")
        self._scaleout_func(targetScale)

