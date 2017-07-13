using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeanDevice : MonoBehaviour, IBLEdevice {

    //values specific for a Bean device
    //reference: https://punchthrough.com/bean/reference#Scratch
    private string _serviceUUID = "a495ff20-c5b1-4b44-b512-1370f02d74de"; //the Scratch characteristic
    private string _characteristicUUID = "a495ff21-c5b1-4b44-b512-1370f02d74de"; //used as write in this code
    //there are actually 4 more for use
    private string _characteristicUUID2 = "a495ff22-c5b1-4b44-b512-1370f02d74de"; //used as read in this code
    private string _characteristicUUID3 = "a495ff23-c5b1-4b44-b512-1370f02d74de";
    private string _characteristicUUID4 = "a495ff24-c5b1-4b44-b512-1370f02d74de";
    private string _characteristicUUID5 = "a495ff25-c5b1-4b44-b512-1370f02d74de";
    private string _connectedName = null; //name of the device
    private string _connectedAddress = null; //address of the device
    private bool _connecting = false;
    private bool _connected = false;
    private bool _readFound = false;
    private bool _writeFound = false;

    private BLEmanager _bleManager; //reference to the BLE manager object for calls

    //constructor
    public BeanDevice(BLEmanager bm)
    {
        _bleManager = bm;
    }

    //handles Disconnect
    //disconnect the device as a peripheral
    public void OnDisconnect()
    {
        if (_connectedAddress != null)
        {//disconnect from a connected device if there is any
            BluetoothLEHardwareInterface.DisconnectPeripheral(_connectedAddress, null);
            _connectedAddress = null;
        }
        _connected = false;
    }

    //handles Connect
    //connect the device as a peripheral, and set up the characteristics for I/O properly
    public void OnConnect(string dName, string dAddress)
    {
        _connectedName = (string)dName.Clone();
        _connectedAddress = (string)dAddress.Clone();

        if (!_connecting)
        {//perform a connection only if it is not doing it
            if (_connected)
            {//if already connected, disconnect it
                if (_connectedAddress != null)
                {
                    BluetoothLEHardwareInterface.DisconnectPeripheral(_connectedAddress, null);
                }
                _connected = false;
            }
            else
            {
                _readFound = false;
                _writeFound = false;

                //connect to the device with the address provided
                BluetoothLEHardwareInterface.ConnectToPeripheral(_connectedAddress,
                    (address) => {//gets called when the connection is successful
                        _connectedAddress = address;
                        _connected = true;
                        _connecting = false;
                        _bleManager.IndicateConnected(dName, dAddress); //let the manager know it is connected

                        //stop scanning if a connection is established
                        BluetoothLEHardwareInterface.StopScan();
                    },
                    (address, serviceUUID) => {//gets called for each service the device supports
                        BluetoothLEHardwareInterface.Log(_connectedName + " supports service: " + serviceUUID);
                    },
                    (address, serviceUUID, characteristicUUID) => {//gets called for each characteristic the device service supports

                        BluetoothLEHardwareInterface.Log(serviceUUID + " supports characteristic: " + characteristicUUID);

                        if (serviceUUID.ToUpper().CompareTo(_serviceUUID.ToUpper()) == 0)
                        {//the _serviceUUID is a hardware-specific value indicating a certain collection of characteristics, 
                         // i.e., different from hardware to hardware
                            if (characteristicUUID.ToUpper().CompareTo(_characteristicUUID.ToUpper()) == 0)
                            {//Bean has 5 characteristics for 2-way communication, this code uses the first one as write
                                _writeFound = true;

                            }
                            else if (characteristicUUID.ToUpper().CompareTo(_characteristicUUID2.ToUpper()) == 0)
                            {//Bean has 5 charateristics for 2-way communication, this code uses the second one as read
                                _readFound = true;

                                //subscribe to the charateristic so the code will be notified if a change happens
                                BluetoothLEHardwareInterface.SubscribeCharacteristic(_connectedAddress, _serviceUUID, _characteristicUUID2,
                                    (theOtherCharacteristicUUID) =>
                                    { //gets called when a notification occurs
                                    },
                                    (theOtherCharacteristicUUID, values) =>
                                    { //gets called when the charateristic value is updated by the peripheral, provides access to the data (values)
                                        _bleManager.IndicateDataReceived(theOtherCharacteristicUUID, values);
                                    }
                                    );
                            }
                        }
                    }, (address) => {
                        // this will get called when the device disconnects
                        // be aware that this will also get called when the disconnect
                        // is called above. both methods get call for the same action
                        // this is for backwards compatibility
                        _connected = false;
                    }
                );

                _connecting = true;
            }
        }
    }

    //handles Send
    //send a byte array to the device assuming that there is a write characteristic under the service
    public void OnSend(byte[] data)
    {
        //generate 20 bytes of data to send, which is the maximum number of bytes in a characteristic in BLE standard
        if (data == null)
        {
            data = new byte[20];
            for (int i = 0; i < data.Length; ++i)
                data[i] = (byte)i;
        }

        BluetoothLEHardwareInterface.WriteCharacteristic(_connectedAddress, _serviceUUID, _characteristicUUID, data, data.Length, true,
            (characteristicUUID) => {//gets called after the write is completed, and if the previous flag is set to true
                _bleManager.IndicateDataSent(characteristicUUID);
            });
    }


}
