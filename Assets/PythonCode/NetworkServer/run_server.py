import os
import sys
sys.path.append(os.path.join(os.path.dirname(__file__), '..'))
from NetworkServer.net.NetManager import NetManagerServer, EventHandler, MsgHandler
from NetworkServer.net.ClientState import ClientState
from NetworkServer.proto.Messages import MsgPing, MsgPong, MsgTest, TransitionMsg, SampleActionRequestMsg, SampleActionResponseMsg
import numpy as np    
    

if __name__ == "__main__":
    SERVER_PORT = 65432;
    server = NetManagerServer()
    #Define MsgHandler
    @staticmethod
    def TransitionMsgHandler(client:ClientState, msg_base):
        # print("[Msg Received][client:{0}]TransitionMsg".format(client.client_address) + str(msg_base.state) + str(msg_base.action) + str(msg_base.reward) + str(msg_base.next_state) + str(msg_base.trancated_flag))
        state = np.array(msg_base.state)
        action = np.array(msg_base.action)
        reward = np.array(msg_base.reward)
        next_state = np.array(msg_base.next_state)
        trancated_flag = msg_base.trancated_flag
        transition = (state,action,reward,next_state)
        # if(state.shape[0]==server.trainner.state_dim and action.shape[0]==server.trainner.action_dim and next_state.shape[0]==server.trainner.state_dim):
        #     server.trainner.agent.memory.put(transition)
    
    @staticmethod
    def SampleActionRequestMsgHandler(client:ClientState, msg_base):
        # print("[Msg Received][client:{0}]SampleActionMsg".format(client.client_address))
        state = np.array(msg_base.state)
        # action = server.trainner.agent.get_action(state,train=False)
        # response = SampleActionResponseMsg()
        # response.action = action.tolist()
        # NetManagerServer.send(client, response)

    @staticmethod
    def EpisodicRewardMsgHandler(client:ClientState, msg_base):
        reward = np.array(msg_base.data)
        # server.trainner.summary_writer.add_scalar("episodic_reward", reward, server.trainner.timePassed)
    #Register MsgHandler
    setattr(MsgHandler,"TransitionMsg",TransitionMsgHandler)
    setattr(MsgHandler,"SampleActionRequestMsg",SampleActionRequestMsgHandler)
    setattr(MsgHandler,"EpisodicRewardMsg",EpisodicRewardMsgHandler)
    
    server.start_loop(SERVER_PORT)

