from enum import Enum

class ServiceType(Enum):
    IO_Microservice = 1
    CPU_Microservice = 2
    Memory_Microservice = 3
    Business_Microservice = 4
    Business_WebAPI = 5
    Unknown = 6