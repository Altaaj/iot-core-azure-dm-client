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
        DeviceManagementClient deviceManagementClient;

        private readonly string DeviceConnectionString = ConnectionStringProvider.Value;

        private void EnableDeviceManagementUI(bool enable)
        {
            this.buttonRestart.IsEnabled = enable;
            this.buttonReset.IsEnabled = enable;
            this.buttonCheckUpdates.IsEnabled = enable;
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.buttonStart.IsEnabled = true;
            this.buttonStop.IsEnabled = false;

            // DM buttons will be enabled when we have created the DM client
            EnableDeviceManagementUI(false);
            this.imageHot.Visibility = Visibility.Collapsed;

#pragma warning disable 4014
            this.InitializeDeviceClientAsync();
#pragma warning restore 4014

        }

        private async Task InitializeDeviceClientAsync()
        {
            // Create DeviceClient. Application uses DeviceClient for telemetry messages, device twin
            // as well as device management
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString, TransportType.Mqtt);

            // IDeviceTwin abstracts away communication with the back-end.
            // AzureIoTHubDeviceTwinProxy is an implementation of Azure IoT Hub
            IDeviceTwin deviceTwinProxy = new AzureIoTHubDeviceTwinProxy(deviceClient);

            // IDeviceManagementRequestHandler handles device management-specific requests to the app,
            // such as whether it is OK to perform a reboot at any givem moment, according the app business logic
            // ToasterDeviceManagementRequestHandler is the Toaster app implementation of the interface
            IDeviceManagementRequestHandler appRequestHandler = new ToasterDeviceManagementRequestHandler(this);

            // Create the DeviceManagementClient, the main entry point into device management
            this.deviceManagementClient = await DeviceManagementClient.CreateAsync(deviceTwinProxy, appRequestHandler);

            EnableDeviceManagementUI(true);

            // Set the callback for desired properties update. The callback will be invoked
            // for all desired properties -- including those specific to device management
            await deviceClient.SetDesiredPropertyUpdateCallback(OnDesiredPropertyUpdate, null);
        }

        public Task OnDesiredPropertyUpdate(TwinCollection desiredProperties, object userContext)
        {
            // Let the device management client process properties specific to device management
            this.deviceManagementClient.ProcessDeviceManagementProperties(desiredProperties);

            // Application developer can process all the top-level nodes here
            return Task.CompletedTask;
        }

        // This method may get called on the DM callback thread - not on the UI thread.
        public async Task<bool> YesNo(string question)
        {
            var tcs = new TaskCompletionSource<bool>();

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                UserDialog dlg = new UserDialog(question);
                ContentDialogResult dialogResult = await dlg.ShowAsync();
                tcs.SetResult(dlg.Result);
            });

            bool yesNo = await tcs.Task;
            return yesNo;
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
            bool updatesAvailable = await deviceManagementClient.CheckForUpdatesAsync();
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
                await deviceManagementClient.ImmediateRebootAsync();
            }
            catch(Exception)
            {
                success = false;
            }

            StatusText.Text = success?  "Operation completed" : "Operation  failed";
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
                // The recovery partition guid is typically picked from a pre-defined set of guids
                // by the builder of the image. For our testing purposes, we have been using the following
                // guid.
                string recoveryPartitionGUID = "a5935ff2-32ba-4617-bf36-5ac314b3f9bf";
                await deviceManagementClient.DoFactoryResetAsync(false /*don't clear TPM*/, recoveryPartitionGUID);
            }
            catch (Exception)
            {
                success = false;
            }

            StatusText.Text = success? "Succeeded!" : "Failed!";
         }

        private void OnFactoryReset(object sender, RoutedEventArgs e)
        {
            FactoryReset();
        }
    }
}
