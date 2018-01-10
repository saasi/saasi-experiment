import requests
import utils
import core


with open("config.txt", "r") as f:
    for line in f:
        l = line.strip()
        print("Registering",l)
        if l == "cpu_microservice":
            core.register(core.CpuMicroserviceMG(min_scale=2, max_scale=15))
        elif l == "memory_microservice":
            core.register(core.MemoryMicroserviceMG(min_scale=2, max_scale=30))
        elif l == "io_microservice":
            core.register(core.IoMicroserviceMG(min_scale=2, max_scale=20))
        elif l == "business_microservice":
            core.register(core.BusinessMicroserviceMG())
        elif l == "business_microservice2":  # eval 2
            core.register(core.MemoryMicroserviceMG(microservice_name='business_microservice', min_scale=2, max_scale=20))
        elif l == "business_web":
            core.register(core.CpuMicroserviceMG('business_web'))

core.start_event_loop()

