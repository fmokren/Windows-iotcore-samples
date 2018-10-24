using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GrovePi;
using GrovePi.Sensors;
using GrovePi.I2CDevices;
using System.Threading;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MyHelloWorld
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Timer periodicTimer;

        IRgbLcdDisplay GroveLCD;
        IDHTTemperatureAndHumiditySensor GroveTempHumi;

        IButtonSensor GroveButton;
        Buzzer buzzer;
        IRotaryAngleSensor GroveRotary;

        Random random;
        bool showTemp = true;

        public MainPage()
        {
            this.InitializeComponent();

            random = new Random();

            GroveTempHumi = DeviceFactory.Build.DHTTemperatureAndHumiditySensor(Pin.DigitalPin3, DHTModel.Dht11);
            GroveLCD = DeviceFactory.Build.RgbLcdDisplay();
            
            GroveButton = DeviceFactory.Build.ButtonSensor(Pin.DigitalPin7);
            buzzer = new Buzzer();

            GroveRotary = DeviceFactory.Build.RotaryAngleSensor(Pin.AnalogPin0);

            periodicTimer = new Timer(this.TimerCallBack, null, 0, 100);
        }

        private byte RandomColor
        {
            get
            {
                return (byte)random.Next(256);
            }
        }

        private int timerCount;
        private void TimerCallBack(object state)
        {
            lock (this)
            {
                timerCount++;

                ButtonCallBack(state);

                if (timerCount % 20 == 1)
                {
                    if (showTemp)
                        TempCallBack(state);
                    else
                        AngleCallBack(state);
                }
            }
        }

        private void ButtonCallBack(object state)
        {
            var buttonState = GroveButton.CurrentState;

            if (buttonState == SensorStatus.On && !buzzer.BuzzerOn)
            {
                showTemp = !showTemp;
                buzzer.Toggle();
            }
            else if (buttonState == SensorStatus.Off && buzzer.BuzzerOn)
            {
                buzzer.Toggle();
            }
        }

        private void TempCallBack(object state)
        {
            var red = RandomColor;
            var green = RandomColor;
            var blue = RandomColor;
            GroveTempHumi.Measure();
            var temp = GroveTempHumi.TemperatureInFahrenheit.ToString();
            var humi = GroveTempHumi.Humidity.ToString();
            var angle = GroveRotary.SensorValue();
            var message = string.Format("{0}F  {1}%  \n  {2}  {3}  {4}", 
                temp,
                humi,
                red,
                green,
                blue);

            GroveLCD.SetText(message).SetBacklightRgb(red, green, blue);
        }

        private void AngleCallBack(object state)
        {
            var red = RandomColor;
            var green = RandomColor;
            var blue = RandomColor;
            var angle = GroveRotary.Degrees();
            var message = string.Format("Angle - {0}  \n  {1}  {2}  {3}",
                angle,
                red,
                green,
                blue);

            GroveLCD.SetText(message).SetBacklightRgb(red, green, blue);
        }
    }

    class Buzzer
    {
        IBuzzer GroveBuzzer;

        public bool BuzzerOn { get; private set; }
        public Buzzer()
        {
            GroveBuzzer = DeviceFactory.Build.Buzzer(Pin.DigitalPin6);
            GroveBuzzer.ChangeState(SensorStatus.Off);
            BuzzerOn = false;
        }

        public void Toggle()
        {
            GroveBuzzer.ChangeState(BuzzerOn ? SensorStatus.Off : SensorStatus.On);
            BuzzerOn = !BuzzerOn;
        }
    }
}
