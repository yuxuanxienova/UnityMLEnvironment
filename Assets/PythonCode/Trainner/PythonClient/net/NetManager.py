import sys
import os 
sys.path.append(os.path.join(os.path.dirname(__file__) , '..'))
import socket
import threading
import time
from enum import Enum
from queue import Queue
from typing import Callable, Dict, List
from net.MsgBase import MsgBase
from net.ByteArray import ByteArray
# Define network events
class NetEvent(Enum):
    CONNECT_SUCC = 1
    CONNECT_FAIL = 2
    CLOSE = 3

# Define listener types
EventListener = Callable[[str], None]
MsgListener = Callable[[MsgBase], None]

class NetManagerClient:
    def __init__(self):
        # Static members
        self.socket = None
        self.read_buff = ByteArray()
        self.write_queue = Queue()
        self.is_connecting = False
        self.is_closing = False
        self.msg_list: List[MsgBase] = []
        self.msg_count = 0
        self.MAX_MESSAGE_FIRE = 10

        # Heartbeat variables
        self.is_use_ping = True
        self.ping_interval = 30
        self.last_ping_time = time.time()
        self.last_pong_time = time.time()

        # Event listeners
        self.event_listeners: Dict[NetEvent, List[EventListener]] = {}

        # Message listeners
        self.msg_listeners: Dict[str, List[MsgListener]] = {}

        # Locks
        self.msg_lock = threading.Lock()
        self.write_lock = threading.Lock()
        
        # Add PONG listener if not present
        if "MsgPong" not in self.msg_listeners:
            self.add_msg_listener("MsgPong", self.on_msg_pong)
