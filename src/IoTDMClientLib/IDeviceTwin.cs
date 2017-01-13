﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Devices.Management
{
    // This interface allows the client to send reported properties
    // to an unspecified DeviceTwin. The abstraction allows us to substitute different
    // DT implementations, such as DT in Azure or DT on prem.
    // The DM only needs an ability to send reported properties to DT
    public interface IDeviceTwin
    {
        void SetManagementClient(DeviceManagementClient deviceManagementClient);

        void ReportProperties(Dictionary<string, object> collection);

        // This API is called when the connection has expired (such as when SAS token has expired)
        void RefreshConnection();
    }
}