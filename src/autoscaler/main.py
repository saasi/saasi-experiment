import requests
import core


with open("config.txt", "r") as f:
    for line in f:
        l = line.strip()
        print("Registering",l)
        if l == "cpu_microservice":
            core.register(core.CpuMicroserviceMG(min_scale=1, max_scale=30, threshold=15.0))
        elif l == "memory_microservice":
            core.register(core.MemoryMicroserviceMG(min_scale=1, max_scale=30, threshold= 200.0*1024*1024))
        elif l == "io_microservice":
            core.register(core.IoMicroserviceMG(min_scale=1, max_scale=30))
        elif l == "business_microservice":
            core.register(core.BusinessMicroserviceMG())
        elif l == "business_microservice2":  # eval 2
            core.register(core.CpuMicroserviceMG(microservice_name='business_microservice', min_scale=1, max_scale=40, threshold=30.0))
        elif l == "combined_microservice":
            core.register(core.CombinedMicroserviceMG('combined_microservice',min_scale=1, max_scale=40))
        elif l == "business_web": # eval 1
            core.register(core.BusinessWebMG('business_web',min_scale=1, max_scale=40))
        elif l == "dummy":
            core.register(core.DummyMG())

core.start_event_loop()