#---------------------------------监听结构------------------------------------------
    #------------------------------1. 网络事件监听----------------------------------------
    # Add event listener
    def add_event_listener(self, net_event: NetEvent, listener: EventListener):
        if net_event not in self.event_listeners:
            self.event_listeners[net_event] = []
        self.event_listeners[net_event].append(listener)
    # Remove event listener
    def remove_event_listener(self, net_event: NetEvent, listener: EventListener):
        if net_event in self.event_listeners:
            self.event_listeners[net_event].remove(listener)
            if not self.event_listeners[net_event]:
                del self.event_listeners[net_event]
    # Fire event
    def fire_event(self, net_event: NetEvent, err: str):
        if net_event in self.event_listeners:
            for listener in self.event_listeners[net_event]:
                listener(err)

    #-------------------------------2. 网络消息监听---------------------------------------------
    # Add message listener
    def add_msg_listener(self, msg_name: str, listener: MsgListener):
        if msg_name not in self.msg_listeners:
            self.msg_listeners[msg_name] = []
        self.msg_listeners[msg_name].append(listener)
    # Remove message listener
    def remove_msg_listener(self, msg_name: str, listener: MsgListener):
        if msg_name in self.msg_listeners:
            self.msg_listeners[msg_name].remove(listener)
            if not self.msg_listeners[msg_name]:
                del self.msg_listeners[msg_name]
    # Fire message
    def fire_msg(self, msg_name: str, msg_base: MsgBase):
        if msg_name in self.msg_listeners:
            for listener in self.msg_listeners[msg_name]:
                listener(msg_base)

    #处理PONG监听的消息
    def on_msg_pong(self, msg_base: MsgBase):
        self.last_pong_time = time.time()
        
    def init_state(self):
        self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.read_buff = ByteArray()
        self.write_queue = Queue()
        self.is_connecting = False
        self.is_closing = False
        self.msg_list = []
        self.msg_count = 0
        self.last_ping_time = time.time()
        self.last_pong_time = time.time()

        # Add PONG listener if not present
        if "MsgPong" not in self.msg_listeners:
            self.add_msg_listener("MsgPong", self.on_msg_pong)

    #--------------------------------------连接-----------------------------------
    def connect(self, ip: str, port: int):
        #状态判断
        #1.已经链接时按下连接键
        if self.socket and self.socket.fileno() != -1:
            try:
                self.socket.getpeername()
                print("Connect fail, already connected!")
                return
            except socket.error:
                pass
        #2. 正在链接时按下链接键
        if self.is_connecting:
            print("Connect fail, isConnecting")
            return
        #3. 未连接是按下连接键
        self.init_state()
        self.socket.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
        self.is_connecting = True

        threading.Thread(target=self.connect_thread, args=(ip, port), daemon=True).start()

    #Connect回调
    def connect_thread(self, ip: str, port: int):
        try:
            self.socket.connect((ip, port))
            print("Socket Connect Succ")
            self.fire_event(NetEvent.CONNECT_SUCC, "")
            self.is_connecting = False

            # Start receive thread
            threading.Thread(target=self.receive_thread, daemon=True).start()
        except socket.error as e:
            print(f"Socket Connect fail: {e}")
            self.fire_event(NetEvent.CONNECT_FAIL, str(e))
            self.is_connecting = False

    #Receive回调
    def receive_thread(self):
        try:
            while True:
                data = self.socket.recv(4096)
                if not data:
                    self.close()
                    break
                with self.msg_lock:
                    self.read_buff.bytes.extend(data)
                    self.read_buff.write_idx += len(data)
                # Process received data
                self.on_receive_data()
        except socket.error as e:
            print(f"Socket Receive Fail: {e}")
            self.close()

    #处理接收到的数据
    def on_receive_data(self):
        while True:
            if self.read_buff.length <= 2:
                break

            read_idx = self.read_buff.read_idx
            bytes_data = self.read_buff.bytes
            body_length = bytes_data[read_idx + 1] << 8 | bytes_data[read_idx]
            if self.read_buff.length < body_length:
                break

            self.read_buff.read_idx += 2

            # Decode protocol name
            proto_name, name_count = MsgBase.decode_name(self.read_buff.bytes, self.read_buff.read_idx)
            if not proto_name:
                print("OnReceiveData MsgBase.DecodeName fail")
                return
            self.read_buff.read_idx += name_count

            # Decode message body
            body_count = body_length - name_count
            msg_base = MsgBase.decode(proto_name, self.read_buff.bytes, self.read_buff.read_idx, body_count)
            self.read_buff.read_idx += body_count
            self.read_buff.check_and_move_bytes()

            # Add to message queue
            self.msg_list.append(msg_base)
            self.msg_count += 1

        # Continue reading if there's more data
        if self.read_buff.length > 2:
            self.on_receive_data()

    #---------------------------关闭连接-----------------------------
    def close(self):
        #状态判断1:只有存在socket 和 建立连接后才能关闭
        if self.socket and self.socket.fileno() != -1:
            #状态判断2:正在连接时不能关闭
            if self.is_connecting:
                return
            #状态判断3:还有数据在发送时候,等待到发送完再关闭
            if not self.write_queue.empty():
                self.is_closing = True
                return
            #关闭连接
            self.socket.close()
            self.fire_event(NetEvent.CLOSE, "")
            self.socket = None

    #------------------------------发送数据--------------------------------
    def send(self, msg: MsgBase):
        #状态判断
        #1.没有socket 或者 socket已经关闭
        if not self.socket or self.socket.fileno() == -1:
            return
        #2.正在连接或者正在关闭时不能发送数据
        if self.is_connecting or self.is_closing:
            return

        # Encode message
        name_bytes = MsgBase.encode_name(msg)
        body_bytes = MsgBase.encode(msg)
        length = len(name_bytes) + len(body_bytes)
        send_bytes = length.to_bytes(2, byteorder='little') + name_bytes + body_bytes

        self.write_queue.put(send_bytes)

        # Start send thread if queue size is 1
        if self.write_queue.qsize() == 1:
            threading.Thread(target=self.send_thread, daemon=True).start()
    #发送数据回调
    def send_thread(self):
        #状态判断
        while not self.write_queue.empty():
            send_bytes = self.write_queue.queue[0]
            try:
                sent = self.socket.send(send_bytes)
                if sent < len(send_bytes):
                    self.write_queue.queue[0] = send_bytes[sent:]
                    break
                else:
                    self.write_queue.get()
            except socket.error as e:
                print(f"Socket Send Fail: {e}")
                self.close()
                break

        if self.is_closing and self.write_queue.empty():
            self.socket.close()
            self.fire_event(NetEvent.CLOSE, "")
   #-------------------Update-------------------------------
    def update(self):
        self.msg_update()
        self.ping_update()

    def msg_update(self):
        if self.msg_count == 0:
            return

        for _ in range(min(self.MAX_MESSAGE_FIRE, self.msg_count)):
            if not self.msg_list:
                break
            msg_base = self.msg_list.pop(0)
            self.msg_count -= 1
            self.fire_msg(msg_base.proto_name, msg_base)

    def ping_update(self):
        current_time = time.time()
        if self.is_use_ping:
            if current_time - self.last_ping_time > self.ping_interval:
                msg_ping = MsgBase("MsgPing", b'')  # Adjust according to your MsgPing implementation
                self.send(msg_ping)
                self.last_ping_time = current_time

            if current_time - self.last_pong_time > self.ping_interval * 4:
                self.close()

# Example usage
if __name__ == "__main__":
    SERVER_HOST = "127.0.0.1";
    SERVER_PORT = 65432;
    def on_connect_success(err):
        print("Connected successfully!")

    def on_connect_fail(err):
        print(f"Failed to connect: {err}")

    def on_close(err):
        print("Connection closed.")

    def on_pong(msg):
        print("Received PONG")
    client = NetManagerClient()
    client.add_event_listener(NetEvent.CONNECT_SUCC, on_connect_success)
    client.add_event_listener(NetEvent.CONNECT_FAIL, on_connect_fail)
    client.add_event_listener(NetEvent.CLOSE, on_close)
    client.add_msg_listener("MsgPong", on_pong)

    client.connect(SERVER_HOST, SERVER_PORT)

    try:
        while True:
            client.update()
            time.sleep(0.016)  # Approximately 60 FPS
    except KeyboardInterrupt:
        client.close()