from locust import HttpLocust, TaskSet, task
import time

class BusinessUserBehaviour(TaskSet):

    config = [
        # io cpu memory timetorun timeout
        [0, 1, 0, 5, 10],
        [1, 0, 0, 5, 10],
        [0, 0, 1, 5, 10],
        [1, 1, 0, 5, 10],
        [1, 0, 1, 5, 10],
        [0, 1, 1, 5, 10],
        [0, 1, 0, 10, 30],
        [0, 0, 1, 10, 30],
        [1, 1, 0, 10, 30],
        [1, 0, 1, 10, 30],
        [0, 1, 1, 10, 30],
        [0, 1, 0, 15, 60],
        [1, 0, 0, 15, 60],
        [0, 0, 1, 15, 60],
        [1, 1, 0, 15, 60],
        [1, 0, 1, 15, 60],
        [0, 1, 1, 15, 60],
        [0, 1, 0, 20, 80],
        [1, 0, 0, 20, 80],
        [0, 0, 1, 20, 80],
        [1, 1, 0, 20, 80],
        [1, 0, 1, 20, 80],
        [0, 1, 1, 20, 80],
        [0, 1, 0, 30, 120],
        [1, 0, 0, 30, 120],
        [0, 0, 1, 30, 120],
        [1, 1, 0, 30, 120],
        [1, 0, 1, 30, 120],
        [0, 1, 1, 30, 120]
    ]
            
    def on_start(self):
        self.current_config_no = 0 # We will iterate through all the configs 
     
    @task
    def business_request(self):
        current_config = BusinessUserBehaviour.config[self.current_config_no]
        run_io = current_config[0]
        run_cpu = current_config[1]
        run_memory = current_config[2]
        time_to_run = current_config[3]
        timeout = current_config[4]

        current_timestamp =  int(time.time())
        self.current_config_no = self.current_config_no + 1
        if (self.current_config_no >= len(BusinessUserBehaviour.config)): # Finished a cycle, restart from the first config
            self.current_config_no = 0
        self.client.get("/Business?io="+str(run_io)+"&cpu="+str(run_cpu)+"&memory="+str(run_memory)+"&timestart="+ str(current_timestamp) +"&timetorun="+str(time_to_run)+"&timeout="+str(timeout))


class BusinessUser(HttpLocust):
    task_set = BusinessUserBehaviour
    # Waiting (resting) time between requests
    min_wait = 500 # ms
    max_wait = 2000 # ms