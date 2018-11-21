using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace HelloWorld_I2C
{
    class LcdDisplay
    {
        private const string I2CName = "I2C1"; /* For Raspberry Pi 2, use I2C1 */
        private const byte DisplayRgbI2CAddress = 0xC4;
        private const byte DisplayTextI2CAddress = 0x7C;

        private const byte RedCommandAddress = 4;
        private const byte GreenCommandAddress = 3;
        private const byte BlueCommandAddress = 2;
        private const byte TextCommandAddress = 0x80;
        private const byte ClearDisplayCommandAddress = 0x01;
        private const byte DisplayOnCommandAddress = 0x08;
        private const byte NoCursorCommandAddress = 0x04;
        private const byte TwoLinesCommandAddress = 0x28;
        private const byte SetCharacterCommandAddress = 0x40;

        public const int GroveRgpLcdMaxLength = 16;
        public const int GroveRgpLcdRows = 2;

        private I2cDevice rgbDevice;
        private I2cDevice txtDevice;
        private GpioPin pin;

        private static async Task<DeviceInformationCollection> GetDeviceInfo()
        {
            //Find the selector string for the I2C bus controller
            var aqs = I2cDevice.GetDeviceSelector(I2CName);
            //Find the I2C bus controller device with our selector string
            var dis = await DeviceInformation.FindAllAsync(aqs);
            return dis;
        }
        public LcdDisplay()
        {
            /* Initialize the I2C bus */
            var rgbConnectionSettings = new I2cConnectionSettings(DisplayRgbI2CAddress >> 1)
            {
                BusSpeed = I2cBusSpeed.StandardMode
            };

            var textConnectionSettings = new I2cConnectionSettings(DisplayTextI2CAddress >> 1)
            {
                BusSpeed = I2cBusSpeed.StandardMode
            };

            Task.Run(async () =>
            {
                var dis = await GetDeviceInfo();

                // Create an I2cDevice with our selected bus controller and I2C settings
                rgbDevice = await I2cDevice.FromIdAsync(dis[0].Id, rgbConnectionSettings);
                txtDevice = await I2cDevice.FromIdAsync(dis[0].Id, textConnectionSettings);
            }).Wait();

            rgbDevice.Write(new byte[] { 0, 0, 1, 0 });
            rgbDevice.Write(new byte[] { DisplayOnCommandAddress, 0xaa });

            txtDevice.Write(new[] { TextCommandAddress, (byte)(DisplayOnCommandAddress | NoCursorCommandAddress) });
            txtDevice.Write(new[] { TextCommandAddress, TwoLinesCommandAddress });
            ClearText();

            GpioController gpioController = Task.Run(async () =>
            {
                return await GpioController.GetDefaultAsync();
            }).Result;

            pin = gpioController.OpenPin(7);
            pin.SetDriveMode(GpioPinDriveMode.Output);
            pin.Write(GpioPinValue.High);
        }

        public void SetBacklightRgb(byte red, byte green, byte blue)
        {
            rgbDevice.Write(new[] { RedCommandAddress, red });
            rgbDevice.Write(new[] { GreenCommandAddress, green });
            rgbDevice.Write(new[] { BlueCommandAddress, blue });
        }

        public void SetText(string message)
        {
            var count = 0;
            List<byte[]> commands = new List<byte[]>();
            List<byte> buffer = new List<byte>();
            buffer.Add(SetCharacterCommandAddress);

            foreach (var c in message)
            {
                if (c.Equals('\n') || count == GroveRgpLcdMaxLength)
                {
                    count = 0;
                    commands.Add(buffer.ToArray());
                    buffer = new List<byte>();
                    if (commands.Count == 3)
                    {
                        break;
                    }
                    commands.Add(new byte[]{ TextCommandAddress, 0xc0 });
                    
                    buffer.Add(SetCharacterCommandAddress);
                    continue;
                }
                
                buffer.Add((byte)c);
                count++;
            }

            if(buffer.Count > 1)
            {
                commands.Add(buffer.ToArray());
            }

            pin.Write(GpioPinValue.Low);

            foreach (byte[] command in commands)
            {
                txtDevice.Write(command);
            }

            pin.Write(GpioPinValue.High);
        }

        public void ClearText()
        {
            txtDevice.Write(new[] { TextCommandAddress, ClearDisplayCommandAddress });
            Task.Delay(1).Wait();
        }
    }
}
