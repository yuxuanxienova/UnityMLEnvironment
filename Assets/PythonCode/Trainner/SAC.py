import torch
import torch.optim as optim
from torch.distributions import Normal
import torch.nn as nn
import numpy as np
import warnings
from typing import Union
# from utils import ReplayBuffer, get_env, run_episode
import torch.nn.functional as F
from ReplayBuffer import ReplayBuffer, PriorityReplayBuffer
warnings.filterwarnings("ignore", category=DeprecationWarning)
warnings.filterwarnings("ignore", category=UserWarning)
from torch.utils.tensorboard import SummaryWriter

class NeuralNetwork(nn.Module):
    '''
    This class implements a neural network with a variable number of hidden layers and hidden units.
    You may use this function to parametrize your policy and critic networks.
    '''
    def __init__(self, input_dim: int, output_dim: int, hidden_size: int, 
                                hidden_layers: int, activation: str, dtype=torch.float64):
        super(NeuralNetwork, self).__init__()
        
        self.dtype = dtype
        
        self.activation = activation
        #---------1.Pre Processing---------------------
        self.layer_pre = nn.Linear(input_dim, hidden_size).to(self.dtype)
        
        #---------2. Deep Layers with constant size------
        hiddenLayerLists = []
        for i in range(hidden_layers):
            hiddenLayerLists.append(nn.Linear(hidden_size,hidden_size).to(self.dtype))
            hiddenLayerLists.append(nn.ReLU())
        self.hiddenLayers = nn.Sequential(*hiddenLayerLists)
        
        #---------3. Post Processing------------
        self.layer_post = nn.Linear(hidden_size, output_dim).to(self.dtype)
        
        
    def forward(self, s: torch.Tensor) -> torch.Tensor:
        s = s.to(self.dtype)
        s = self.layer_pre(s)
        # s = F.relu(s)
        s = F.leaky_relu(s)
        s = self.hiddenLayers(s)
        s = self.layer_post(s)
        
        #---------Variable Activation-------
        if(self.activation == None):
            out = s
        elif(self.activation == 'relu'):
            out = F.relu(s)
        elif(self.activation == 'softmax'):
            out = F.softmax(s)
        elif(self.activation == 'tanh'):
            out = F.tanh(s)
        elif(self.activation == 'softplus'):
            out = F.softplus(s)       
        return out
    
class Actor:
    def __init__(self,hidden_size: int, hidden_layers: int, actor_lr: float,action_bound: float ,
                state_dim: int = 3, action_dim: int = 1, device: torch.device = torch.device('cpu')):
        super(Actor, self).__init__()

        self.hidden_size = hidden_size
        self.hidden_layers = hidden_layers
        self.actor_lr = actor_lr
        self.state_dim = state_dim
        self.action_dim = action_dim
        self.device = device
        self.LOG_STD_MIN = -20
        self.LOG_STD_MAX = 2
        #-----------------------
        self.action_bound = action_bound
        #-----------------------
        self.setup_actor()
        
        
        

    def setup_actor(self):
        '''
        This function sets up the actor network in the Actor class.
        '''
        #Step 1. creat two net that map state input to mean and std of f 
        # input dim(state_dim) output dim(num_action)
        activation_f_mu = None
        self.actor_f_mu_net = NeuralNetwork(input_dim=self.state_dim, output_dim=self.action_dim, hidden_size=self.hidden_size, hidden_layers=self.hidden_layers,activation=activation_f_mu ).to(self.device)

        activation_f_logStd = None
        self.actor_f_logStd_net = NeuralNetwork(input_dim=self.state_dim, output_dim=self.action_dim, hidden_size=self.hidden_size, hidden_layers=self.hidden_layers,activation=activation_f_logStd ).to(self.device)
        
        # Step 2: Initialize optimizer with combined parameters
        all_parameters = list(self.actor_f_mu_net.parameters()) + list(self.actor_f_logStd_net.parameters())
        self.optimizer = torch.optim.Adam(all_parameters, lr=self.actor_lr)
        

    def clamp_log_std(self, log_std: torch.Tensor) -> torch.Tensor:
        '''
        :param log_std: torch.Tensor, log_std of the policy.
        Returns:
        :param log_std: torch.Tensor, log_std of the policy clamped between LOG_STD_MIN and LOG_STD_MAX.
        '''
        return torch.clamp(log_std, self.LOG_STD_MIN, self.LOG_STD_MAX)

    def get_action_and_log_prob(self, state: torch.Tensor, 
                                deterministic: bool) -> (torch.Tensor, torch.Tensor):
        '''
        :param state: torch.Tensor, state of the agent
        :param deterministic: boolean, if true return a deterministic action 
                                otherwise sample from the policy distribution.
        Returns:
        :param action: torch.Tensor, action the policy returns for the state.
        :param log_prob: log_probability of the the action.
        '''
        assert state.shape == (self.state_dim,) or state.shape[1] == self.state_dim, 'State passed to this method has a wrong shape'
        
        action , log_prob = torch.zeros(state.shape[0]), torch.ones(state.shape[0])
        
        #---------------------------------------------------------
        mu = self.actor_f_mu_net(state)
        log_std = self.clamp_log_std(self.actor_f_logStd_net(state))
        std = torch.exp(log_std)


        if(deterministic):
            #Deterministic
            action = torch.tanh(mu)
            log_prob = torch.zeros((state.shape[0], self.action_dim))
            

        else:
            #Stochastic
            dist = Normal(mu, std)
            #Use rsample to sample with reparametrization: i.e. the result is differentiable w.r.t. mu and std
            normal_rsample = dist.rsample()
            log_prob = dist.log_prob(normal_rsample)
            action = torch.tanh(normal_rsample)
            # calculate tanh_normal distribution pdf
            log_prob = log_prob - torch.log(1 - torch.tanh(action).pow(2) + 1e-7)
            
            action = action * self.action_bound

        #---------------------------------------------------------------------

            
        # print("action.shape={0} should be ({1},{2})".format(action.shape,state.shape[0] ,self.action_dim ))     
        # assert action.shape == (state.shape[0], self.action_dim) and \
        #     log_prob.shape == (state.shape[0], self.action_dim), 'Incorrect shape for action or log_prob.'
        return action, log_prob


