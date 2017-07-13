using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * Handles all the connection stuff to the BLE device.
 * Bsed on the TestScript.cs in the Library Example (Test), and also CentralScript.cs
 */
public class BLEmanager : MonoBehaviour
{
    public Button ScanButton; //reference to the ScanBtn label
    public Button SendButton; //reference to the SendBtn
    public Transform PanelScrollContents; //reference to the scroll contents showing peripheral devices
    public GameObject PeripheralButtonPrefab; //reference to the button prefab to click on for establishing connection
    public Text DeviceServices; //reference to the Services Text to show what the connected device can do

	private float _screenWidth, _screenHeight;
    public GameObject theCircle;

    private byte[] data;
    private BluetoothDeviceScript bluetoothDeviceScript;

    //values specific for the connected device
    private string _serviceUUID = "0000dfb0-0000-1000-8000-00805f9b34fb";
    private string _characteristicUUID = "0000dfb1-0000-1000-8000-00805f9b34fb";
    private string _connectedName = null;
    private string _connectedAddress = null;
    private bool _connecting = false;
    private bool _connected = false;
    private bool _readFound = false;
    private bool _writeFound = false;

    //private Dictionary<string, CentralPeripheralButtonScript> _peripheralList;  //stores all the scanned devices
    private Dictionary<string, PeripheralButtonScript> _peripheralList;  //stores all the scanned devices
    private bool _scanning = false;

    // Use this before Start
    private void Awake()
    {
        //set the UI appearance before everything
        theCircle.GetComponent<Renderer>().enabled = false;
        PanelScrollContents.gameObject.SetActive(true);// GetComponent<Renderer>().enabled = true;

    }

