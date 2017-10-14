using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;




// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Robot
{
    enum MessageType { Info, Success, Error, Warning }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CoreDispatcher _main;
        private GpioController _gpio;
        private GattServiceProvider _bt;
        private Guid _btId = new Guid("{D830D022-91B1-4EE6-82D7-65DBC71BD380}");
        private Guid _btNotifyId = new Guid("{72794B21-A06E-445C-BC86-A9548BBBA1A2}");
        private GattLocalCharacteristic _btNotify;

        private void Message(string text, MessageType messageType)
        {
            Color color = Colors.WhiteSmoke;
            {
                Run inline = new Run();
                inline.Text = "\n Jero2 " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss") + " >> ";
                inline.FontFamily = new FontFamily("Console");
                inline.FontSize = 10;
                inline.Foreground = new SolidColorBrush(color);
                _console.Inlines.Add(inline);
            }
            {
                switch(messageType)
                {
                    case MessageType.Info:
                        color = Colors.Yellow;
                        break;
                    case MessageType.Success:
                        color = Colors.Lime;
                        break;
                    case MessageType.Warning:
                        color = Colors.Orange;
                        text = "warning : " + text;
                        break;
                    case MessageType.Error:
                        color = Colors.OrangeRed;
                        text = "* error * " + text;
                        break;
                }
                Run inline = new Run();
                inline.Text = text;
                inline.FontFamily = new FontFamily("Console");
                inline.FontSize = 12;
                if (messageType == MessageType.Error)
                    inline.FontWeight = FontWeights.Bold;
                inline.Foreground = new SolidColorBrush(color);
                _console.Inlines.Add(inline);
            }
        }
        public MainPage()
        {
            this.InitializeComponent();
            this._main = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
            this.InitializeGpio();
            Task.Run(() => this.InitializeBluetooth());
        }

        void InitializeGpio()
        {
            Message("Welcome to Jero2 IoT system v1.0", MessageType.Success);
            Message("initializing GPIO device controller ...", MessageType.Info);
            _gpio = GpioController.GetDefault();
            if (_gpio == null)
            {
                Message("can't find a usable IoT GPIO device controller", MessageType.Error);
                Message("without GPIO controller this system won't be able to perform any task", MessageType.Warning);
            }
        }

        async Task InitializeBluetooth()
        {
            try
            {
                await _main.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { Message("initializing Bluetooth GATT service ...", MessageType.Info); });
                GattServiceProviderResult result = await GattServiceProvider.CreateAsync(_btId);

                if (result.Error != BluetoothError.Success)
                {
                    await _main.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { Message("can't create GATT BlueTooth service with id " + _btId.ToString(), MessageType.Error); });
                    await _main.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { Message("without bluetooth service, remote controller won't work", MessageType.Warning); });
                    return;
                }
                _bt = result.ServiceProvider;

                byte[] value = new byte[] { 0x21 };
                var constantParameters = new GattLocalCharacteristicParameters
                {
                    CharacteristicProperties = (GattCharacteristicProperties.Read),
                    StaticValue = value.AsBuffer(),
                    ReadProtectionLevel = GattProtectionLevel.Plain,
                };

                GattLocalCharacteristicResult characteristicResult = await _bt.Service.CreateCharacteristicAsync(_btNotifyId, new GattLocalCharacteristicParameters {
                    CharacteristicProperties = GattCharacteristicProperties.Notify,
                    ReadProtectionLevel = GattProtectionLevel.Plain,
                    StaticValue = value.AsBuffer()
                });
                if (characteristicResult.Error != BluetoothError.Success)
                {
                    // An error occurred.
                    return;
                }
                _btNotify = characteristicResult.Characteristic;
                _btNotify.SubscribedClientsChanged += _btNotify_SubscribedClientsChanged;

                GattServiceProviderAdvertisingParameters advParameters = new GattServiceProviderAdvertisingParameters
                {
                    IsDiscoverable = true,
                    IsConnectable = true
                };
                _bt.StartAdvertising(advParameters);
                await _main.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { Message("created Bluetooth GATT service with id " + _btId.ToString(), MessageType.Success); });
            }
            catch (Exception x)
            {
                await _main.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { Message("can't create GATT BlueTooth service with id " + _btId.ToString(), MessageType.Error); });
                await _main.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { Message(x.Message, MessageType.Error); });
                await _main.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { Message("without bluetooth service, remote controller won't work", MessageType.Warning); });
            }
        }

        private async void _btNotify_SubscribedClientsChanged(GattLocalCharacteristic sender, object args)
        {
            await _main.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { Message("Bluetooth Nofity event", MessageType.Info); });
        }
    }
}
