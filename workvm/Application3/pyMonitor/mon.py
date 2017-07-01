import docker
from datetime import datetime, timedelta
import socket
from threading import Thread
import time
from subprocess import call
from utils import ServiceType

interval = 1 # interval between each stats collection
CPUViolationCounter = 0
MemoryViolationCounter = 0
IOViolationCounter = 0
cpuViolationThreshold = 80.0
memoryViolationThreshold = 40.0
IOViolationThreshold = 30.0

bmsNum = 1
ioNum = 1
cpuNum = 1
memNum = 1

cli = docker.from_env()


def getUsage(container, sType=ServiceType.IO_Microservice):
    '''
    params:
        container: container object
    '''
    stats = container.stats(stream=False)
    if (sType == ServiceType.Memory_Microservice):
        return stats['memory_stats']['usage']
    elif (sType== ServiceType.CPU_Microservice):
        return stats['cpu_stats']['cpu_usage']['total_usage'] #?
    elif (sType == ServiceType.IO_Microservice):
        alist = stats['blkio_stats']['io_service_bytes_recursive']
        filterList = [x for x in alist if x['op']=='Total']
        print(filterList)
        if len(filterList)>0:
            return filterList[0]['value']
        else:
            return 0 # No IO stats

def getVmAddress():
    myname = socket.getfqdn(socket.gethostname())
    myaddr = socket.gethostbyname(myname)
    return myaddr

def getImageName(img):
    return img.attrs['RepoTags'][0].split(':')[0]

def getContainerType(container):
    imageName = getImageName(container.image)
    if (imageName == 'io_microservice'):
        return ServiceType.IO_Microservice
    elif (imageName == 'cpu_microservice'):
        return ServiceType.CPU_Microservice
    elif (imageName == 'memory_microservice'):
        return ServiceType.Memory_Microservice
    else:
        return ServiceType.Unknown

def getContainerList():
    global cli
    return cli.containers.list()

def scaleOut(sType): 
    global bmsNum, cpuNum, ioNum, memNum
    if (sType == ServiceType.Business_Microservice):
        print("Scaleout bms")
        bmsNum += 1
        call(["sudo", "docker-compose", "scale", "businessfunction="+str(bmsNum)])
    elif (sType == ServiceType.IO_Microservice):
        ioNum += 1
        print("Scaleout io ->", ioNum)
        call(["sudo", "docker-compose", "scale", "io_microservice="+str(ioNum)])
    elif (sType == ServiceType.Memory_Microservice):
        memNum +=1 
        print("Scaleout memory ->", memNum)
        call(["sudo","docker-compose", "scale", "memory_microservice="+str(memNum)])
    elif (sType == ServiceType.CPU_Microservice):
        cpuNum += 1
        print("Scaleout cpu ->",cpuNum)
        call(["sudo", "docker-compose", "scale", "cpu_microservice="+str(cpuNum)])


def monitorBusinessTimeout():
    import pika
    print("start listening to business timeout")
    queue_name = 'monitor_queue'
    conn = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
    channel = connection.channel()
    channel.exchange_declare(exchange='dm',type='direct')
    channel.queue_declare(queue=queue_name,durable=True, exclusive=False, autoDelete=False,arguments=None)
    channel.queue_bind(exchange='dm', queue=queue_name, routingKey='scaleout')
    def callback(ch, method, properties, body):
        print("Scaleout: "+body)

    channel.basic_consume(callback, queue=queue_name, no_ack=True)
    while True:
        time.sleep(500)

def writeStats(sType, containerId, usage):
    if (sType == ServiceType.IO_Microservice):
        prefix = 'io'
    elif (sType == ServiceType.CPU_Microservice):
        prefix = 'cpu'
    elif (sType == ServiceType.Memory_Microservice):
        prefix = 'memory'
    with open('logs/'+prefix+'.txt', 'a') as file:
        file.write(containerId+' '+str(usage)+' '+str(datetime.now())+'\n')

def writeBmsScaleout(bmsguid):
    with open('logs/business.txt', 'a') as file:
        file.write(bmsguid+' '+ str(datetime.now())+'\n')

def writeApiScaleout(sType):
    if (sType == ServiceType.IO_Microservice):
        prefix = 'io'
    elif (sType == ServiceType.CPU_Microservice):
        prefix = 'cpu'
    elif (sType == ServiceType.Memory_Microservice):
        prefix = 'memory'
    with open('logs/api-scaleout.txt', 'a') as file:
        file.write(prefix+' '+ str(datetime.now())+'\n')

def sendVMInfo():
    global vmaddress
    import requests
    url = "http://10.137.0.81:5000/BusinessContainer?adress=" + vmaddress
    try:
        requests.get(url)
    except:
        print("Network Error")


def checkViolationCPU(container):
    global cpuViolationThreshold
    usage = getUsage(container, ServiceType.CPU_Microservice)
    if (usage > cpuViolationThreshold):
        return True
    else:
        return False

def checkViolationIO(container):
    global IOViolationThreshold
    usage = getUsage(container, ServiceType.IO_Microservice)
    if (usage > IOViolationThreshold):
        return True
    else:
        return False

def checkViolationMemory(container):
    global memoryViolationThreshold
    usage = getUsage(container, ServiceType.Memory_Microservice)
    if (usage > memoryViolationThreshold):
        return True
    else:
        return False

def checkViolation( sType, containerList):
    if (sType == ServiceType.IO_Microservice):
        func = checkViolationIO
    elif (sType == ServiceType.CPU_Microservice):
        func = checkViolationCPU
    elif (sType == ServiceType.Memory_Microservice):
        func = checkViolationMemory
    for container in containerList:
        if (func(container)):
            return True
    return False
    
if __name__ == '__main__':
    vmaddress = getVmAddress()
    containerViolation = {}
    lastScaleTime = {}
    print("IP:", vmaddress)
    sendVMInfo()
    print("VM info sent")
    Thread(target = monitorBusinessTimeout).start()
    while(True):
        containers = getContainerList()
        print("Updated container list")

        serviceContainers = {ServiceType.CPU_Microservice:[], ServiceType.IO_Microservice:[], ServiceType.Memory_Microservice:[]}

        # classify containers
        for container in containers:
            sType = getContainerType(container)
            print(container.id, sType)
            if sType in [ServiceType.CPU_Microservice, ServiceType.IO_Microservice, ServiceType.Memory_Microservice]:
                writeStats(sType, container.id, getUsage(container,sType))
                serviceContainers[sType].append(container)

        # scale out by category
        for sType in [ServiceType.CPU_Microservice, ServiceType.IO_Microservice, ServiceType.Memory_Microservice]:
            violated = checkViolation(sType, serviceContainers[sType])
            if (violated):
                if (sType not in lastScaleTime or (lastScaleTime[sType]+timedelta(minutes=1)<datetime.now())):
                    scaleOut(sType)
                    writeApiScaleout(sType)
                    lastScaleTime[sType] = datetime.now()


        time.sleep(interval)
