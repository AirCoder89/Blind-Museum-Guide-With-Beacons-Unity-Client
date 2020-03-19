﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NonBlindAutoMode : MonoBehaviour
{
    private List<BeaconData> _ourBeacons;
    private List<Beacon> _beaconsInRange;

    private BeaconData _targetBeacon;
    private BeaconData _lastTargetBeacon;
    [SerializeField] private BeaconsManager beaconsManager;
    [SerializeField] private NonBlindPopUp popup;

    public void Initialize(List<BeaconData> list)
    {
        _targetBeacon = null;
        _lastTargetBeacon = null;
        _ourBeacons = list;
    }

    private void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
      return;
#endif
       if (Input.GetKeyUp(KeyCode.A))
       {
          var result = new Beacon[] {new Beacon("e7cd46b3-b65f-414d-814b-fcd2195fa930", 1, 1)};
          OnBeaconRangeChanged(result);
       }
       else  if (Input.GetKeyUp(KeyCode.Z))
       {
          var result = new Beacon[] {new Beacon("972ea272-1e53-4652-9895-2c8783f76f16", 1, 1)};
          OnBeaconRangeChanged(result);
       }
       else  if (Input.GetKeyUp(KeyCode.E))
       {
          var result = new Beacon[] {new Beacon("89082614-1b20-4794-96df-c8a23842685b", 1, 1)};
          OnBeaconRangeChanged(result);
       }
       else  if (Input.GetKeyUp(KeyCode.R))
       {
          var result = new Beacon[] {new Beacon("2d32eefa-10a4-4b69-af15-b92947d84c8d", 1, 1),new Beacon("2d32eefa-10a4-4b69-af15-b92947d84c8d", 1, 1)};
          OnBeaconRangeChanged(result);
       }
       else  if (Input.GetKeyUp(KeyCode.T))
       {
          var result = new Beacon[] {new Beacon("89082614-1b20-4794-96df-c8a23842685b", 1, 1),new Beacon("2d32eefa-10a4-4b69-af15-b92947d84c8d", 1, 1)};
          OnBeaconRangeChanged(result);
       }
       else  if (Input.GetKeyUp(KeyCode.Y))
       {
          var result = new Beacon[] {};
          OnBeaconRangeChanged(result);
       }
    }
    public void OnOpen()
    {
        print("OnOpen");
        iBeaconReceiver.BeaconRangeChangedEvent += OnBeaconRangeChanged;
        _targetBeacon = null;
        _lastTargetBeacon = null;
         #if UNITY_EDITOR
                 print("start scan !");
         #elif UNITY_ANDROID
                  DefineBeaconRegion();
                  iBeaconReceiver.Scan();
         #endif
    }

    private void DefineBeaconRegion()
    {
       iBeaconReceiver.regions = new iBeaconRegion[]{new iBeaconRegion("Any", new Beacon())};
       return;
       var region = new iBeaconRegion[_ourBeacons.Count];
       Log("______________");
       for (var i = 0; i < _ourBeacons.Count; i++)
       {
          region[i] = new iBeaconRegion("museum",
             new Beacon(_ourBeacons[i].uuid, Convert.ToInt32(0), Convert.ToInt32(0)));
          Log("uuid range: " + _ourBeacons[i].uuid);
       }
      // iBeaconReceiver.regions = region;
       Log("______________");
    }
    
    private void OnBeaconRangeChanged(Beacon[] beacons)
    {
       Log("________________");
       Log("OnBeaconRangeChanged > " + beacons.Length);
       var detectedBeacons = new List<Beacon>(beacons);
       
       try
       {
          _beaconsInRange = detectedBeacons.Where(b => b.accuracy <= 1 && UUIDExists(b.UUID)).ToList();
          Log("_beaconsInRange > " + _beaconsInRange.Count);
          
          if (_beaconsInRange.Count == 1)
          {
             _targetBeacon = GetBeaconByUUID(_beaconsInRange[0].UUID);
             ExecuteBeacon(_targetBeacon);
          }
          else
          {
             //get min range beacon
             var min = _beaconsInRange.Min(b => b.accuracy);
             _targetBeacon = GetBeaconByUUID(_beaconsInRange.First(b => b.accuracy == min).UUID);
             ExecuteBeacon(_targetBeacon);
          }
       }
       catch
       {
       }
    }
    
     private void OnBeaconRangeChangedOld(Beacon[] beacons)
      {
      Log("OnBeaconRangeChanged > " + beacons.Length);
      var detectedBeacons = new List<Beacon>(beacons);
      try
      {
        // _beaconsInRange = detectedBeacons.Where(b => UUIDExists(b.UUID)).ToList(); // && b.accuracy <= 1
        // _beaconsInRange = detectedBeacons.Where(b => b.accuracy <= 1).ToList();
         _beaconsInRange = detectedBeacons.Where(b => b.accuracy <= 1 && UUIDExists(b.UUID)).ToList();
         Log("_beaconsInRange > " + _beaconsInRange.Count);
         
         if (_beaconsInRange.Count == 1)
         {
            _targetBeacon = GetBeaconByUUID(_beaconsInRange[0].UUID);
            if (_targetBeacon == null)
            {
               Log($"beacon {_beaconsInRange[0].UUID} not Found");
               _lastTargetBeacon = null;
               AudioManager.Instance.Stop();
               popup.Close();
               return;
            }
            if(_lastTargetBeacon != _targetBeacon) _targetBeacon.onPause = false;
            ExecuteBeacon();
         }
         else
         {
            //get min range beacon
            var min = _beaconsInRange.Min(b => b.accuracy);
            _targetBeacon = GetBeaconByUUID(_beaconsInRange.First(b => b.accuracy == min).UUID);
            if (_targetBeacon == null)
            {
               Log($"beacon not Found");
               _lastTargetBeacon = null;
               AudioManager.Instance.Stop();
               popup.Close();
               return;
            }
            ExecuteBeacon();
         }
      }
      catch
      {
         _targetBeacon = null;
         popup.Close();
         AudioManager.Instance.Stop();
         print("no beacon in range or matching uuid");
         Log("no beacon in range or matching uuid");
      }
   }

     private void ExecuteBeacon(BeaconData beacon)
     {
        beaconsManager.SelectBeaconByUUID(beacon.uuid);
        popup.Open(beacon,OnClosePopUp);
     }
     
   private void ExecuteBeacon()
   {
      if (_targetBeacon == null)
      {
         popup.Close();
         AudioManager.Instance.Stop();
         return;
      }
      if (_lastTargetBeacon == _targetBeacon)
      {
         return;
      }
      if (_targetBeacon.onPause)
      {
         return;
      }
      _lastTargetBeacon = _targetBeacon;
      Log("ExecuteBeacon > " + _targetBeacon.uuid);
      beaconsManager.SelectBeaconByUUID(_targetBeacon.uuid);
      popup.Open(_targetBeacon,OnClosePopUp);
   }

   private void OnClosePopUp()
   {
      AudioManager.Instance.Stop();
   }
   
   private BeaconData GetBeaconByUUID(string uuid)
   {
      try
      {
         return this._ourBeacons.First(b => b.uuid == uuid);
      }
      catch
      {
         return null;
      }
   }
   private bool UUIDExists(string uuid)
   {
      try
      {
         return _ourBeacons.Exists(b => string.Equals(b.uuid, uuid, StringComparison.CurrentCultureIgnoreCase));
      }
      catch
      {
         return false;
      }
   }
   
   public void OnClose()
   {
      print("OnClose");
      iBeaconReceiver.BeaconRangeChangedEvent -= OnBeaconRangeChanged;
      _targetBeacon = null;
      popup.Close();
      AudioManager.Instance.Stop();
      #if UNITY_EDITOR
            print("stop scan !");
      #elif UNITY_ANDROID
            iBeaconReceiver.Stop();
      #endif
   }
   
   private void Log(string str)
   {
      print(str);
      //if(logTxt ==null) return;
     // logTxt.text = str + "\n" + logTxt.text;
   }
}