class Critic:
    def __init__(self, hidden_size: int, 
                 hidden_layers: int, critic_lr: int, action_bound:float, state_dim: int = 3, 
                    action_dim: int = 1,device: torch.device = torch.device('cpu')):
        super(Critic, self).__init__()
        self.hidden_size = hidden_size
        self.hidden_layers = hidden_layers
        self.critic_lr = critic_lr
        self.state_dim = state_dim
        self.action_dim = action_dim
        self.device = device
        #----------------------
        self.action_bound = action_bound
        #----------------------
        self.setup_critic()
        

    def setup_critic(self):
        
        #--------1. Set up the Q network--------
        #dim(state_dim + action_dim) -> dim(1)
        activation_Q = None
        self.critic_Q_net = NeuralNetwork(input_dim=self.state_dim + self.action_dim, output_dim=1, hidden_size=self.hidden_size,hidden_layers=self.hidden_layers, activation=activation_Q).to(self.device)
        
        #--------2. Set up the Q_second network--------
        #dim(state_dim + action_dim) -> dim(1)
        activation_Q_second = None
        self.critic_Q_second_net = NeuralNetwork(input_dim=self.state_dim + self.action_dim, output_dim=1, hidden_size=self.hidden_size,hidden_layers=self.hidden_layers, activation=activation_Q_second).to(self.device)
        
        #--------3. Set up the Q_target network--------
        #dim(state_dim + action_dim) -> dim(1)
        activation_Q = None
        self.critic_Q_target_net = NeuralNetwork(input_dim=self.state_dim + self.action_dim, output_dim=1, hidden_size=self.hidden_size,hidden_layers=self.hidden_layers, activation=activation_Q).to(self.device)
        
        #--------4. Set up the Q_second_target network--------
        #dim(state_dim + action_dim) -> dim(1)
        activation_Q_second = None
        self.critic_Q_second_target_net = NeuralNetwork(input_dim=self.state_dim + self.action_dim, output_dim=1, hidden_size=self.hidden_size,hidden_layers=self.hidden_layers, activation=activation_Q_second).to(self.device)
        
        self.critic_Q_target_net.load_state_dict(self.critic_Q_net.state_dict())
        self.critic_Q_second_target_net.load_state_dict(self.critic_Q_second_net.state_dict())
        #-----------------------Using Multiple optimizer------------------------
        self.optimizer_Q_net = torch.optim.Adam(self.critic_Q_net.parameters(), lr=self.critic_lr)
        self.optimizer_Q_second_net = torch.optim.Adam(self.critic_Q_second_net.parameters(), lr=self.critic_lr)
        
