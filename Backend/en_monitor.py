#!/usr/bin/env python
# encoding: utf-8
"""
en_monitor.py

Created by Jason Sundram on 2010-01-07.
Copyright (c) 2010. All rights reserved.


"""
import xmlrpclib
from time import sleep
from threading import Thread

# TODO: Thread monitor: http://docs.python.org/library/threading.html
# eventLoop = Thread(target=Eventer, name="EventLoop")
# eventLoop.start()


class SonosClient():
    def __init__(self):
        self.proxy = xmlrpclib.ServerProxy('http://localhost:8000')
        
        # interesting, but useless: print list of available methods
        # print self.proxy.system.listMethods()
        
        print "Searching for Sonos Households"
        hhids = self.proxy.SearchForHHIDs()
        if len(hhids) == 0:
            raise Exception('Unable to find any households')
        
        print "Found the following households: ", hhids
        print "Subscribing to the first one on the list . . ."
        subscribed = self.proxy.SubscribeToHH(hhids[0])
        if not subscribed:
            raise Exception('Unable to subscribe to %d' % hhids[0])
        
        print "Subscribed successfully. Getting Zone Groups . . . "
        sleep(1)
        self.zgids = self.proxy.GetAllZoneGroups()
        
        print "Got %d zone groups" % len(self.zgids)
    
    def get_all_zone_groups(self):
        self.zgids = self.proxy.GetAllZoneGroups()
        return self.zgids
    
    def get_queue(self, zgid):
        return self.proxy.GetQueue(zgid)
    
    def get_current_track_progress(self, zgid):
        return self.proxy.GetCurrentTrackTime(zgid)
    
    def get_track(self, zgid):
        return self.proxy.GetCurrentTrackMD(zgid)
    
    def enqueue(self, zgid, track):
        """ Track needs to have Uri, Artist, Album, Title, and PlayTime populated.
            Album Art currently not respected, but it will be, someday"""
        track = {
            "Uri" : "http://10.0.0.193:60210/sid/track.mp3?sid=809C275B-7738-4E76-8BED-8F01C4701721",
            "Artist" : "Pink Floyd", 
            "Album": "The Wall",
            "AlbumArtUri" : 'http://upload.wikimedia.org/wikipedia/en/1/13/PinkFloydWallCoverOriginalNoText.jpg',
            "Title": "Another Brick In The Wall (Part 2)",
            "PlayTime": 242
        }
        
        return self.proxy.EnqueueTrack(zgid, track)
    
    def next(self, zgid):
        return self.proxy.Next(zgid)
    
    def back(self, zgid):
        return self.proxy.Back(zgid)
    
    def play(self, zgid, index):
        """ returns True on success, False if index is out of range, or if zgid is unknown. 
            N.B. 
            This will return True if the currently playing track is already playing, and you pass in -1. 
            It will restart the currently playing track if you pass in its index. 
        """
        return self.proxy.Play(zgid, index)
    
    def pause(self, zgid):
        return self.proxy.Pause(zgid)
    
    def is_playing(self, zgid):
        return self.proxy.IsPlaying(zgid)
    
    def set_volume(self, zgid, level):
        """level is in the range 0-100"""
        self.proxy.SetVolume(zgid, level);  
    
    def get_volume(self, zgid):
        """returns volume level in range 0-100"""
        return self.proxy.GetVolume(zgid)
    
    def set_mute(self, zgid, mute):
        return self.proxy.SetMute(zgid, mute)
    
    def is_muted(self, zgid):
        return self.proxy.IsMuted(zgid)
    
    # event handlers
    def OnMuteChanged(self, zgid, muted):
        print "OnMuteChanged, %s, %r" % (zgid, muted)
    def OnPlayStateChanged(self, zgid, playing):
        print "OnPlayStateChanged, %s, %r" % (zgid, playing)
    def OnQueueChanged(self, zgid):
        print "OnQueueChanged, %s" % (zgid)
    def OnTick(self, zgid, progress):
        pass#print "OnTick, %s (%d)" % (zgid, progress)
    def OnTrackChanged(self, zgid):
        print "OnTrackChanged, %s" % zgid
    def OnVolumeChanged(self, zgid, level):
        print "OnVolumeChanged, %s (%d)" % (zgid, level)
    def OnZoneGroupsChanged(self):
        print "OnZoneGroupsChanged!"
    
    #event loop
    def dispatch(self, events):
        for e in events:
            name = e['Name']
            args = e['Args'] if 'Args' in e else []
            if name in SonosClient.__dict__:
                SonosClient.__dict__[name](self, *args)
    
    def __monitor(self, timeout):
        try:
            return self.proxy.PollForEvents(timeout);
        except Exception, e:
            return []
    
    def monitor(self, timeout=5):
        print "Entering infinite event-monitoring loop . . ."
        while True:
            events = self.__monitor(timeout)
            self.dispatch(events)


def main():
    s = SonosClient()
    s.monitor() # run an event loop that triggers shit that I can subscribe to.



if __name__ == '__main__':
    main()

