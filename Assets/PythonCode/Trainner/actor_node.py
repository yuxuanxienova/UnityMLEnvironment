import sys
import os
import rospy
import numpy as np
import torch
import PIL
from std_msgs.msg import String
from std_msgs.msg import Float32MultiArray
from RL_trainner_pkg.msg import TransitionMsg, Float32IDMsg
from RL_trainner_pkg.srv import ProcessArray, ProcessArrayResponse
from SAC import SAC_Agent
import concurrent.futures
from torch.utils.tensorboard import SummaryWriter
import shutil
import threading
#---------------------------------utils-----------------------------------
def clean_log_dir(log_dir):
    if os.path.exists(log_dir):
        shutil.rmtree(log_dir)
    os.makedirs(log_dir)

#----------------------Node--------------------------
class TrainnerNode:
    def __init__(self) -> None:
        #Parameters
        self.state_dim = 73
        self.action_dim = 16
        self.trainTime=1000
        self.log_dir= "./catkin_ws/src/RL_trainner_pkg/logs"
        self.save_dir = "./catkin_ws/src/RL_trainner_pkg/models/"
        self.save_interval = 1000
        self.log_interval = 100
        # initialize components
        clean_log_dir(self.log_dir)
        self.summary_writer = SummaryWriter(log_dir=self.log_dir)
        self.publishersDict = {}
        self.subscriberDict = {}
        self.serviceDict = {}
        self.callbackFuncToTopicName = {}
        self.handleFuncToServiceName = {}
        self.threadPoolExecutor = concurrent.futures.ThreadPoolExecutor(max_workers=20)
        #Storage Field        
        self.agent = SAC_Agent(self.state_dim,self.action_dim)
        self.lock = threading.Lock()
        self.num_update=0
        self.timePassed=0

        #load model
        model_name = "6000"
        self.agent.load_model(self.save_dir + model_name)

    def run_node(self):
        #1.Register Function Runner
        self.run_function(self.updateInfo,interval=1)
        #2. Register Publishers
        self.register_event_publisher( topic_name="/unity/RL_Agent/event_sample_action_response",data_class=Float32IDMsg)
        #3.Register Subscribers
        self.register_subscriber(topic_name="/unity/RL_Agent/event_sample_action",data_class=Float32IDMsg,callback=self.onCall_subscribeEvent_sampleAction)
        self.register_subscriber(topic_name="/unity/RL_Agent/episodic_reward",data_class=Float32IDMsg,callback=self.onCall_subscribe_episodicReward)
        #4. Register Service Server
        # for id in range(self.num_agents):
        #     self.register_service_server(service_name="/trainner_node/service/sample_action/id_{0}".format(id),service_class=ProcessArray,handle_func=self.onCall_handleService_sampleAction)
    #----------------------------------FunctionRunner---------------------------
    def run_function(self, func, interval: float):
        rospy.Timer(rospy.Duration(interval), func)
    def updateInfo(self,event):
        self.timePassed+=1
        print("[INFO][updateInfo]timePassed={0}".format(self.timePassed))
        
    # ------------------------------------Publishers-----------------------------
    def register_event_publisher(self, topic_name: str, data_class, queue_size=10):
        publisher = rospy.Publisher(name=topic_name, data_class=data_class, queue_size=queue_size)
        if topic_name in self.publishersDict:
            print("[ERROR][LMM_Sf_Node]register publisher with name:{0} twice!!".format(topic_name))
        else:
            self.publishersDict[topic_name] = publisher

    def register_run_publisher(self, topic_name: str, data_class, call_publish_func, duration: int):
        publisher = rospy.Publisher(name=topic_name, data_class=data_class, queue_size=10)
        # store topic name and publisher in a dictionary
        if topic_name in self.publishersDict:
            print("[ERROR][LMM_Sf_Node]register publisher with name:{0} twice!!".format(topic_name))
        else:
            self.publishersDict[topic_name] = publisher
        # store callback function and topic name in a dictionary
        if call_publish_func in self.callbackFuncToTopicName:
            print("[ERROR][LMM_Sf_Node]register callback func with name:{0} twice!!".format(call_publish_func))
        else:
            self.callbackFuncToTopicName[call_publish_func] = topic_name
        rospy.Timer(rospy.Duration(duration), call_publish_func)
    # ---------------------------------------Subscribers---------------------------------------------------------
    def register_subscriber(self, topic_name: str, data_class, callback):
        subscriber = rospy.Subscriber(name=topic_name, data_class=data_class, callback=callback)
        # store topic name and subscriber in a dictionary
        if topic_name in self.subscriberDict:
            print("[ERROR][LMM_Sf_Node]register subscriber with name:{0}".format(topic_name))
        else:
            self.subscriberDict[topic_name] = subscriber
        # store callback function and topic name in a dictionary
        if callback in self.callbackFuncToTopicName:
            print("[ERROR][LMM_Sf_Node]register callback func with name:{0} twice!!".format(callback))
        else:
            self.callbackFuncToTopicName[callback] = topic_name
    def onCall_subscribeEvent_sampleAction(self,msg):
        response_topic_name="/unity/RL_Agent/event_sample_action_response"
        publisher = self.publishersDict[response_topic_name]

        #Parse the message
        id = msg.id
        state = np.array(msg.data)
        # print("[INFO][onCall_handleEvent_sampleAction]Incomming request; id={0}; state={1}".format(id,state))
        #Get action
        action = self.agent.get_action(state,train=False)
        #Publish response
        response = Float32IDMsg()
        response.id = id
        response.data = action.tolist()
        publisher.publish(response)
    def onCall_subscribe_episodicReward(self,msg):
        topic_name = self.callbackFuncToTopicName[self.onCall_subscribe_episodicReward]
        # print("[INFO][onCall_subscribe_transition]state:{0};action:{1};reward:{2};next_state:{3}".format(msg.state,msg.action,msg.reward,msg.next_state))
        reward = np.array(msg.data)
        self.summary_writer.add_scalar("episodic_reward", reward, self.timePassed)
    # -------------------------------------Service----------------------------------------------------------
    def register_service_server(self,service_name:str,service_class,handle_func):
        service = rospy.Service(service_name, service_class, handle_func)


if __name__ == "__main__":
    # -----------------------Main-------------------
    rospy.init_node(name="actor_node", anonymous=True, log_level=rospy.INFO)
    try:
        node = TrainnerNode()
        node.run_node()
        rospy.spin()

    except rospy.ROSInterruptException:
        pass