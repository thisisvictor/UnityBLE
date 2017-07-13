using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * Created by Victor Cheung 
 * Last modified: Jan 2017
 * This script will analyze the name of the device (through the OnSelected method) and attach the corresponding BLE device script so the right UUIDs are used.
 * Can also add in some actions so the UI reflects what kind of BLE device is being detected (all the other connection part is done by the IBLEdevice interface).
 * Populate this script when more BLE device types are available.
 */
public class PeripheralButtonScript : MonoBehaviour {

    //a reference to an interface script to a BLE device
    private IBLEdevice _bleDevice;

    public Text TextName = null;
    public Text TextAddress = null;

    //handles the Selected event
    public void OnSelected(BLEmanager bm)
    {//populate the list

        if (_bleDevice == null)
        {//first time the button is clicked
            if (TextName.text.ToUpper().Contains("BLUNO"))
            {
                _bleDevice = new BlunoDevice(bm);
            }
            else if(TextName.text.ToUpper().Contains("BEAN"))
            {
                _bleDevice = new BeanDevice(bm);
            }
            else
            {
                BluetoothLEHardwareInterface.Log("Selected an unrecognized BLE device");
            }
        }

        if(_bleDevice != null)
        {//connect if the BLE device is recognized
            _bleDevice.OnConnect(TextName.text, TextAddress.text);
        }
    }

    //handles the Send
    public void OnSend(byte[] data)
    {
        _bleDevice.OnSend(data);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
