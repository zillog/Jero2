using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Jero2
{
    public static class Robot
    {
        static bool _init = false;
        internal static Queue<Tuple<string, MessageType>> MessageQueue = new Queue<Tuple<string, MessageType>>();

        static MainPage _main;
        static CoreDispatcher _thread;

        static GPIO _gpio = new GPIO();
        static BLE _ble = new BLE();


        static internal void Message(string text, MessageType messageType)
        {
            MessageQueue.Enqueue(new Tuple<string, MessageType>(text, messageType));
        }

        static internal async Task ProcessMessageQueue()
        {
            while (MessageQueue.Count() > 0)
            {
                Tuple<string, MessageType> msg = MessageQueue.Dequeue();
                await _thread.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { _main.ConsoleWrite(msg.Item1, msg.Item2); });
            }
        }

        internal static async Task<bool> Initialize(MainPage main, CoreDispatcher dispatcher)
        {
            try
            {
                _main = main;
                _thread = dispatcher;

                Message("Welcome to Jero2 IoT system v1.0", MessageType.Success);
                Message("initializing GPIO device controller ...", MessageType.Info);
                await ProcessMessageQueue();

                _init = _gpio.Initialize();

                Robot.Message("initializing Bluetooth GATT service ...", MessageType.Info);
                await ProcessMessageQueue();

                if (!(await _ble.Initialize()) && _init)
                    _init = false;
                await ProcessMessageQueue();
            }
            catch (Exception x)
            {
                throw (x);
            }
            return(_init);
        }

    }
}
