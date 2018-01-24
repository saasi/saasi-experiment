from .MicroserviceMonitoringGroup import MicroserviceMonitoringGroup

class DummyMG(MicroserviceMonitoringGroup):
    def __init__(self):
        super().__init__('cpu_microservice')        
        
    def _check(self):
        print('Do Nothing')