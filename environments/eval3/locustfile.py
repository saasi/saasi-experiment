from locust import HttpLocust, TaskSet, task
import time
import random
from datetime import datetime
random.seed(datetime.now())

class BusinessUserBehaviour(TaskSet):
            
    def on_start(self):
        self.current_config_no = 0 # We will iterate through all the configs 
        
    @task
    def business_request(self): 
        self.client.get("/Business?operationId="+str(self.current_config_no))
        self.current_config_no = self.current_config_no + 1
        if (self.current_config_no > 5):
            self.current_config_no = 0

class BusinessUser(HttpLocust):
    task_set = BusinessUserBehaviour
    # Waiting (resting) time between requests
    min_wait = 500 # ms
    max_wait = 2000 # ms