using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Jero2
{
    class BLE
    {
        bool _init = false;
        GattServiceProvider _service;
        Guid _serviceId = new Guid("{D830D022-91B1-4EE6-82D7-65DBC71BD380}");
        Guid _notifyId = new Guid("{72794B21-A06E-445C-BC86-A9548BBBA1A2}");
        GattLocalCharacteristic _notifyCharacteristic;

        internal async Task<bool> Initialize()
        {
            try
            {
                GattServiceProviderResult result = await GattServiceProvider.CreateAsync(_serviceId);

                if (result.Error != BluetoothError.Success)
                {
                    Robot.Message("can't create GATT BlueTooth service with id " + _serviceId.ToString(), MessageType.Error);
                    Robot.Message("without bluetooth service, remote controller won't work", MessageType.Warning);
                    return(_init);
                }
                _service = result.ServiceProvider;

                byte[] value = new byte[] { 0x21 };
                var constantParameters = new GattLocalCharacteristicParameters
                {
                    CharacteristicProperties = (GattCharacteristicProperties.Read),
                    StaticValue = value.AsBuffer(),
                    ReadProtectionLevel = GattProtectionLevel.Plain,
                };

                GattLocalCharacteristicResult characteristicResult = await _service.Service.CreateCharacteristicAsync(_notifyId, new GattLocalCharacteristicParameters
                {
                    CharacteristicProperties = GattCharacteristicProperties.Notify,
                    ReadProtectionLevel = GattProtectionLevel.Plain,
                    StaticValue = value.AsBuffer()
                });
                if (characteristicResult.Error != BluetoothError.Success)
                {
                    Robot.Message("can't create GATT BlueTooth service with id " + _serviceId.ToString(), MessageType.Error);
                    Robot.Message("without bluetooth service, remote controller won't work", MessageType.Warning);
                    return (_init);
                }
                _notifyCharacteristic = characteristicResult.Characteristic;
                //_notifyCharacteristic.SubscribedClientsChanged += _btNotify_SubscribedClientsChanged;

                GattServiceProviderAdvertisingParameters advParameters = new GattServiceProviderAdvertisingParameters
                {
                    IsDiscoverable = true,
                    IsConnectable = true
                };
                _service.StartAdvertising(advParameters);
                Robot.Message("created Bluetooth GATT service with id " + _serviceId.ToString(), MessageType.Success);
                _init = true;
            }
            catch (Exception x)
            {
                while (x != null)
                {
                    Robot.Message(x.Message, MessageType.Error);
                    x = x.InnerException;
                }
                Robot.Message("without bluetooth service, remote controller won't work", MessageType.Warning);
            }
            return (_init);
        }
    }
}
