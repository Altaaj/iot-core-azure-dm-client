﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Devices.Management
{
    namespace RebootInfo
    {
        public class GetResponse
        {
            public string lastBootTime;
            public string lastRebootCmdTime;
            public string lastRebootCmdStatus;
            public string singleRebootTime;
            public string dailyRebootTime;
        }

        public class SetParams
        {
            public DateTime singleRebootTime;
            public DateTime dailyRebootTime;

            public string ToJson()
            {
                return "\"scheduledReboot\" : " + JsonConvert.SerializeObject(this);
            }
        }
    }

    namespace TimeInfo
    {
        public class GetResponse
        {
            public long timeZoneDaylightBias;
            public string timeZoneDaylightDate;
            public string timeZoneDaylightName;
            public long timeZoneStandardBias;
            public string timeZoneStandardDate;
            public string timeZoneStandardName;
            public long timeZoneBias;
            public string localTime;
            public string ntpServer;
        }

        public class SetParams
        {
            public long timeZoneDaylightBias;
            public string timeZoneDaylightDate;
            public string timeZoneDaylightName;
            public long timeZoneStandardBias;
            public string timeZoneStandardDate;
            public string timeZoneStandardName;
            public long timeZoneBias;
            public string ntpServer;

            public string ToJson()
            {
                return "\"timeInfo\" : " + JsonConvert.SerializeObject(this);
            }
        }
    }

    public struct DeviceInfo
    {
        public string id;
        public string manufacturer;
        public string model;
        public string dmVer;
        public string lang;

        public string type;
        public string oem;
        public string hwVer;
        public string fwVer;
        public string osVer;

        public string platform;
        public string processorType;
        public string radioSwVer;
        public string displayResolution;
        public string commercializationOperator;

        public string processorArchitecture;
        public string name;
        public string totalStorage;
        public string totalMemory;
        public string secureBootState;

        public string osEdition;
        public string batteryStatus;
        public string batteryRemaining;
        public string batteryRuntime;
    }

    public struct WindowsUpdatePolicyConfiguration
    {
        public uint activeHoursStart;
        public uint activeHoursEnd;
        public uint allowAutoUpdate;
        public uint allowMUUpdateService;
        public uint allowNonMicrosoftSignedUpdate;

        public uint allowUpdateService;
        public uint branchReadinessLevel;
        public uint deferFeatureUpdatesPeriod;    // in days
        public uint deferQualityUpdatesPeriod;    // in days
        public uint excludeWUDrivers;

        public uint pauseFeatureUpdates;
        public uint pauseQualityUpdates;
        public uint requireUpdateApproval;
        public uint scheduledInstallDay;
        public uint scheduledInstallTime;

        public string updateServiceUrl;

        public string ToJson()
        {
            return "\"windowsUpdatePolicy\" : " + JsonConvert.SerializeObject(this);
        }
    }

    public struct WindowsUpdateRebootPolicyConfiguration
    {
        public bool allow;

        public string ToJson()
        {
            return "\"windowsUpdateRebootPolicy\" : " + JsonConvert.SerializeObject(this);
        }
    }

    namespace WindowsUpdates
    {
        public class GetResponse
        {
            public string installed;
            public string approved;
            public string failed;
            public string installable;
            public string pendingReboot;
            public string lastScanTime;
            public bool deferUpgrade;
        }

        public class SetParams
        {
            public string approved;

            public string ToJson()
            {
                return "\"windowsUpdates\" : " + JsonConvert.SerializeObject(this);
            }
        }
    }

    public class FactorResetParams
    {
        public bool clearTPM;
        public string recoveryPartitionGUID;
    }

    class ExternalStorage
    {
        public string connectionString;
        public string container;

        public string ToJson()
        {
            return "\"externalStorage\" : " + JsonConvert.SerializeObject(this);
        }
    }

    class Certificates
    {
        public class CertificateConfiguration
        {
            public string rootCATrustedCertificates_Root;
            public string rootCATrustedCertificates_CA;
            public string rootCATrustedCertificates_TrustedPublisher;
            public string rootCATrustedCertificates_TrustedPeople;
            public string certificateStore_CA_System;
            public string certificateStore_Root_System;
            public string certificateStore_My_User;
            public string certificateStore_My_System;

            public string ToJson()
            {
                return "\"certificates\" : " + JsonConvert.SerializeObject(this);
            }
        }

        public uint Tag;
        public uint Status;
        public CertificateConfiguration Configuration;
    }

    class GetCertificateDetailsParams
    {
        public string path;
        public string hash;
        public string connectionString;
        public string containerName;
        public string blobName;
    }

    class GetCertificateDetailsResponse
    {
        public int Tag;
        public int Status;
        public string ValidTo;
        public string ValidFrom;
        public string IssuedTo;
        public string IssuedBy;
    }
}