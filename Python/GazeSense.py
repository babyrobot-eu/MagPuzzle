"""
Copyright (c) 2017 Eyeware Tech SA, http://www.eyeware.tech

Communication interface for the GazeSense application.

Requirements:
- Zeromq: http://zeromq.org/bindings:python
"""
from threading import Thread, Lock
import json
import zmq
import time
import copy
import math


def insert_translation(dd, target_uid, translation=None, base='target'):
    if translation is None:
        translation = [0.0, 0.0, 0.0]
    dd['{}_{}_tx'.format(base, target_uid)] = translation[0]
    dd['{}_{}_ty'.format(base, target_uid)] = translation[1]
    dd['{}_{}_tz'.format(base, target_uid)] = translation[2]


def insert_rotation(dd, target_uid, rotation=None, base='target'):
    if rotation is None:
        rotation = [[1.0, 0.0, 0.0],
                    [0.0, 1.0, 0.0],
                    [0.0, 0.0, 1.0]]
    dd['{}_{}_r00'.format(base, target_uid)] = rotation[0][0]
    dd['{}_{}_r01'.format(base, target_uid)] = rotation[0][1]
    dd['{}_{}_r02'.format(base, target_uid)] = rotation[0][2]
    dd['{}_{}_r10'.format(base, target_uid)] = rotation[1][0]
    dd['{}_{}_r11'.format(base, target_uid)] = rotation[1][1]
    dd['{}_{}_r12'.format(base, target_uid)] = rotation[1][2]
    dd['{}_{}_r20'.format(base, target_uid)] = rotation[2][0]
    dd['{}_{}_r21'.format(base, target_uid)] = rotation[2][1]
    dd['{}_{}_r22'.format(base, target_uid)] = rotation[2][2]



class ZmqClient(object):
    zmq_context = zmq.Context()

    def __init__(self, host_to_subscribe='localhost', port_to_subscribe=12000, port_to_publish=12001):

        self.callback_ = None
        self.subscriber = self.zmq_context.socket(zmq.SUB)
        self.subscriber.set_hwm(5)
        connection = "tcp://%s:%d" % (host_to_subscribe, port_to_subscribe)
        self.subscriber.connect(connection)
        self.subscriber.setsockopt(zmq.SUBSCRIBE, "")

        connection = 'tcp://*:%d' % port_to_publish

        self.publisher = self.zmq_context.socket(zmq.PUB)
        self.publisher.bind(connection)

        self.sendLock = Lock()

        self.running_ = True
        self.listening_thread = Thread(target=self.receiver_loop)
        self.listening_thread.start()

        self.callback_ = None

    def __del__(self):
        self.kill()

    def kill(self):
        self.running_ = False
        if self.listening_thread:
            self.listening_thread.join()

    def set_callback(self, callback=None):
        self.callback_ = callback

    def recv_last(self):
        prev_data = None
        while True:
            try:
                data = self.subscriber.recv_json(flags=zmq.NOBLOCK)
            except:
                data = None
            if data is not None:
                if 'ArrayDesc' in data.keys():
                    msg = self.subscriber.recv()
                    data[data['ArrayDesc']] = (buffer(msg), data['dtype'], data['shape'])
                    del data['shape']
                    del data['dtype']
                    del data['ArrayDesc']
                prev_data = data
            else:
                break
        return prev_data

    def receiver_loop(self):
        while self.running_:
            data = self.recv_last()
            if data is None or self.callback_ is None:
                time.sleep(0.001)
            else:
                self.callback_(data)
            time.sleep(0.01)

    def send_dictionary(self, to_send, topic=''):
        """
        Accepts a dictionary of only numbers or strings
        """
        dd = copy.deepcopy(to_send)
        for key in dd.keys():
            if not type(dd[key]) is str:
                dd[key] = float(dd[key])
        topic_str = ''
        if topic != '':
            topic_str = '{} '.format(topic)
        self.sendLock.acquire()
        self.publisher.send_string(topic_str + json.dumps(dd))
        self.sendLock.release()


