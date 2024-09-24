import os
import sys
sys.path.append(os.path.join(os.path.dirname(__file__), '..'))
from NetworkServer.net.NetManager import NetManagerServer, EventHandler, MsgHandler
from NetworkServer.net.ClientState import ClientState
from NetworkServer.proto.Messages import MsgPing, MsgPong, MsgTest, TransitionMsg, SampleActionRequestMsg, SampleActionResponseMsg
from Trainner.PythonClient.net.NetManager import NetManagerClient, NetEvent
import numpy as np
import torch
import PIL
from SAC import SAC_Agent, SAC_PriorityRB_Agent
import concurrent.futures
from torch.utils.tensorboard import SummaryWriter
import shutil
import time

#---------------------------------utils-----------------------------------
def clean_log_dir(log_dir):
    if os.path.exists(log_dir):
        shutil.rmtree(log_dir)
    os.makedirs(log_dir)

#----------------------Node--------------------------

class Trainner:
    def __init__(self) -> None:
        #Parameters
        self.state_dim = 73#73
        self.action_dim = 16#16
        self.trainTime=1000
        self.log_dir= "./catkin_ws/src/RL_trainner_pkg/logs"
        self.save_dir = "./catkin_ws/src/RL_trainner_pkg/models/"
        self.save_interval = 1000
        self.log_interval = 20
        # initialize components
        clean_log_dir(self.log_dir)
        self.summary_writer = SummaryWriter(log_dir=self.log_dir)
        self.threadPoolExecutor = concurrent.futures.ThreadPoolExecutor(max_workers=20)
        #Storage Field        
        self.agent = SAC_Agent(self.state_dim,self.action_dim)
        self.num_update=0
        self.timePassed=0
        
        #Initialize NetManagerClient
        self.client = NetManagerClient()
        def on_connect_success(err):
            print("Connected successfully!")
        def on_connect_fail(err):
            print(f"Failed to connect: {err}")
        def on_close(err):
            print("Connection closed.")
        def on_pong(msg):
            print("Received PONG")
        def TransitionMsgHandler(msg_base):
            state = np.array(msg_base.state)
            action = np.array(msg_base.action)
            reward = np.array(msg_base.reward)
            next_state = np.array(msg_base.next_state)
            trancated_flag = msg_base.trancated_flag
            transition = (state,action,reward,next_state)
            print("[INFO][TransitionMsgHandler]transition=(0)".format(transition))
            if(state.shape[0]==self.state_dim and action.shape[0]==self.action_dim and next_state.shape[0]==self.state_dim):
                self.agent.memory.put(transition)
                        
        self.client.add_event_listener(NetEvent.CONNECT_SUCC, on_connect_success)
        self.client.add_event_listener(NetEvent.CONNECT_FAIL, on_connect_fail)
        self.client.add_event_listener(NetEvent.CLOSE, on_close)
        self.client.add_msg_listener("MsgPong", on_pong)
        self.cloent.add_msg_listener("TransitionMsg", TransitionMsgHandler)

    def run(self):
        
        # Initialize time trackers for each function
        last_network_update = time.time()
        last_info_update = time.time()
        network_interval = 0.2  # Interval in seconds for updateNetwork
        info_interval = 1.0     # Interval in seconds for updateInfo

        while True:
            current_time = time.time()
            
            #update client
            self.client.update()
            # Check if it's time to run updateNetwork
            if current_time - last_network_update >= network_interval:
                self.updateNetwork(None)
                last_network_update = current_time  # Reset the timer

            # Check if it's time to run updateInfo
            if current_time - last_info_update >= info_interval:
                self.updateInfo(None)
                last_info_update = current_time  # Reset the timer

            time.sleep(0.01)  # Sleep briefly to prevent high CPU usage
            

    def updateNetwork(self,event):
        if(self.timePassed < self.trainTime):
            if(self.agent.memory.start_training()):
                self.num_update += 1
                loss_Q1, loss_Q2, actor_loss, alpha_loss = self.agent.updateNetwork()

                if(self.timePassed % self.log_interval == 0):
                    self.summary_writer.add_scalar("loss_Q1", loss_Q1, self.timePassed)
                    self.summary_writer.add_scalar("loss_Q2", loss_Q2, self.timePassed)
                    self.summary_writer.add_scalar("actor_loss", actor_loss, self.timePassed)
                    self.summary_writer.add_scalar("alpha_loss", alpha_loss, self.timePassed) 

                if(self.num_update % self.save_interval == 0):
                    print("[INFO][updateNetwork]Saving model at num_update:{0}".format(self.num_update))
                    self.agent.save_model(self.save_dir + str(self.num_update))
    def updateInfo(self,event):
        self.timePassed+=1
        print("[INFO][updateInfo]timePassed={0}".format(self.timePassed))
        print("[INFO][updateInfo]self.agent.memory.size={0}".format(self.agent.memory.size()))
        print("[INFO][updateInfo]num_NetowrkUpdate:{0}".format(self.num_update))

        
if __name__ == "__main__":
    
    #initialize trainner
    trainner = Trainner()
    
    #Start Trainner
    trainner.run()