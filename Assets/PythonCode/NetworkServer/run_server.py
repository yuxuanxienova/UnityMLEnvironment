from net.NetManager import NetManager
SERVER_PORT = 65432
if __name__ == "__main__":
    NetManager.start_loop(SERVER_PORT)