class GazeSenseClient:
    def __init__(self, server_host_ip='localhost', server_port=12000, port_to_broadcast=12001,
                 callback=None, verbose=True):
        """
        Creates a GazeSense client instance
        :param server_host_ip: The IP of the host, ie. where the GazeSense Software is running.
        :param server_port: The port of the host, ie. where the GazeSense Software is running.
        :param port_to_broadcast: The port where this client will be broadcasting instructions to the GazeSense Software
        :param callback: Callback function to wherever messages from GazeSense Software are sent
        :param verbose: Whether further information will be required in the command line
        """
        # State of the process
        self.connected_ = False  # Indicates that it is not receiving data
        self.tracking_ = False  # Indicates whether it is receiving tracking data
        self.verbose_ = verbose
        self.current_gaze = 'NA'
        self.attention_scores = {}

        # Communication with the user
        self.callback_ = callback

        # Communication with the GazeSense core
        self.server_host_ip = server_host_ip
        self.server_port = server_port
        self.port_to_broadcast = port_to_broadcast
        self.running_ = False
        self.zmq_client = None
        self.thread = None

        self.time_of_last_message = -10000000
        self.time_of_last_data = -10000000
        self.current_rgb_frame_buffer = None
        self.por_3D = 0.0, 0.0, 0.0
        self.eye_contact_point = 0.0, 0.0, 0.0

    def __del__(self):
        self.stop()

    def start(self):
        if self.zmq_client is not None:
            return
        self.zmq_client = ZmqClient(host_to_subscribe=self.server_host_ip,
                                    port_to_subscribe=self.server_port,
                                    port_to_publish=self.port_to_broadcast)
        self.zmq_client.set_callback(self.zmq_callback)

        self.running_ = True
        self.thread = Thread(target=self.events_loop)
        self.thread.start()

    def stop(self):
        if self.zmq_client is None:
            return
        self.running_ = False
        self.thread.join()

        self.zmq_client.kill()

        self.thread = None
        self.zmq_client = None

    def send_camera_pose(self, camera):
        if type(camera) is Camera:
            dd={}
            dd['camera_uid_{}'.format(camera.uid_)] = camera.uid_
            insert_rotation(dd, camera.uid_, rotation=camera.rotation_, base='camera')
            insert_translation(dd, camera.uid_, translation=camera.translation_, base='camera')
            self.zmq_client.send_dictionary(dd)

    def send_marker(self, marker):
        if type(marker) is Marker:
            dd = {}
            marker_uid = marker.uid_
            dd['marker_uid_{}'.format(marker_uid)] = marker_uid
            dd['marker_{}_size_x'.format(marker_uid)] = marker.size_[0]
            dd['marker_{}_size_y'.format(marker_uid)] = marker.size_[1]
            dd['marker_{}_p1_x'.format(marker_uid)] = marker.p1_[0]
            dd['marker_{}_p1_y'.format(marker_uid)] = marker.p1_[1]
            dd['marker_{}_p2_x'.format(marker_uid)] = marker.p2_[0]
            dd['marker_{}_p2_y'.format(marker_uid)] = marker.p2_[1]
            self.zmq_client.send_dictionary(dd)

    def send_gaze_targets_list(self, targets):
        dd = {}
        target_uid = 0
        for target in targets:
            dd['target_uid_{}'.format(target_uid)] = target_uid
            dd['target_{}_label'.format(target_uid)] = target.label_

            if type(target) is PointGazeTarget:
                dd['target_{}_type'.format(target_uid)] = 'Point'

                insert_translation(dd, target_uid=target_uid, translation=target.translation_)

            elif type(target) is PlanarGazeTarget:
                dd['target_{}_type'.format(target_uid)] = 'Plane'
                dd['target_{}_size_x'.format(target_uid)] = target.size_x_
                dd['target_{}_size_y'.format(target_uid)] = target.size_y_

                insert_translation(dd, target_uid=target_uid, translation=target.translation_)
                insert_rotation(dd, target_uid=target_uid, rotation=target.rotation_)

            elif type(target) is CylinderGazeTarget:
                dd['target_{}_type'.format(target_uid)] = 'Cylinder'
                dd['target_{}_radius'.format(target_uid)] = target.radius_
                dd['target_{}_height'.format(target_uid)] = target.height_

                insert_translation(dd, target_uid=target_uid, translation=target.translation_)
                insert_rotation(dd, target_uid=target_uid, rotation=target.rotation_)

            target_uid += 1
        if len(dd) > 0:
            self.zmq_client.send_dictionary(dd)

    def request_next_video_frame(self):
        dd = {}
        dd['rgb_frame_request'] = 0
        self.zmq_client.send_dictionary(dd)

    def is_tracking(self):
        return self.tracking_

    def is_connected(self):
        return self.connected_

    def get_current_gaze(self):
        return self.current_gaze

    def send_gaze_status(self):
        if self.callback_ is None:
            return
        data = {}
        data['GazeCoding'] = self.current_gaze
        data['InTracking'] = self.tracking_
        data['ConnectionOK'] = self.connected_
        data['Attention Scores'] = self.attention_scores
        data['rgb_video_frame_buffer'] = self.current_rgb_frame_buffer
        data['point_of_regard_3D'] = self.por_3D
        data['eye_contact_point'] = self.eye_contact_point
        self.callback_(data)

    def zmq_callback(self, data):
        current_time = time.time()
        # Color video frame
        self.current_rgb_frame_buffer = data.pop('raw_rgb', None)
        por_x = data.pop('PoR3D_x', 0.0)
        por_y = data.pop('PoR3D_y', 0.0)
        por_z = data.pop('PoR3D_z', 0.0)
        # Point of regard in 3D
        self.por_3D = por_x, por_y, por_z

        ec_point_x = data.pop('EyeContactPoint_x', 0.0)
        ec_point_y = data.pop('EyeContactPoint_y', 0.0)
        ec_point_z = data.pop('EyeContactPoint_z', 0.0)
        # Point of regard in 3D
        self.eye_contact_point = ec_point_x, ec_point_y, ec_point_z

        # The attention labels
        if 'GazeCoding' in data.keys():
            attention_scores = {}
            for object_name in data.keys():
                if object_name != 'GazeCoding':
                    attention_scores[object_name] = float(data[object_name])
            self.time_of_last_data = current_time
            self.attention_scores = attention_scores
            self.current_gaze = data['GazeCoding']

        self.time_of_last_message = current_time
        self.send_gaze_status()

    def events_loop(self):
        if self.verbose_:
            print('Waiting for connection...')

        while self.running_:
            novelty = False

            current_time = time.time()
            is_tracking = current_time - self.time_of_last_data < 0.5
            is_connected = current_time - self.time_of_last_message < 2.0

            novelty = novelty or (is_connected != self.connected_)
            novelty = novelty or (is_tracking != self.tracking_)

            if self.verbose_ and (self.tracking_ != is_tracking):
                print('Tracking updated: Tracking? {}'.format(is_tracking))
            if self.verbose_ and not is_connected and self.connected_:
                print('Waiting for connection...')
            if self.verbose_ and is_connected and not self.connected_:
                print('Connected...')

            self.connected_ = is_connected
            self.tracking_ = is_tracking
            if not self.tracking_:
                self.current_gaze = 'NA'
            if novelty:
                self.send_gaze_status()

            time.sleep(0.02)


