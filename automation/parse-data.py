import glob
import re
import json
import csv 

def parseOne(filepath):
    p = re.compile(".*saasi-data/eval([1-3])/users-([0-9]+)-req-([0-9]+)-([0-9]+)-eval[0-9]+/data.json")
    with open(filepath) as f:
        data = json.load(f)
       # print data
        m = p.match(filepath)
        evalN = int(m.group(1))
        return [
            evalN,
            int(m.group(2)),
            int(m.group(3)),
            data['AvgScale'] if evalN==1 else data['AvgScale']['business_web_1'] if evalN==2 else data['AvgScale']['business_microservice'],
            'N/A' if evalN==1 else data['AvgScale']['business_web_2'] if evalN==2 else data['AvgScale']['io_microservice'],
            'N/A' if evalN==1 else data['AvgScale']['business_web_3'] if evalN==2 else data['AvgScale']['cpu_microservice'],
            'N/A' if evalN==1 or evalN==2 else data['AvgScale']['memory_microservice'],
            0,
            data['MaxScale'] if evalN==1 else data['MaxScale']['business_web_1'] if evalN==2 else data['MaxScale']['business_microservice'],
            'N/A' if evalN==1 else data['MaxScale']['business_web_2'] if evalN==2 else data['MaxScale']['io_microservice'],
            'N/A' if evalN==1 else data['MaxScale']['business_web_3'] if evalN==2 else data['MaxScale']['cpu_microservice'],
            'N/A' if evalN==1 or evalN==2 else data['MaxScale']['memory_microservice'],
            data['CpuAverage'] if evalN==1 else data['CpuAverage']['business_web_1'] if evalN==2 else data['CpuAverage']['business_microservice'],
            'N/A' if evalN==1 else data['CpuAverage']['business_web_2'] if evalN==2 else data['CpuAverage']['io_microservice'],
            'N/A' if evalN==1 else data['CpuAverage']['business_web_3'] if evalN==2 else data['CpuAverage']['cpu_microservice'],
            'N/A' if evalN==1 or evalN==2 else data['CpuAverage']['memory_microservice'],
            data['MemAverage'] if evalN==1 else data['MemAverage']['business_web_1'] if evalN==2 else data['MemAverage']['business_microservice'],
            'N/A' if evalN==1 else data['MemAverage']['business_web_2'] if evalN==2 else data['MemAverage']['io_microservice'],
            'N/A' if evalN==1 else data['MemAverage']['business_web_3'] if evalN==2 else data['MemAverage']['cpu_microservice'],
            'N/A' if evalN==1 or evalN==2 else data['MemAverage']['memory_microservice'],
            data['NetInAverage'] if evalN==1 else data['NetInAverage']['business_web_1'] if evalN==2 else data['NetInAverage']['business_microservice'],
            'N/A' if evalN==1 else data['NetInAverage']['business_web_2'] if evalN==2 else data['NetInAverage']['io_microservice'],
            'N/A' if evalN==1 else data['NetInAverage']['business_web_3'] if evalN==2 else data['NetInAverage']['cpu_microservice'],
            'N/A' if evalN==1 or evalN==2 else data['NetInAverage']['memory_microservice'],
            data['NetOutAverage'] if evalN==1 else data['NetOutAverage']['business_web_1'] if evalN==2 else data['NetOutAverage']['business_microservice'],
            'N/A' if evalN==1 else data['NetOutAverage']['business_web_2'] if evalN==2 else data['NetOutAverage']['io_microservice'],
            'N/A' if evalN==1 else data['NetOutAverage']['business_web_3'] if evalN==2 else data['NetOutAverage']['cpu_microservice'],
            'N/A' if evalN==1 or evalN==2 else data['NetOutAverage']['memory_microservice'],
            data['WorkloadTotalCpu'],
            data['ControllerTotalCpu'],
            0,
            0,
            0,
            data['WorkloadTotalMemory'],
            data['ControllerTotalMemory'],
            0,
            data['WorkloadNetInTotal'],
            data['ControllerNetInTotal'],        
            0,
            data['WorkloadNetOutTotal'],
            data['ControllerNetOutTotal'],        
            0,
            0,
            0,
            0,
            data['TotalRequests'],
            data['TotalViolations'],
            0
        ]


dataFiles = glob.glob("/home/ztl8702/saasi-data/eval[1-3]/users-*-req-*-*-*/data.json")

data = [parseOne(s) for s in dataFiles]
data = sorted(data, key=lambda s: (s[0],s[1],s[2]))
print data

with open('data.csv', 'w') as csvfile:
    writer = csv.writer(csvfile)
    for line in data:
        writer.writerow(line)
        