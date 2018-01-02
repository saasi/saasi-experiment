import requests
import utils
import core


core.register(core.CpuMicroserviceMG())
core.register(core.MemoryMicroserviceMG())
core.register(core.DummyMG())
core.start_event_loop()

    