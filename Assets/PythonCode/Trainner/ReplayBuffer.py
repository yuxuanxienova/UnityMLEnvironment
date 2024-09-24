
from typing import Optional
import numpy as np
import torch
import random
from collections import deque
class ReplayBuffer():
    '''
    This class implements a replay buffer for storing transitions. Upon every transition, 
    it saves data into a buffer for later learning, which is later sampled for training the agent.
    '''
    def __init__(self, min_size, max_size, device):
        self.buffer = deque(maxlen=max_size)
        self.device = device
        self.min_size = min_size

    def put(self, transition):
        self.buffer.append(transition)

    def sample(self, n):
        mini_batch = random.sample(self.buffer, n)
        s_lst, a_lst, r_lst, s_prime_lst = [], [], [], []

        for transition in mini_batch:
            s, a, r, s_prime = transition
            s_lst.append(s)
            a_lst.append(a)
            r_lst.append(r)
            s_prime_lst.append(s_prime)

        s_batch = torch.tensor(s_lst, dtype=torch.float, device = self.device)#dim:(N_sample,D_s)
        a_batch = torch.tensor(a_lst, dtype=torch.float, device = self.device)#dim:(N_sample,D_a)
        r_batch = torch.tensor(r_lst, dtype=torch.float, device = self.device)#dim:(N_sample,D_r)
        s_prime_batch = torch.tensor(s_prime_lst, dtype=torch.float, device = self.device)#dim:(N_sample,D_s)

        # Normalize rewards
        r_batch = (r_batch - r_batch.mean()) / (r_batch.std() + 1e-7)

        return s_batch, a_batch, r_batch, s_prime_batch

    def size(self):
        return len(self.buffer)

    def start_training(self):
        # Training starts when the buffer collected enough training data.
        return self.size() >= self.min_size

class PriorityReplayBuffer():
    '''
    This class implements a prioritized replay buffer for storing transitions. Transitions with higher rewards
    are sampled with higher probability for training the agent.
    '''
    def __init__(self, min_size, max_size, device, alpha=0.6):
        self.buffer = deque(maxlen=max_size)
        self.priorities = deque(maxlen=max_size)
        self.device = device
        self.min_size = min_size
        self.alpha = alpha  # Controls the level of prioritization (0 is uniform sampling, 1 is full prioritization)


    def put(self, transition):
        if len(self.priorities) > 0:
            max_priority = max(self.priorities)
        else:
            max_priority = 1.0
        self.buffer.append(transition)
        self.priorities.append(max_priority)

    def sample(self, n):
        # Calculate the probabilities of sampling each transition based on their priority
        scaled_priorities = np.array(self.priorities) ** self.alpha
        sampling_probabilities = scaled_priorities / sum(scaled_priorities)
        
        # Sample indices according to the calculated probabilities
        indices = np.random.choice(len(self.buffer), n, p=sampling_probabilities)
        mini_batch = [self.buffer[i] for i in indices]
        
        s_lst, a_lst, r_lst, s_prime_lst = [], [], [], []
        for transition in mini_batch:
            s, a, r, s_prime = transition
            s_lst.append(s)
            a_lst.append(a)
            r_lst.append(r)
            s_prime_lst.append(s_prime)

        s_batch = torch.tensor(s_lst, dtype=torch.float, device=self.device)  # dim:(N_sample,D_s)
        a_batch = torch.tensor(a_lst, dtype=torch.float, device=self.device)  # dim:(N_sample,D_a)
        r_batch = torch.tensor(r_lst, dtype=torch.float, device=self.device)  # dim:(N_sample,D_r)
        s_prime_batch = torch.tensor(s_prime_lst, dtype=torch.float, device=self.device)  # dim:(N_sample,D_s)

        # Normalize rewards
        r_batch = (r_batch - r_batch.mean()) / (r_batch.std() + 1e-7)

        return indices, s_batch, a_batch, r_batch, s_prime_batch

    def update_priorities(self, indices, priorities:float):
        # Assert that priorities is a list or array of floats
        assert isinstance(priorities, (list, np.ndarray)), "Priorities must be a list or numpy array."
        assert all(isinstance(priority, (float, int)) for priority in priorities), "All elements in priorities must be floats or ints."

        for idx, priority in zip(indices, priorities):
            self.priorities[idx] = priority 

    def size(self):
        return len(self.buffer)

    def start_training(self):
        # Training starts when the buffer collected enough training data.
        return self.size() >= self.min_size   
if __name__ == "__main__":
    min_buffer_size = 1000
    max_buffer_size = 100000
    device = torch.device("cpu")
    buffer = ReplayBuffer(min_buffer_size,max_buffer_size,device)
    transition = ( [1.0,1.1,1.2], [2.0,2.5], [33.0], [1.1,1.3,1.4])
    buffer.put(transition)
    buffer.put(transition)
    sample = buffer.sample(2)
    print(sample)