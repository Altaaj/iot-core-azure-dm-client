﻿/*
Copyright 2017 Microsoft
Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
and associated documentation files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace Microsoft.Devices.Management.DMDataContract
{
    public class WindowsUpdatePolicyDataContract
    {
        public const string NotFound = "<not found>";
        public const string Unexpected = "<unexpected>";
        public const string SectionName = "windowsUpdatePolicy";

        public const string JsonYes = "yes";
        public const string JsonNo = "no";

        public const string JsonActiveHoursStart = "activeHoursStart";
        public const string JsonActiveHoursEnd = "activeHoursEnd";
        public const string JsonAllowAutoUpdate = "allowAutoUpdate";
        public const string JsonAllowUpdateService = "allowUpdateService";
        public const string JsonBranchReadinessLevel = "branchReadinessLevel";

        public const string JsonDeferFeatureUpdatesPeriod = "deferFeatureUpdatesPeriod";
        public const string JsonDeferQualityUpdatesPeriod = "deferQualityUpdatesPeriod";
        public const string JsonPauseFeatureUpdates = "pauseFeatureUpdates";
        public const string JsonPauseQualityUpdates = "pauseQualityUpdates";
        public const string JsonScheduledInstallDay = "scheduledInstallDay";

        public const string JsonScheduledInstallTime = "scheduledInstallTime";

        public const string JsonRing = "ring";
        public const string JsonEarlyAdopter = "EarlyAdopter";
        public const string JsonPreview = "Preview";
        public const string JsonGeneralAvailability = "GeneralAvailability";

        public const string JsonApplyProperties = "applyProperties";
        public const string JsonReportProperties = "reportProperties";

        public const string JsonSourcePriority = "sourcePriority";

        public class WUProperties
        {
            public int activeHoursStart;
            public int activeHoursEnd;
            public int allowAutoUpdate;
            public int allowUpdateService;
            public int branchReadinessLevel;

            public int deferFeatureUpdatesPeriod;    // in days
            public int deferQualityUpdatesPeriod;    // in days
            public int pauseFeatureUpdates;
            public int pauseQualityUpdates;
            public int scheduledInstallDay;
            public int scheduledInstallTime;

            public string ring;

            public string sourcePriority;

            public static WUProperties FromJsonObject(JObject json)
            {
                WUProperties wuProperties = new WUProperties();
                wuProperties.activeHoursStart = Utils.GetInt(json, JsonActiveHoursStart, -1);
                wuProperties.activeHoursEnd = Utils.GetInt(json, JsonActiveHoursEnd, -1);
                wuProperties.allowAutoUpdate = Utils.GetInt(json, JsonAllowAutoUpdate, -1);
                wuProperties.allowUpdateService = Utils.GetInt(json, JsonAllowUpdateService, -1);
                wuProperties.branchReadinessLevel = Utils.GetInt(json, JsonBranchReadinessLevel, -1);

                wuProperties.deferFeatureUpdatesPeriod = Utils.GetInt(json, JsonDeferFeatureUpdatesPeriod, -1);
                wuProperties.deferQualityUpdatesPeriod = Utils.GetInt(json, JsonDeferQualityUpdatesPeriod, -1);
                wuProperties.pauseFeatureUpdates = Utils.GetInt(json, JsonPauseFeatureUpdates, -1);
                wuProperties.pauseQualityUpdates = Utils.GetInt(json, JsonPauseQualityUpdates, -1);
                wuProperties.scheduledInstallDay = Utils.GetInt(json, JsonScheduledInstallDay, -1);

                wuProperties.scheduledInstallTime = Utils.GetInt(json, JsonScheduledInstallTime, -1);

                wuProperties.ring = Utils.GetString(json, JsonRing, NotFound);

                wuProperties.sourcePriority = Utils.GetString(json, JsonSourcePriority, NotFound);

                return wuProperties;
            }

            public JObject ToJsonObject()
            {
                return JObject.FromObject(this);
            }
        }

        public class DesiredProperties
        {
            public WUProperties applyProperties;
            public string reportProperties;

            public static DesiredProperties FromJsonObject(JObject jObj)
            {
                DesiredProperties desiredProperties = new DesiredProperties();

                JToken jValue;
                if (jObj.TryGetValue(JsonApplyProperties, out jValue) && jValue is JObject)
                {
                    desiredProperties.applyProperties = WUProperties.FromJsonObject((JObject)jValue);
                }
                if (jObj.TryGetValue(JsonReportProperties, out jValue) && jValue is JValue && jValue.Type == JTokenType.String)
                {
                    desiredProperties.reportProperties = (string)jValue;
                }

                return desiredProperties;
            }
        }
    }
}