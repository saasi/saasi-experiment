import docker
from datetime import datetime, timedelta
import socket
from threading import Thread
import time
from subprocess import call

interval = 1 # interval between each stats collection
CPUViolationCounter = 0
MemoryViolationCounter = 0
IOViolationCounter = 0
cpuViolationThresdhold = 80.0
memoryViolationThreshold = 40.0
IOViolationThresdhold = 30.0

bmsNum = 1
ioNum = 1
cpuNum = 1
memNum = 1

cli = docker.from_env()


def getUsage(container, sType='io'):
    '''
    params:
        container: container object
    '''
    stats = container.stats()
    if (sType =='memory'):
        return stats['memory_stats']['usage']
    elif (sType=='cpu'):
        return stats['cpu_stats']['total_usage'] #?
    elif (sType=='io'):
        return stats['memory_stats']['io_service_bytes_recursive']

def getVmAddress():
    myname = socket.getfqdn(socket.gethostname())
    myaddr = socket.gethostbyname(myname)
    return myaddr

def getContainerList():
    global cli
    return cli.containers.list()

def scaleOut(sType): 
    if (sType == 'bms'):
        print("Scaleout bms")
        call("sudo docker-compose scale businessfunction="+str(bmsNum))
    elif (sType == 'io'):
        print("Scaleout io")
        call("sudo docker-compose scale io_microservice="+str(ioNum))
    elif (sType == 'memory'):
        print("Scaleout memory")
        call("sudo docker-compose scale memory_microservice="+str(memNum))
    elif (sType == 'cpu'):
        print("Scaleout cpu")
        call("sudo docker-compose scale cpu_microservice="+str(cpuNum))


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
    with open(sType+'.txt', 'a') as file:
        file.write(containerId+' '+usage+' '+str(datetime.now())+'\n')

def writeBmsScaleout(bmsguid):
    with open('business.txt', 'a') as file:
        file.write(bmsguid+' '+ str(datetime.now())+'\n')

def writeApiScaleout(sType):
    with open('api-scaleout.txt', 'a') as file:
        file.write(sType+' '+ str(datetime.now())+'\n')

def sendVMInfo():
    global vmaddress
    import requests
    url = "http://10.137.0.81:5000/BusinessContainer?adress=" + vmaddress
    try:
        requests.get(url)
    except:
        print("Network Error")


if __name__ == '__main__':
    vmaddress = getVmAddress()
    containerViolation = {}
    print("IP:", vmaddress)
    sendVMInfo()
    print("VM info sent")
    Thread(target = monitorBusinessTimeout).start()
    while(True):
        containers = getContainerList()
        print("Updated container list")
        time.sleep(interval)
        for container in containers:
            if container.id not in containerViolation:
                containerViolation[container.id] = 0
            if container.image == 'io_microservice':
                usage = getUsage(container,"io")
                if (usage > IOViolationThresdhold):
                    containerViolation[container.id] += 1
                    print("IO violation:", container.id, containerViolation[container.id])
                    if (containerViolation[container.id] >= 5): 
                        if ((container.id not in lastScaleTime) or (lastScaleTime[container.id]+timedelta(minutes=1)<datetime.now())):
                            ioNum +=1
                            writeApiScaleout('io')
                            scaleOut('io')
                            lastScaleTime[container.id] = datetime.now()
                        containerViolation[container.id]=0

            elif container.image == 'cpu_microservice':
                usage = getUsage(container, 'cpu')
                if (usage > cpuViolationThresdhold):
                    containerViolation[container.id] += 1
                    print("CPU violation",container.id, containerViolation[container.id])
                    if (containerViolation[container.id] >= 5): 
                        if ((container.id not in lastScaleTime) or (lastScaleTime[container.id]+timedelta(minutes=1)<datetime.now())):
                            cpuNum +=1 
                            writeApiScaleout('cpu')
                            scaleOut('cpu')
                            lastScaleTime[container.id] = datetime.now()
                        containerViolation[container.id]=0
            elif container.image == 'memory_microservice':
                usage = getUsage(container, 'memory')
                if (usage > cpuViolationThresdhold):
                    containerViolation[container.id] += 1
                    print("Memory violation",container.id, containerViolation[container.id])
                    if (containerViolation[container.id] >= 5): 
                        if ((container.id not in lastScaleTime) or (lastScaleTime[container.id]+timedelta(minutes=1)<datetime.now())):
                            memNum +=1
                            writeApiScaleout('memory')
                            scaleOut('memory')
                            lastScaleTime[container.id] = datetime.now()
                        containerViolation[container.id]=0
        time.sleep(interval)
