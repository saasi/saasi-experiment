from .IoMicroserviceMG import IoMicroserviceMG

class IoMicroserviceMGStub(IoMicroserviceMG):

    def __init__(self, scaleout_func, microservice_name = 'io_microservice', min_scale = 1, max_scale = 10, threshold=48.0*2):
        super().__init__(microservice_name, min_scale, max_scale,threshold)
        self._scaleout_func = scaleout_func
 
    def _do_scale(self, targetScale):
        print("Passing scale to ...")
        self._scaleout_func(targetScale)

