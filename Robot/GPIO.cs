using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace Jero2
{
    class GPIO
    {
        bool _init = false;
        GpioController _gpio;

        internal bool Initialize()
        {
            try
            {
                _gpio = GpioController.GetDefault();
                if (_gpio != null)
                {
                    Robot.Message("IoT GPIO device controller found and initialized", MessageType.Success);
                    _init = true;
                }
                else
                {
                    Robot.Message("can't find a usable IoT GPIO device controller", MessageType.Error);
                    Robot.Message("without GPIO controller this system won't be able to perform any task", MessageType.Warning);
                }
            }
            catch (Exception x)
            {
                while(x != null)
                {
                    Robot.Message(x.Message, MessageType.Error);
                    x = x.InnerException;
                }
                Robot.Message("without GPIO controller this system won't be able to perform any task", MessageType.Warning);
            }
            return (_init);
        }
    }
}