class TrainableParameter:
    '''
    This class could be used to define a trainable parameter in your method. You could find it 
    useful if you try to implement the entropy temerature parameter for SAC algorithm.
    '''
    def __init__(self, init_param: float, lr_param: float, 
                 train_param: bool, device: torch.device = torch.device('cpu')):
        
        self.log_param = torch.tensor(np.log(init_param), requires_grad=train_param, device=device)
        self.optimizer = optim.Adam([self.log_param], lr=lr_param)

    def get_param(self) -> torch.Tensor:
        return torch.exp(self.log_param)

    def get_log_param(self) -> torch.Tensor:
        return self.log_param


class SAC_Agent:
    def __init__(self,state_dim,action_dim):
        # Environment variables. You don't need to change this.
        self.state_dim = state_dim
        self.action_dim = action_dim

        self.batch_size = 200#200
        self.min_buffer_size = 200#1000
        self.max_buffer_size = 100000
        # If your PC possesses a GPU, you should be able to use it for training, 
        # as self.device should be 'cuda' in that case.
        self.device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        print("Using device: {}".format(self.device))
        self.memory = ReplayBuffer(self.min_buffer_size, self.max_buffer_size, self.device)
        

        
        #------------------------
        self.tau = 0.1
        self.gamma = 0.98
        self.action_bound = 1
        self.criterion = nn.MSELoss() 
        self.actor_lr = 3e-4
        self.critic_lr = 3e-3
        self.hidden_dim = 512
        #--------------------------------
        
        #----------------Module Used to Adjust Entropy---------------------------------
        # use log alpha to stablize the training
        self.target_entropy = 0
        alpha_lr = 0.001
        self.log_alpha = torch.tensor(np.log(0.001), dtype=torch.float)
        self.log_alpha.requires_grad = True  
        self.log_alpha_optimizer = torch.optim.Adam([self.log_alpha],lr=alpha_lr)
        #------------------------------------------------------------------------------
        self.setup_agent()
        
    def save_model(self, path: str):
        torch.save({
            'actor_state_dict': self.actor.actor_f_mu_net.state_dict(),
            'actor_optimizer_state_dict': self.actor.optimizer.state_dict(),
            'critic_Q_net_state_dict': self.critic.critic_Q_net.state_dict(),
            'critic_Q_target_net_state_dict': self.critic.critic_Q_target_net.state_dict(),
            'critic_Q_second_net_state_dict': self.critic.critic_Q_second_net.state_dict(),
            'critic_Q_second_target_net_state_dict': self.critic.critic_Q_second_target_net.state_dict(),
            'critic_Q_net_optimizer_state_dict': self.critic.optimizer_Q_net.state_dict(),
            'critic_Q_second_net_optimizer_state_dict': self.critic.optimizer_Q_second_net.state_dict(),
            'log_alpha': self.log_alpha,
            'log_alpha_optimizer_state_dict': self.log_alpha_optimizer.state_dict()
        }, path)

    def load_model(self, path: str):
        checkpoint = torch.load(path)
        self.actor.actor_f_mu_net.load_state_dict(checkpoint['actor_state_dict'])
        self.actor.optimizer.load_state_dict(checkpoint['actor_optimizer_state_dict'])
        self.critic.critic_Q_net.load_state_dict(checkpoint['critic_Q_net_state_dict'])
        self.critic.critic_Q_target_net.load_state_dict(checkpoint['critic_Q_target_net_state_dict'])
        self.critic.critic_Q_second_net.load_state_dict(checkpoint['critic_Q_second_net_state_dict'])
        self.critic.critic_Q_second_target_net.load_state_dict(checkpoint['critic_Q_second_target_net_state_dict'])
        self.critic.optimizer_Q_net.load_state_dict(checkpoint['critic_Q_net_optimizer_state_dict'])
        self.critic.optimizer_Q_second_net.load_state_dict(checkpoint['critic_Q_second_net_optimizer_state_dict'])
        self.log_alpha = checkpoint['log_alpha']
        self.log_alpha_optimizer.load_state_dict(checkpoint['log_alpha_optimizer_state_dict'])

    def setup_agent(self):
        self.actor = Actor(hidden_size=self.hidden_dim, hidden_layers=4, actor_lr=self.actor_lr, action_bound=self.action_bound,state_dim=self.state_dim, action_dim=self.action_dim,device=self.device)         
        self.critic = Critic(hidden_size=self.hidden_dim, hidden_layers=4,critic_lr=self.critic_lr, action_bound=self.action_bound,state_dim=self.state_dim,action_dim=self.action_dim,device=self.device)

    def get_action(self, s: np.ndarray, train: bool) -> np.ndarray:
        """
        :param s: np.ndarray, state. shape (state_dim, )
        :param train: boolean to indicate if you are in eval or train mode. 
                    You can find it useful if you want to sample from deterministic policy.
        :return: np.ndarray, action to apply on the environment, shape (action_dim,)
        """        
        #if training, we schocahsticly sample action
        if(train):
            deterministic = 0
        else:
            deterministic = 1
        
        #Convert State to torch tensor
        s = torch.tensor(s)
        #Move state to GPU
        s = s.to(self.device)
        #get action
        action, log_prob = self.actor.get_action_and_log_prob(state=s,deterministic=deterministic)
        
        #Convert action to np array
        action = action.to("cpu")
        action = action.detach().numpy().reshape(-1)
        assert action.shape == (self.action_dim,), 'Incorrect action shape.'
        assert isinstance(action, np.ndarray ), 'Action dtype must be np.ndarray' 
        return action

    @staticmethod
    def run_gradient_update_step(object: Union[Actor, Critic], loss: torch.Tensor):
        '''
        This function takes in a object containing trainable parameters and an optimizer, 
        and using a given loss, runs one step of gradient update. If you set up trainable parameters 
        and optimizer inside the object, you could find this function useful while training.
        :param object: object containing trainable parameters and an optimizer
        '''
        object.optimizer.zero_grad()
        loss.mean().backward()
        object.optimizer.step()

    def critic_target_update(self, base_net: NeuralNetwork, target_net: NeuralNetwork, 
                             tau: float, soft_update: bool):
        '''
        This method updates the target network parameters using the source network parameters.
        If soft_update is True, then perform a soft update, otherwise a hard update (copy).
        :param base_net: source network
        :param target_net: target network
        :param tau: soft update parameter
        :param soft_update: boolean to indicate whether to perform a soft update or not
        '''
        for param_target, param in zip(target_net.parameters(), base_net.parameters()):
            if soft_update:
                param_target.data.copy_(param_target.data * (1.0 - tau) + param.data * tau)
            else:
                param_target.data.copy_(param.data)
                
    def calc_target(self, rewards, next_states, deterministic):  # Calculate TD Target Q
        next_actions, log_prob = self.actor.get_action_and_log_prob(next_states,deterministic=deterministic)
        entropy = -log_prob
        q1_value = self.critic.critic_Q_target_net(torch.cat([next_states,next_actions], dim=1))
        q2_value = self.critic.critic_Q_second_target_net(torch.cat([next_states,next_actions], dim=1))
        next_value = torch.min(q1_value,
                               q2_value) + self.log_alpha.exp() * entropy
        td_target = rewards + self.gamma * next_value 
        return td_target
    def updateNetwork(self):
        '''
        This function represents one training iteration for the agent. It samples a batch 
        from the replay buffer,and then updates the policy and critic networks 
        using the sampled batch.
        '''

        # Batch sampling
        batch = self.memory.sample(self.batch_size)
        s_batch, a_batch, r_batch, s_prime_batch = batch

        #-----------0. move inputs to GPU----------
        s_batch = s_batch.to(self.device) 
        #-----------1. Update Q--------------
        #sample actions stochasticly from current policy
        Q_td_target = self.calc_target(r_batch, s_prime_batch, deterministic=False)
        loss_Q1 = torch.mean(F.mse_loss(self.critic.critic_Q_net(torch.cat([s_batch,a_batch], dim=1)) , Q_td_target.detach()))
        loss_Q2 = torch.mean(F.mse_loss(self.critic.critic_Q_second_net(torch.cat([s_batch,a_batch], dim=1)) , Q_td_target.detach()))
        
        self.critic.optimizer_Q_net.zero_grad()
        loss_Q1.backward()
        self.critic.optimizer_Q_net.step()
        
        self.critic.optimizer_Q_second_net.zero_grad()
        loss_Q2.backward()
        self.critic.optimizer_Q_second_net.step()
                
        
        #---------3. Update pi--------------------
        new_actions, log_prob = self.actor.get_action_and_log_prob(s_batch,deterministic=False)
        entropy = -log_prob
        q1_value_new = self.critic.critic_Q_net(torch.cat([s_batch,new_actions],dim=1))
        q2_value_new = self.critic.critic_Q_second_net(torch.cat([s_batch,new_actions],dim=1))
        actor_loss = torch.mean(-self.log_alpha.exp() * entropy - torch.min(q1_value_new, q2_value_new))
        
        self.actor.optimizer.zero_grad()
        actor_loss.backward()
        self.actor.optimizer.step()
        #------------**Update alpha--------------
        
        alpha_loss = torch.mean((entropy - self.target_entropy).detach() * self.log_alpha.exp())
        self.log_alpha_optimizer.zero_grad()
        alpha_loss.backward()
        self.log_alpha_optimizer.step()
        #---------4. Update V_target----------------------
        self.critic_target_update(base_net=self.critic.critic_Q_net, target_net=self.critic.critic_Q_target_net, tau=self.tau, soft_update=True)
        self.critic_target_update(base_net=self.critic.critic_Q_second_net, target_net=self.critic.critic_Q_second_target_net, tau=self.tau, soft_update=True)
        return loss_Q1, loss_Q2, actor_loss, alpha_loss
