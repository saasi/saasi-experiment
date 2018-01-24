import docker
from . import utils
import asyncio
import math

def limit_range(number, lower_bound, upper_bound):
    if (number < lower_bound):
        number = lower_bound
    if (number > upper_bound):
        number = upper_bound
    return number

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

from .BusinessMicroserviceMG import BusinessMicroserviceMG
from .BusinessWebMG import BusinessWebMG
from .CpuMicroserviceMG import CpuMicroserviceMG
from .CpuMicroserviceMGStub import CpuMicroserviceMGStub
from .DummyMG import DummyMG
from .IoMicroserviceMG import IoMicroserviceMG
from .IoMicroserviceMGStub import IoMicroserviceMGStub
from .MemoryMicroserviceMG import MemoryMicroserviceMG
from .MemoryMicroserviceMGStub import MemoryMicroserviceMGStub
from .MicroserviceMonitoringGroup import MicroserviceMonitoringGroup
from .CombinedMicroserviceMG import CombinedMicroserviceMG