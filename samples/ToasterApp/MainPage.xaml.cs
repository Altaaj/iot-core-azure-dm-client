﻿using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Devices.Management;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Toaster
{
    public sealed partial class MainPage : Page
    {
        DeviceManagementClient DMClient;

        private const string DeviceConnectionString = "HostName=iot-open-house-demo.azure-devices.net;DeviceId=dog-feeder;SharedAccessKey=88JacFKEPdfsouEwcYdF9VRuz0RsyTqkH6iRlufvRkE=";

        public MainPage()
        {
            this.InitializeComponent();
            this.buttonStart.IsEnabled = true;
            this.buttonStop.IsEnabled = false;
            this.imageHot.Visibility = Visibility.Collapsed;
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString, TransportType.Mqtt);

            DMClient = DeviceManagementClient.Create(
                new AzureIoTHubDeviceTwinProxy(deviceClient), 
                new ToasterDeviceManagementRequestHandler(this));

            deviceClient.SetDesiredPropertyUpdateCallback(OnDesiredPropertyUpdate, this);
        }

        public Task OnDesiredPropertyUpdate(TwinCollection desiredProperties, object userContext)
        {
#if false // TODO
            TwinCollection nonDMProperties = DMClient.HandleDesiredPropertiesChanged(desiredProperties);

            // Application developer can process all the top-level nodes (in nonDMProperties)
            // that did not get filtered out by DM.
#endif
            return null;
        }

        // This method may get called on the DM callback thread - not on the UI thread.
        public async Task<bool> YesNo(string question)
        {
            // ToDo: This needs a clean solution (ideally before Artur sees it :)).
            bool answered = false;
            bool yesAnswer = false;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                UserDialog dlg = new UserDialog(question);
                ContentDialogResult dialogResult = await dlg.ShowAsync();
                yesAnswer = dlg.Result;
                answered = true;
            });

            while (!answered)
            {
                Debug.WriteLine("sleeping for 1 second...");
                System.Threading.SpinWait.SpinUntil(() => false, 1000);
            }

            return yesAnswer;
        }

        private void OnStartToasting(object sender, RoutedEventArgs e)
        {
            this.buttonStart.IsEnabled = false;
            this.buttonStop.IsEnabled = true;
            this.slider.IsEnabled = false;
            this.textBlock.Text = string.Format("Toasting at {0}%", this.slider.Value);
            this.imageHot.Visibility = Visibility.Visible;
        }

        private void OnStopToasting(object sender, RoutedEventArgs e)
        {
            this.buttonStart.IsEnabled = true;
            this.buttonStop.IsEnabled = false;
            this.slider.IsEnabled = true;
            this.textBlock.Text = "Ready";
            this.imageHot.Visibility = Visibility.Collapsed;
        }

        private async void OnCheckForUpdates(object sender, RoutedEventArgs e)
        {
            bool updatesAvailable = await DMClient.CheckForUpdatesAsync();
            if (updatesAvailable)
            {
                System.Diagnostics.Debug.WriteLine("updates available");
                var dlg = new UserDialog("Updates available. Install?");
                await dlg.ShowAsync();
                // Don't do anything yet
            }
        }

        private async void RestartSystem()
        {
            bool success = true;
            try
            {
                await DMClient.RebootSystemAsync();
            }
            catch(Exception)
            {
                success = false;
            }

            StatusText.Text = success?  "Succeeded!" : "Failed!";
        }

        private void OnSystemRestart(object sender, RoutedEventArgs e)
        {
            RestartSystem();
        }

        private async void FactoryReset()
        {
            bool success = true;
            try
            {
                await DMClient.DoFactoryResetAsync();
            }
            catch (Exception)
            {
                success = false;
            }

            StatusText.Text = success? "Succeeded!" : "Failed!";
         }

        private void OnFactoryReset(object sender, RoutedEventArgs e)
        {
            RestartSystem();
        }

    }
}
