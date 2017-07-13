using System;

public interface IBLEdevice
{
    //handles Disconnect
    //disconnect the device as a peripheral
    void OnDisconnect();

    //handles Connect
    //connect the device as a peripheral, and set up the characteristics for I/O properly
    void OnConnect(string dName, string dAddress);

    //handles Send
    //send a byte array to the device assuming that there is a write characteristic under the service
    void OnSend(byte[] data);

}
