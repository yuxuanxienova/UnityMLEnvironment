import os
import sys
sys.path.append(os.path.join(os.path.dirname(__file__), '..'))
from net.MsgBase import MsgBase
#----------------System Message----------------
class MsgPing(MsgBase):
    def __init__(self):
        super().__init__()
        self.protoName = "MsgPing"

class MsgPong(MsgBase):
    def __init__(self):
        super().__init__()
        self.protoName = "MsgPong"
        
        
#---------------Test Message----------------
class MyMessage(MsgBase):
    def __init__(self):
        super().__init__()
        self.protoName = "MyMessage"
        self.content = ""
class MsgTest(MsgBase):
    def __init__(self):
        super().__init__()
        self.protoName = "MsgTest"
        self.str = ""
        self.num = 0