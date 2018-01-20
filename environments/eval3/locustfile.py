from locust import HttpLocust, TaskSet, task
import time
import random
from datetime import datetime
random.seed(datetime.now())

class BusinessUserBehaviour(TaskSet):

    config = [
        # io cpu memory timetorun timeout
        [0, 0, 0, 1, 5],
        [0, 0, 1, 1, 5],
        [0, 1, 0, 1, 5],
        [0, 1, 1, 1, 5],
        [1, 0, 0, 1, 5],
        [1, 0, 1, 1, 5],
        [1, 1, 0, 1, 5],
        [1, 1, 1, 1, 5]
    ]
            
    def on_start(self):
        self.current_config_no = 0 # We will iterate through all the configs 
        self._shuffled = BusinessUserBehaviour.config
        self._round = 0
        random.shuffle(self._shuffled)

    @task
    def business_request(self):
        if (self._round >= 20):
            return
        current_config = self._shuffled[self.current_config_no]
        run_io = current_config[0]
        run_cpu = current_config[1]
        run_memory = current_config[2]
        time_to_run = current_config[3]
        timeout = current_config[4]

        current_timestamp =  int(time.time())
        r = self.client.get("/Business?io="+str(run_io)+"&cpu="+str(run_cpu)+"&memory="+str(run_memory)+"&timestart="+ str(current_timestamp) +"&timetorun="+str(time_to_run)+"&timeout="+str(timeout),
            name="/Business?io="+str(run_io)+"&cpu="+str(run_cpu)+"&memory="+str(run_memory)+"&timetorun="+str(time_to_run))
        if r.status_code == 200:
            self.current_config_no = self.current_config_no + 1
            if (self.current_config_no >= len(BusinessUserBehaviour.config)): # Finished a cycle, restart from the first config
                self.current_config_no = 0
                self._round = self._round + 1
            


class BusinessUser(HttpLocust):
    task_set = BusinessUserBehaviour
    # Waiting (resting) time between requests
    min_wait = 50 # ms
    max_wait = 100 # ms