    // Use this for initialization
    void Start()
    {
		_screenHeight = Camera.main.orthographicSize * 2f;
		_screenWidth = _screenHeight * Camera.main.aspect;

        //initialize itself as a central device
        bluetoothDeviceScript = BluetoothLEHardwareInterface.Initialize(true, false, null, null);

        //request the screen to not sleep
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //deinitialize the bluetooth system when the script is destroyed
    private void OnDestroy()
    {
        if (_connectedAddress != null)
        {//disconnect from a connected device if there is any
            BluetoothLEHardwareInterface.DisconnectPeripheral(_connectedAddress, null);
            _connectedAddress = null;
            _connected = false;
            _connecting = false;
        }
        BluetoothLEHardwareInterface.DeInitialize(null);
    }

    //function to add devices listed in the scrollview
    void AddPeripheral(string name, string address)
    {
        DeviceServices.text = "Adding peripheral " + name;

        if (_peripheralList == null)
            _peripheralList = new Dictionary<string, PeripheralButtonScript>();

        if (!_peripheralList.ContainsKey(address))
        {
            GameObject peripheralObject = (GameObject)Instantiate(PeripheralButtonPrefab);
            //this script is like a struct to trigger the correct panel for the device type and initialize the connection with the correct protocol
            PeripheralButtonScript script = peripheralObject.GetComponent<PeripheralButtonScript>();
            script.TextName.text = name;
            script.TextAddress.text = address;
            peripheralObject.transform.SetParent(PanelScrollContents);
            peripheralObject.transform.localScale = new Vector3(1f, 1f, 1f);
            peripheralObject.GetComponent<Button>().onClick.AddListener(() => script.OnSelected(this));
            //peripheralObject.GetComponent<Button>().onClick.AddListener(() => OnConnect(peripheralObject.GetComponent<Button>()));

            _peripheralList[address] = script;
        }
    }

    //function to remove devices listed in the scrollview
    void RemovePeripherals()
    {
        for (int i = 0; i < PanelScrollContents.childCount; ++i)
        {
            GameObject gameObject = PanelScrollContents.GetChild(i).gameObject;
            Destroy(gameObject);
        }

        if (_peripheralList != null)
            _peripheralList.Clear();
    }

    //helper function to convert an array of bytes into a string
    protected string BytesToString(byte[] bytes)
    {
        string result = "";

        foreach (var b in bytes)
            result += b.ToString("X2"); //convert the byte into hex with a precision of 2

        return result;
    }

    //handles the scan button click
    public void OnScan()
    {
        if (_scanning)
        {//only scan once
            BluetoothLEHardwareInterface.StopScan();
            ScanButton.GetComponentInChildren<Text>().text = "Scan";
            _scanning = false;
        }
        else
        {
            if (_connectedAddress != null)
            {//disconnect from a connected device if there is any (only meant to connect to one device?)
                BluetoothLEHardwareInterface.DisconnectPeripheral(_connectedAddress, null);
                _connectedName = null;
                _connectedAddress = null;
            }

            theCircle.GetComponent<Renderer>().enabled = false;
            PanelScrollContents.gameObject.SetActive(true);// GetComponent<Renderer>().enabled = true;

            SendButton.interactable = false;

            RemovePeripherals();

            DeviceServices.text = "scanning...";
            // the first callback will only get called the first time this device is seen
            // this is because it gets added to a list in the BluetoothDeviceScript
            // after that only the second callback will get called and only if there is
            // advertising data available
            BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, name) => {

                AddPeripheral(name, address);

            }, (address, name, rssi, advertisingInfo) => {

                if (advertisingInfo != null)
                    BluetoothLEHardwareInterface.Log(string.Format("Device: {0} RSSI: {1} Data Length: {2} Bytes: {3}", name, rssi, advertisingInfo.Length, BytesToString(advertisingInfo)));
            });

            ScanButton.GetComponentInChildren<Text>().text = "Stop";
            _scanning = true;
        }
    }

    //Public method called by others to indicate that the connection is established
    public void IndicateConnected(string name, string address)
    {
        _connected = true;
        _connectedName = name;
        _connectedAddress = address;
        _connecting = false;

        DeviceServices.text = "Connected to " + _connectedName + " " + _connectedAddress;
        SendButton.interactable = true;
        theCircle.GetComponent<Renderer>().enabled = true;
        PanelScrollContents.gameObject.SetActive(false);

        //the device code that calls this method will likely to call BluetoothLEHardwareInterface.StopScan()
        // once a connection is establised, so reflect this here as well
        ScanButton.GetComponentInChildren<Text>().text = "Scan";
        _scanning = false;
    }
	
	//Public method called by others to indicate that the connection is terminated
	public void IndicateDisconnected() {
		_connected = false;
		_connectedName = null;
		_connectedAddress = null;
		_connecting = false;

		DeviceServices.text = "Disconnected";
	}

    //Public method called by others to indicate that data is sent
    public void IndicateDataSent(string characteristic)
    {
        string s = "Sent";
        for (int i = 0; i < data.Length; i++)
            s += " " + data[i];
        DeviceServices.text = s + " to "+ characteristic;
    }

    //Public method called by others to indicate that data is received
    public void IndicateDataReceived(string characteristic, byte[] receivedData)
    {
        string s = "Received";
        for (int i = 0; i < receivedData.Length; i++)
            s += " " + receivedData[i];
        DeviceServices.text = s + " from " + characteristic;

        if (receivedData[0] == (int)0)
        {
            Camera.main.backgroundColor = new Color(1f, 200 / 255f, 0);

            int bendVal = BitConverter.ToInt16(receivedData, 1); //convert the 2nd and 3rd byte into an int
            theCircle.transform.localScale = Vector3.one * bendVal / 1024f * 4f;
            DeviceServices.text += " " + bendVal;
        }
        else if (receivedData[0] == (int)1)
        {
            Camera.main.backgroundColor = Color.white;
        }
        else
        {//in case some other values are sent in the first byte
            Camera.main.backgroundColor = Color.red;
        }
    }

    //handles the sending when the SendBtn is clicked
    public void OnSend()
    {
        //generate 20 bytes of data to send, which is the maximum number of bytes in a characteristic in BLE standard
        if (data == null)
        {
            data = new byte[20];
            for (int i = 0; i < data.Length; ++i)
                data[i] = (byte)i;
        }

        _peripheralList[_connectedAddress].OnSend(data);

    }

}
