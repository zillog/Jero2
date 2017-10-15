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

namespace Jero2
{
    enum MessageType { Info, Success, Error, Warning }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        internal void ConsoleWrite(string text, MessageType messageType)
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
            MainPage main = this;
            CoreDispatcher core = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
            Task.Run(() => Robot.Initialize(main, core));
        }
    }
}
