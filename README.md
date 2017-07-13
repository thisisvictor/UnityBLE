# UnityBLE
Resources for creating apps in Unity to work with BLE devices

Last edit: 13 July 2017
Created by: Victor Cheung

This repository contains all the scripts and objects necessary for creating a mobile app in Unity to talk to BLE devices, except the "Bluetooth LE for iOS, tvOS and Android unity package" (https://www.assetstore.unity3d.com/en/#!/content/26661). Our lab has bought the licence so get it using the lab's Unity account.

Currently the the resources work with Bluno and Blue Bean. If you want them to work with other BLE devices, you'll need to figure out the Service UUID and Characteristics UUID that handle the I/O on the device, then create a new class implementing the "IBLEdevice" interface.

To get started, read the "AndroidAppThatTalksToaBLEdevice" document. You'll need to know a bit about Unity and C# to get it working... I'll try to create a walkthrough video when I have time.