#---------------------------------------------------------------------------------------------
class SAC_PriorityRB_Agent:
    def __init__(self,state_dim,action_dim):
        # Environment variables. You don't need to change this.
        self.state_dim = state_dim
        self.action_dim = action_dim

        self.batch_size = 200#200
        self.min_buffer_size = 200#1000
        self.max_buffer_size = 100000
        # If your PC possesses a GPU, you should be able to use it for training, 
        # as self.device should be 'cuda' in that case.
        self.device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        print("Using device: {}".format(self.device))
        self.memory = PriorityReplayBuffer(self.min_buffer_size, self.max_buffer_size, self.device)
        

        
        #------------------------
        self.tau = 0.1
        self.gamma = 0.98
        self.action_bound = 1
        self.criterion = nn.MSELoss() 
        self.actor_lr = 3e-4
        self.critic_lr = 3e-3
        self.hidden_dim = 512
        #--------------------------------
        
        #----------------Module Used to Adjust Entropy---------------------------------
        # use log alpha to stablize the training
        self.target_entropy = 0
        alpha_lr = 0.001
        self.log_alpha = torch.tensor(np.log(0.001), dtype=torch.float)
        self.log_alpha.requires_grad = True  
        self.log_alpha_optimizer = torch.optim.Adam([self.log_alpha],lr=alpha_lr)
        #------------------------------------------------------------------------------
        self.setup_agent()
        
    def save_model(self, path: str):
        torch.save({
            'actor_state_dict': self.actor.actor_f_mu_net.state_dict(),
            'actor_optimizer_state_dict': self.actor.optimizer.state_dict(),
            'critic_Q_net_state_dict': self.critic.critic_Q_net.state_dict(),
            'critic_Q_target_net_state_dict': self.critic.critic_Q_target_net.state_dict(),
            'critic_Q_second_net_state_dict': self.critic.critic_Q_second_net.state_dict(),
            'critic_Q_second_target_net_state_dict': self.critic.critic_Q_second_target_net.state_dict(),
            'critic_Q_net_optimizer_state_dict': self.critic.optimizer_Q_net.state_dict(),
            'critic_Q_second_net_optimizer_state_dict': self.critic.optimizer_Q_second_net.state_dict(),
            'log_alpha': self.log_alpha,
            'log_alpha_optimizer_state_dict': self.log_alpha_optimizer.state_dict()
        }, path)

    def load_model(self, path: str):
        checkpoint = torch.load(path)
        self.actor.actor_f_mu_net.load_state_dict(checkpoint['actor_state_dict'])
        self.actor.optimizer.load_state_dict(checkpoint['actor_optimizer_state_dict'])
        self.critic.critic_Q_net.load_state_dict(checkpoint['critic_Q_net_state_dict'])
        self.critic.critic_Q_target_net.load_state_dict(checkpoint['critic_Q_target_net_state_dict'])
        self.critic.critic_Q_second_net.load_state_dict(checkpoint['critic_Q_second_net_state_dict'])
        self.critic.critic_Q_second_target_net.load_state_dict(checkpoint['critic_Q_second_target_net_state_dict'])
        self.critic.optimizer_Q_net.load_state_dict(checkpoint['critic_Q_net_optimizer_state_dict'])
        self.critic.optimizer_Q_second_net.load_state_dict(checkpoint['critic_Q_second_net_optimizer_state_dict'])
        self.log_alpha = checkpoint['log_alpha']
        self.log_alpha_optimizer.load_state_dict(checkpoint['log_alpha_optimizer_state_dict'])

    def setup_agent(self):
        self.actor = Actor(hidden_size=self.hidden_dim, hidden_layers=4, actor_lr=self.actor_lr, action_bound=self.action_bound,state_dim=self.state_dim, action_dim=self.action_dim,device=self.device)         
        self.critic = Critic(hidden_size=self.hidden_dim, hidden_layers=4,critic_lr=self.critic_lr, action_bound=self.action_bound,state_dim=self.state_dim,action_dim=self.action_dim,device=self.device)

    def get_action(self, s: np.ndarray, train: bool) -> np.ndarray:
        """
        :param s: np.ndarray, state. shape (state_dim, )
        :param train: boolean to indicate if you are in eval or train mode. 
                    You can find it useful if you want to sample from deterministic policy.
        :return: np.ndarray, action to apply on the environment, shape (action_dim,)
        """        
        #if training, we schocahsticly sample action
        if(train):
            deterministic = 0
        else:
            deterministic = 1
        
        #Convert State to torch tensor
        s = torch.tensor(s)
        #Move state to GPU
        s = s.to(self.device)
        #get action
        action, log_prob = self.actor.get_action_and_log_prob(state=s,deterministic=deterministic)
        
        #Convert action to np array
        action = action.to("cpu")
        action = action.detach().numpy().reshape(-1)
        assert action.shape == (self.action_dim,), 'Incorrect action shape.'
        assert isinstance(action, np.ndarray ), 'Action dtype must be np.ndarray' 
        return action

    @staticmethod
    def run_gradient_update_step(object: Union[Actor, Critic], loss: torch.Tensor):
        '''
        This function takes in a object containing trainable parameters and an optimizer, 
        and using a given loss, runs one step of gradient update. If you set up trainable parameters 
        and optimizer inside the object, you could find this function useful while training.
        :param object: object containing trainable parameters and an optimizer
        '''
        object.optimizer.zero_grad()
        loss.mean().backward()
        object.optimizer.step()

    def critic_target_update(self, base_net: NeuralNetwork, target_net: NeuralNetwork, 
                             tau: float, soft_update: bool):
        '''
        This method updates the target network parameters using the source network parameters.
        If soft_update is True, then perform a soft update, otherwise a hard update (copy).
        :param base_net: source network
        :param target_net: target network
        :param tau: soft update parameter
        :param soft_update: boolean to indicate whether to perform a soft update or not
        '''
        for param_target, param in zip(target_net.parameters(), base_net.parameters()):
            if soft_update:
                param_target.data.copy_(param_target.data * (1.0 - tau) + param.data * tau)
            else:
                param_target.data.copy_(param.data)
                
    def calc_target(self, rewards, next_states, deterministic):  # Calculate TD Target Q
        next_actions, log_prob = self.actor.get_action_and_log_prob(next_states,deterministic=deterministic)
        entropy = -log_prob
        q1_value = self.critic.critic_Q_target_net(torch.cat([next_states,next_actions], dim=1))
        q2_value = self.critic.critic_Q_second_target_net(torch.cat([next_states,next_actions], dim=1))
        next_value = torch.min(q1_value,
                               q2_value) + self.log_alpha.exp() * entropy
        td_target = rewards + self.gamma * next_value 
        return td_target
    def updateNetwork(self):
        '''
        This function represents one training iteration for the agent. It samples a batch 
        from the replay buffer,and then updates the policy and critic networks 
        using the sampled batch.
        '''

        # Batch sampling
        indices, s_batch, a_batch, r_batch, s_prime_batch = self.memory.sample(self.batch_size)

        #-----------0. move inputs to GPU----------
        s_batch = s_batch.to(self.device) 
        #-----------1. Update Q--------------
        #sample actions stochasticly from current policy
        Q_td_target = self.calc_target(r_batch, s_prime_batch, deterministic=False)#dim:(N_sample,N_action)
        current_Q1 = self.critic.critic_Q_net(torch.cat([s_batch,a_batch], dim=1))
        current_Q2 = self.critic.critic_Q_second_net(torch.cat([s_batch,a_batch], dim=1))
        loss_Q1 = torch.mean(F.mse_loss(current_Q1 , Q_td_target.detach()))
        loss_Q2 = torch.mean(F.mse_loss(current_Q2 , Q_td_target.detach()))
        
        self.critic.optimizer_Q_net.zero_grad()
        loss_Q1.backward()
        self.critic.optimizer_Q_net.step()
        
        self.critic.optimizer_Q_second_net.zero_grad()
        loss_Q2.backward()
        self.critic.optimizer_Q_second_net.step()

        #---------2. Update Priorities----------------
        # Compute TD errors and update priorities
        with torch.no_grad():
            td_errors_np = torch.abs(Q_td_target.mean(dim=1, keepdim=True) - torch.min(current_Q1, current_Q2)).cpu().numpy()
            Q_td_target_np = Q_td_target.mean(dim=1, keepdim=True).detach().cpu().numpy()
        priorities_np = td_errors_np + Q_td_target_np + 1e-6  # Convert to float and add small epsilon to avoid zero priority
        # Ensure non-negative priorities by adding a constant offset
        min_priority = np.min(priorities_np)
        if min_priority < 0:
            priorities_np += abs(min_priority) + 1e-6  # Shift all priorities to be non-negative
        priorities =[arr.item() for arr in priorities_np]
        self.memory.update_priorities(indices, priorities= priorities)
                
        
        #---------3. Update pi--------------------
        new_actions, log_prob = self.actor.get_action_and_log_prob(s_batch,deterministic=False)
        entropy = -log_prob
        q1_value_new = self.critic.critic_Q_net(torch.cat([s_batch,new_actions],dim=1))
        q2_value_new = self.critic.critic_Q_second_net(torch.cat([s_batch,new_actions],dim=1))
        actor_loss = torch.mean(-self.log_alpha.exp() * entropy - torch.min(q1_value_new, q2_value_new))
        
        self.actor.optimizer.zero_grad()
        actor_loss.backward()
        self.actor.optimizer.step()
        #------------**Update alpha--------------
        
        alpha_loss = torch.mean((entropy - self.target_entropy).detach() * self.log_alpha.exp())
        self.log_alpha_optimizer.zero_grad()
        alpha_loss.backward()
        self.log_alpha_optimizer.step()
        #---------4. Update V_target----------------------
        self.critic_target_update(base_net=self.critic.critic_Q_net, target_net=self.critic.critic_Q_target_net, tau=self.tau, soft_update=True)
        self.critic_target_update(base_net=self.critic.critic_Q_second_net, target_net=self.critic.critic_Q_second_target_net, tau=self.tau, soft_update=True)
        return loss_Q1, loss_Q2, actor_loss, alpha_loss
