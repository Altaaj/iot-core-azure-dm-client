﻿using Microsoft.Devices.Management.Message;
using Microsoft.Devices.Management;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace IoTDMClient
{
    internal class AppBlobInfo
    {
        public string PackageFamilyName { get; set; }
        public BlobInfo Appx { get; set; }
        public List<BlobInfo> Dependencies { get; set; }

        public async Task<string> AppInstallAsync(DeviceManagementClient client)
        {
            var cleanup = new List<string>();
            var result = "install failed";
            var repeatCount = 5;
            try
            {
                var appInstallInfo = new AppInstallInfo();

                foreach (var dependencyBlobInfo in Dependencies)
                {
                    var depPath = await dependencyBlobInfo.DownloadToTemp();
                    appInstallInfo.Dependencies.Add(depPath);
                    cleanup.Add(dependencyBlobInfo.BlobName);
                }

                var path = await Appx.DownloadToTemp();
                appInstallInfo.AppxPath = path;
                cleanup.Add(Appx.BlobName);

                appInstallInfo.PackageFamilyName = PackageFamilyName;
                await client.InstallAppAsync(appInstallInfo);

                result = "install succeeded";
            }
            catch (Exception e)
            {
                result += (": " + e.Message);
            }
            finally
            {
                for (int i= 0; i < cleanup.Count; i++)
                {
                    try
                    {
                        var file = cleanup[i];
                        var deleteFile = await ApplicationData.Current.TemporaryFolder.GetFileAsync(file);
                        await deleteFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                    catch (Exception e)
                    {
                        // sharing violation indicating that install is not done with appx file yet
                        if (e.HResult == -2147024864 && --repeatCount > 0)
                        {
                            // wait for install to finish with file and try again ... limit the retries to repeatCount
                            new System.Threading.ManualResetEvent(false).WaitOne(1000);
                            i--;
                            continue;
                        }
                    }
                }
            }

            var response = JsonConvert.SerializeObject(new { response = result });
            return response;
        }
    }

    public class AppxManagement
    {
    }
}