def get_rotation_matrix_in_x(theta):
    R = [[1.0, 0.0, 0.0],\
         [0.0, math.cos(theta), -math.sin(theta)],\
         [0.0, math.sin(theta), math.cos(theta)]]
    return R


def get_rotation_matrix_in_y(theta):
    R = [[math.cos(theta), 0.0, math.sin(theta)],\
         [0.0, 1.0, 0.0],\
         [-math.sin(theta),0.0, math.cos(theta)]]
    return R


def get_rotation_matrix_in_z(theta):
    R = [[math.cos(theta), -math.sin(theta), 0.0],\
         [math.sin(theta),  math.cos(theta), 0.0],\
         [0.0, 0.0, 1.0]]
    return R


def multi_rot(ra, rb):
    rc = [ [1.0, 0.0, 0.0], [0.0, 1.0, 0.0], [0.0, 0.0, 1.0]]
    for i in range(3):
        for j in range(3):
            rc[i][j] = 0.0
            for k in range(3):
                rc[i][j] += ra[i][k] * rb[k][j]
    return rc


class Camera(object):
    def __init__(self, uid=0, rotation=None, translation=None):
        """
        :param uid: Unique identifier (integer)
        :param rotation:
        :param translation:
        """
        self.uid_ = uid
        self.rotation_ = rotation
        self.translation_ = translation


class PointGazeTarget(object):
    """
    A single 3D point in space
    """
    def __init__(self, label, translation=None):
        self.translation_ = translation
        self.label_ = label


class PlanarGazeTarget(object):
    def __init__(self, label, size_x=0.1, size_y=0.2, rotation=None, translation=None):
        self.size_x_ = size_x
        self.size_y_ = size_y

        self.rotation_ = rotation
        self.translation_ = translation

        self.label_ = label


class CylinderGazeTarget(object):
    def __init__(self, label, radius=0.1, height=0.2, rotation=None, translation=None):
        self.radius_ = radius
        self.height_ = height

        self.rotation_ = rotation
        self.translation_ = translation

        self.label_ = label


class Marker(object):
    def __init__(self, uid=0, p1=(100, 100), p2=(120, 120), size = (0.1, 0.1)):
        """
        :param uid: Unique identifier (integer)
        :param p1: A tuple with two numbers for the top-left point of the bounding box
        :param p1: A tuple with two numbers for the bottom-right point of the bounding box
        """
        self.uid_ = uid
        self.p1_ = p1
        self.p2_ = p2
        self.size_ = size