#---------------------------------------------------------------------------------------------

# This main function is provided here to enable some basic testing. 
# ANY changes here WON'T take any effect while grading.
if __name__ == '__main__':
    pass

    # TRAIN_EPISODES = 50
    # TEST_EPISODES = 200

    # # You may set the save_video param to output the video of one of the evalution episodes, or 
    # # you can disable console printing during training and testing by setting verbose to False.
    # save_video = True
    # verbose = True

    # agent = SAC_Agent()
    # env = get_env(g=10.0, train=True)

    # for EP in range(TRAIN_EPISODES):
    #     run_episode(env, agent, None, verbose, train=True)

    # if verbose:
    #     print('\n')

    # test_returns = []
    # env = get_env(g=10.0, train=False)

    # if save_video:
    #     video_rec = VideoRecorder(env, "pendulum_episode.mp4")
    
    # for EP in range(TEST_EPISODES):
    #     rec = video_rec if (save_video and EP == TEST_EPISODES - 1) else None
    #     with torch.no_grad():
    #         episode_return = run_episode(env, agent, rec, verbose, train=False)
    #     test_returns.append(episode_return)

    # avg_test_return = np.mean(np.array(test_returns))

    # print("\n AVG_TEST_RETURN:{:.1f} \n".format(avg_test_return))

    # if save_video:
    #     video_rec.close()