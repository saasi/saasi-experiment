import docker

client = docker.from_env()

imgList = client.images.list()
print(imgList)

srvList = client.services.list()
print(srvList)