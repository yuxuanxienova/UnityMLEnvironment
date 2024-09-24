import sys
import os
sys.path.append(os.path.join(os.path.dirname(__file__) , '..'))
import socket
import select
import time
from typing import List, Dict
from net.ClientState import ClientState
from net.ByteArray import ByteArray
from net.MsgBase import MsgBase
from proto.Messages import MsgPing, MsgPong , MsgTest
class EventHandler:
    @staticmethod
    def on_disconnect():
        print("Close")

    @staticmethod
    def on_timer():
        EventHandler.check_ping()

    # Ping check
    @staticmethod
    def check_ping():
        # Current timestamp
        time_now = NetManagerServer.get_time_stamp()

        # Iterate and remove
        for s in list(NetManagerServer.clients.values()):
            if time_now - s.last_ping_time > NetManagerServer.ping_interval * 4:
                print(f"Ping Close {s.socket.getpeername()}")
                NetManagerServer.close(s)
                # Since 'close' removes the client from the list,
                # we break out of the loop to avoid iteration errors
                break
class MsgHandler:
    @staticmethod
    def MsgPing(client:ClientState, msg_base):
        print("MsgPing")
        client.last_ping_time = NetManagerServer.get_time_stamp()
        msg_pong = MsgPong()
        NetManagerServer.send(client, msg_pong)
        
    @staticmethod
    def MsgTest(client:ClientState, msg_base):
        print("[Msg Received][client:{0}]MsgTest".format(client.client_address) + msg_base.str + str(msg_base.num) )
        msg_test = MsgTest()
        msg_test.str = "Hi from server"
        msg_test.num = 123
        NetManagerServer.send(client, msg_test)
        
    # @staticmethod
    # def TransitionMsg(client:ClientState, msg_base):
    #     # print("[Msg Received][client:{0}]TransitionMsg".format(client.client_address) + str(msg_base.state) + str(msg_base.action) + str(msg_base.reward) + str(msg_base.next_state) + str(msg_base.trancated_flag))


class NetManagerServer:
    # Listening socket
    listen_fd = None

    # Client sockets and states
    clients:Dict[socket.socket,ClientState] = {}

    # Select read list
    check_read = []

    # Ping interval
    ping_interval = 30

    @staticmethod
    def start_loop(listen_port):
        # Create socket
        NetManagerServer.listen_fd = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        NetManagerServer.listen_fd.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

        # Bind
        NetManagerServer.listen_fd.bind(('0.0.0.0', listen_port))

        # Listen
        NetManagerServer.listen_fd.listen()
        print("Server started successfully")

        # Main loop
        while True:
            NetManagerServer.reset_check_read()

            # Use select to wait for sockets to be ready
            read_sockets, _, _ = select.select(NetManagerServer.check_read, [], [], 1)

            for s in read_sockets:
                if s == NetManagerServer.listen_fd:
                    NetManagerServer.read_listen_fd()
                else:
                    NetManagerServer.read_client_fd(s)

            # Handle timeouts
            NetManagerServer.timer()

    @staticmethod
    def reset_check_read():
        NetManagerServer.check_read = [NetManagerServer.listen_fd]
        for state in NetManagerServer.clients.values():
            NetManagerServer.check_read.append(state.socket)

    @staticmethod
    def read_listen_fd():
        try:
            client_fd, addr = NetManagerServer.listen_fd.accept()
            print(f"Accepted connection from {addr}")
            state = ClientState(client_socket=client_fd)
            state.last_ping_time = NetManagerServer.get_time_stamp()
            NetManagerServer.clients[client_fd] = state
        except socket.error as ex:
            print(f"Accept failed: {ex}")

    @staticmethod
    def read_client_fd(client_fd):
        state:ClientState = NetManagerServer.clients.get(client_fd)
        read_buffer = state.read_buffer

       
        try:
            # Receive data
            data = client_fd.recv(read_buffer.remain)
            if not data:
                print(f"Client disconnected: {client_fd.getpeername()}")
                NetManagerServer.close(state)
                return
            # Write data to buffer
            read_buffer.write(data, 0, len(data))

            # Process received data
            NetManagerServer.on_receive_data(state)

            # Move buffer if necessary
            read_buffer.check_and_move_bytes()

        except socket.error as ex:
            print(f"Receive socket exception: {ex}")
            NetManagerServer.close(state)

    @staticmethod
    def close(state):
        # Event handling
        EventHandler.on_disconnect(state)

        # Close socket and remove from clients
        state.socket.close()
        del NetManagerServer.clients[state.socket]

    @staticmethod
    def on_receive_data(state):
        read_buffer = state.read_buffer

        while True:
            # Check if we have enough data for the message length
            if read_buffer.length < 2:
                break

            # Peek message length
            body_length = int.from_bytes(read_buffer.bytes[read_buffer.read_idx:read_buffer.read_idx+2], byteorder='little')

            # Check if we have the full message
            if read_buffer.length < 2 + body_length:
                break

            read_buffer.read_idx += 2  # Move past the length field

            # Decode message name
            proto_name, name_count = MsgBase.decode_name(read_buffer.bytes, read_buffer.read_idx)
            if not proto_name:
                print("Failed to decode message name")
                NetManagerServer.close(state)
                return

            read_buffer.read_idx += name_count

            # Decode message body
            body_count = body_length - name_count
            msg_base = MsgBase.decode(proto_name, read_buffer.bytes, read_buffer.read_idx, body_count)
            read_buffer.read_idx += body_count

            # Handle message
            print(f"Received message: {proto_name}")
            handler = getattr(MsgHandler, proto_name, None)
            if handler:
                handler(state, msg_base)
            else:
                print(f"No handler for message: {proto_name}")

            # Move buffer if necessary
            read_buffer.check_and_move_bytes()

    @staticmethod
    def timer():
        # Event handling
        EventHandler.on_timer()

    @staticmethod
    def send(cs, msg):
        # Check state
        if cs is None or cs.socket.fileno() == -1:
            return

        # Encode data
        name_bytes = MsgBase.encode_name(msg)
        body_bytes = MsgBase.encode(msg)
        length = len(name_bytes) + len(body_bytes)
        send_bytes = bytearray(2 + length)

        # Pack length
        send_bytes[0:2] = length.to_bytes(2, byteorder='little')

        # Pack name
        send_bytes[2:2+len(name_bytes)] = name_bytes

        # Pack body
        send_bytes[2+len(name_bytes):] = body_bytes

        # Send data
        try:
            cs.socket.sendall(send_bytes)
        except socket.error as ex:
            print(f"Socket closed on sendall: {ex}")
            NetManagerServer.close(cs)

    @staticmethod
    def get_time_stamp():
        return int(time.time())
