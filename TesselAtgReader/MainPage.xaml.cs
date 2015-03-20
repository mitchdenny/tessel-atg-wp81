using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace TesselAtgReader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private async void ReadButton_Click(object sender, RoutedEventArgs e)
        {
            var transceiverServiceId = Guid.Parse("d752c5fb-1380-4cd5-b0ef-cac7d72cff20");
            var selector = GattDeviceService.GetDeviceSelectorFromUuid(transceiverServiceId);
            var devices = await DeviceInformation.FindAllAsync(selector);
            if (devices.Count != 0)
            {
                var deviceId = devices[0].Id;

                var service = await GattDeviceService.FromIdAsync(deviceId);

                var distanceCharacteristicId = Guid.Parse("883f1e6b-76f6-4da1-87eb-6bdbdb617888");
                var distanceCharacteristic = service.GetCharacteristics(distanceCharacteristicId)[0];

                var distanceResult = await distanceCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                var distanceResultBytes = distanceResult.Value.ToArray();
                var distanceAsString = Encoding.UTF8.GetString(distanceResultBytes, 0, distanceResultBytes.Length);

                this.ReadButton.Content = distanceAsString;

                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var unixTimestamp = (DateTime.UtcNow - epoch).TotalSeconds;
                var distanceEvent = string.Format("{{device: 'tessel-atg-tenant1-device1', distance: {0}, timestamp: {1}}}", distanceAsString, unixTimestamp);
                var content = new StringContent(distanceEvent);
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", "sr=https%3A%2F%2Ftessel-atg.servicebus.windows.net%2Ftessel-atg-tenant1%2Fpublishers%2Ftessel-atg-tenant1-device1%2Fmessages&sig=EPKqXdzcCzzgCJRAome1kdbGxbqAtcZyaT2oKP3f7UY%3D&se=1454612852&skn=tessel-atg-tenant1-policy1");
                var response = await client.PostAsync("https://tessel-atg.servicebus.windows.net/tessel-atg-tenant1/publishers/tessel-atg-tenant1-device1/messages", content);
            }
        }
    }
}
