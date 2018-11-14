using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HelloWorld_I2C
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private LcdDisplay display = new LcdDisplay();
        public MainPage()
        {
            this.InitializeComponent();
            Red.Value = 30;
            Green.Value = 156;
            Blue.Value = 142;
            Slider_ValueChanged();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Send button clicked");
            display.ClearText();
            display.SetText(Text.Text);
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Reset button clicked");
            display.ClearText();
        }

        private void Slider_ValueChanged()
        {
            byte red = (byte)this.Red.Value;
            byte green = (byte)this.Green.Value;
            byte blue = (byte)this.Blue.Value;

            Debug.WriteLine(string.Format("(R, G, B) = ({0}, {1}, {2})", red, green, blue));
            display.SetBacklightRgb(red, green, blue);
        }

        private void Red_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider_ValueChanged();
        }

        private void Green_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider_ValueChanged();
        }

        private void Blue_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider_ValueChanged();
        }

        private void Text_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
