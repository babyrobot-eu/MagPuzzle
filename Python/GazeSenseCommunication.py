import GazeSense as GS
import socket
import sched, time
import math
import time

host = ''
clientConnected = False
port = 8081
backlog = 5
size = 1024
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.bind((host,port))
s.listen(backlog)

def my_callback(data):
    """
    Callback function example. Simply follow this signature, and you will receive a dictionary with the following items

    data['GazeCoding'] <- String with the current gazed target
    data['InTracking'] <- Boolean to indicate the subject is being tracked
    data['ConnectionOK'] <- Boolean to indicate there is a connection to the GazeSense application

    It will be called when:
    - A new data frame has been processed and gaze coding data is available
    - There is change in either the tracking status or the connection status
    """
    global clientConnected
    global lastFrameSent
    #print '====== Status ======'
    #print 'GazeCoding:', data['GazeCoding']
    #print 'InTracking:', data['InTracking']
    #print 'ConnectionOK:', data['ConnectionOK']
    biggestConfidence = 0
    biggestLabel = ''
    for label in data['Attention Scores']:
        confidence = data['Attention Scores'][label]
        if confidence > biggestConfidence:
            biggestConfidence = confidence
            biggestLabel = label
        #print label, '%.3f' % float(data['Attention Scores'][label])

    if(biggestConfidence > 0.5):
        string = 'M' +biggestLabel
    else:
        string = 'M' + 'Unknown'
    string+=';'
    if(len(gc.eye_contact_point) == 3):
        if(gc.eye_contact_point[0] == 0 and gc.eye_contact_point[1] == 0 and gc.eye_contact_point[2] == 0):
            string = 'MNone;'
            string+=str(0)
            string += ','
            string +=str(0.01)
            string += ','
            string +=str(1.01)
        else:
            string+=str(round(gc.eye_contact_point[0],2))
            string += ','
            string +=str(round(gc.eye_contact_point[2]-0.3,2))
            string += ','
            string +=str(-round(gc.eye_contact_point[1],2))
    if clientConnected:
        if biggestLabel <> "":
            #print 'sending: ' + biggestLabel
            try:
                client.send(string)
                print(string)
                # except (error_reply, error_perm, error_temp):
                #     clientConnected = False
            except socket.error as error:
                clientConnected = False
        else:
            client.send(string)
            print(string)



gc = GS.GazeSenseClient(server_host_ip='localhost', server_port=12000, port_to_broadcast=12001,
                        callback=my_callback, verbose=True)

try:
    gc.start()
    start_time = time.time()
    while True:
        client, address = s.accept()
        clientConnected = True
        print ("Client Connected")
    # s = sched.scheduler(time.time, time.sleep)
        # s.enter(2,1,do_something, (s,))
        # s.run()
        #time.sleep(0.05)
    time.sleep(1.0)
finally:
    # clean up
    gc.stop()