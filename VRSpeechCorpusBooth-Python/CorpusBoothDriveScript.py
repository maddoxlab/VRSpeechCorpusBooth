'''
Author: Jin Dou
Created on 2/17/2021
'''
from pythonosc import udp_client, dispatcher, osc_server
import argparse
import numpy as np

from StellarInfra.Logger import CLog

# %% Classes for communicating with OSC-Unity
class VBControl:
    """ Parent class of virtual booth controller 
    
    Attributes
    ----------
    oClient: a reference of the initialized UDPClient object
    oDispatcher: a reference of the initialized UDPServer object
    oServer: a reference of the initialized UDPServer object
    netConfig: a tuple storing the configuration parameters of the client and server
    
    """
    def __init__(self,UDPClientType,UDPServerType,dispatcherType,cIp,sIp,cPort,sPort):
        """ initialization function
        
        Parameters
        ----------
        UDPClientType : the class of UDPClient chosen for client initialization
    
        UDPServerType : the class of UDPServer chosen for server initialization
            
        dispatcherType : the class of dispatcher chosen for dispatcher initialization
            
        cIp: the target IP address of the client you will initialize 
            
        sIp: the IP address of the server you will initialize 
            
        cPort: the target Port number of the client you will initialize (the In Port in OSC-Unity)
            
        sPort: the port number of the server you will initialize (the Out Port in OSC-Unity)
        
        """    
    
        self.oClient = UDPClientType(cIp,cPort)
        self.oDispatcher = dispatcherType()
        self.oServer = UDPServerType((sIp,sPort),self.oDispatcher)
        self.netConfig = (cIp,sIp,cPort,sPort)
    
    def addListener(self,addr,*args,**kwargs):
        """ function for mapping
        Parameters
        ----------
        the same as the dispatcher.map function
        
        """
        self.oDispatcher.map(addr,*args,**kwargs)

class VBControlSimple(VBControl):
    """ A child class of VBControl
    
    Attributes
    ----------
    oClient: a reference of the initialized UDPClient object
    oDispatcher: a reference of the initialized UDPServer object
    oServer: a reference of the initialized UDPServer object
    netConfig: a tuple storing the configuration parameters of the client and server
    
    """
    
    def __init__(self,cIp,sIp,cPort,sPort):
        """ initialization function
        
        Parameters
        ----------
        cIp: the target IP address of the client you will initialize 
            
        sIp: the IP address of the server you will initialize 
            
        cPort: the target Port number of the client you will initialize (the In Port in OSC-Unity)
            
        sPort: the port number of the server you will initialize (the Out Port in OSC-Unity)
        """   
        super().__init__(udp_client.SimpleUDPClient, \
                         osc_server.BlockingOSCUDPServer, \
                        dispatcher.Dispatcher, cIp, sIp, cPort, sPort)
        self.oServer.timeout = 10000
        
def voidFunc(address, *args):
    print(address,*args)
        
def start_trial(address, *args):
    pass

def loaded(address, *args):
    print(address,*args)    
    
def response(address, *args):
    resp_data = list(args[1:])
    print(resp_data)
    # assumes responses sent as (time, value) pairs
    for i in np.arange(0, len(resp_data), 2):
        args[0][0].write_data_line("response", [resp_data[i], resp_data[i + 1]])     
        

# %% initialize the OSC
parser = argparse.ArgumentParser()
parser.add_argument("--cIp", default="127.0.0.1")
parser.add_argument("--cPort", type=int, default=5006)
parser.add_argument("--sIp", default="127.0.0.1")
parser.add_argument("--sPort", type=int, default=6162)
args = parser.parse_args()

oLog = CLog()

oVB = VBControlSimple(args.cIp, args.sIp, args.cPort, args.sPort)
oVB.addListener('/start_ok', start_trial)
oVB.addListener('/loaded', loaded)
oVB.addListener('/calibrated', voidFunc)
oVB.addListener('/prepared', loaded)
oVB.addListener('/start_now', voidFunc)

# tell the unity to start calibrating the virtual booth
oVB.oClient.send_message('/Calibrate',[0])
# wait until the unity reply
oVB.oServer.handle_request()

# tell the Unity to rotate the virtual people
oVB.oClient.send_message('/SetTransform',[-10,20])
oVB.oServer.handle_request()
# tell the Unity to start preparing the video
oVB.oClient.send_message('/PrepareVideo',[0])
# wait for reply - there are two 'handle_request' call because we will wait
# for two videos
oVB.oServer.handle_request()
oVB.oServer.handle_request()
# tell the Unity to start playing the video
oVB.oClient.send_message('/PlayVideo',[0])
# wait for the reply that the Unity is ready to play
oVB.oServer.handle_request() 
oLog.t('start')#print the time of starting the audio
#tell the Unity to stop playing the video
oVB.oClient.send_message('/StopVideo',[0])

