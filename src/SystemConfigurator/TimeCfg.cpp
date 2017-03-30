/*
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
#include "stdafx.h"
#include <vector>
#include <map>
#include "TimeCfg.h"
#include "..\SharedUtilities\Utils.h"
#include "..\SharedUtilities\TimeHelpers.h"
#include "..\SharedUtilities\JsonHelpers.h"
#include "..\SharedUtilities\Logger.h"
#include "..\SharedUtilities\DMException.h"
#include "CSPs\MdmProvision.h"

#include "Models\TimeInfo.h"

#define NtpServerPropertyName "NtpServer"

using namespace Windows::System;
using namespace Platform;
using namespace Windows::Data::Json;
using namespace Windows::System::Profile;
using namespace Windows::Foundation::Collections;

using namespace Microsoft::Devices::Management::Message;

using namespace std;
using namespace Utils;

#ifdef GetObject
#undef GetObject
#endif

void TimeCfg::Get(TimeInfo& info)
{
    TRACE(__FUNCTION__);

    unsigned long returnCode;
    std::string output;
    Utils::LaunchProcess(L"C:\\windows\\system32\\w32tm.exe /query /configuration", returnCode, output);
    if (returnCode != 0)
    {
        throw DMExceptionWithErrorCode("Error: w32tm.exe returned an error code.", returnCode);
    }

    vector<string> lines;
    Utils::SplitString(output, '\n', lines);
    for (const string& line : lines)
    {
        TRACEP("Line: ", line.c_str());

        vector<string> tokens;
        Utils::SplitString(line, ':', tokens);
        if (tokens.size() == 2)
        {
            string name = Utils::TrimString(tokens[0], string(" "));
            string value = Utils::TrimString(tokens[1], string(" "));

            // remove the trailing " (Local)".
            size_t pos = value.find(" (Local)");
            if (pos != -1)
            {
                value = value.replace(pos, value.length() - pos, "");
            }

            TRACEP("--> Found: ", (name + " = " + value).c_str());
            if (name == NtpServerPropertyName)
            {
                info.ntpServer = MultibyteToWide(value.c_str());
            }
        }
    }

    // Local time
    info.localTime = MdmProvision::RunGetString(L"./DevDetail/Ext/Microsoft/LocalTime");

    // Time zone info...
    TIME_ZONE_INFORMATION tzi = { 0 };
    GetTimeZoneInformation(&info.timeZoneInformation);
}

void TimeCfg::Set(SetTimeInfoRequest^ setTimeInfoRequest)
{
    TRACE(__FUNCTION__);

    SetNtpServer(setTimeInfoRequest->ntpServer->Data());

    TIME_ZONE_INFORMATION tzi = { 0 };

    // Bias...
    tzi.Bias = setTimeInfoRequest->timeZoneBias;

    TRACEP("Bias: ", to_string(setTimeInfoRequest->timeZoneBias).c_str());

    TRACEP("Standard Bias: ", to_string(setTimeInfoRequest->timeZoneStandardBias).c_str());
    TRACEP(L"Standard Name: ", setTimeInfoRequest->timeZoneStandardName->Data());
    TRACEP(L"Standard Date: ", setTimeInfoRequest->timeZoneStandardDate->Data());

    TRACEP("Daytime Bias: ", to_string(setTimeInfoRequest->timeZoneDaylightBias).c_str());
    TRACEP(L"Daytime Name: ", setTimeInfoRequest->timeZoneDaylightName->Data());
    TRACEP(L"Daytime Date: ", setTimeInfoRequest->timeZoneDaylightDate->Data());

    // Standard...
    wcsncpy_s(tzi.StandardName, setTimeInfoRequest->timeZoneStandardName->Data(), _TRUNCATE);
    if (!SystemTimeFromISO8601(setTimeInfoRequest->timeZoneStandardDate->Data(), tzi.StandardDate))
    {
        throw DMExceptionWithErrorCode("Error: invalid date/time format. Error Code = ", GetLastError());
    }
    tzi.StandardBias = setTimeInfoRequest->timeZoneStandardBias;

    // Daytime...
    wcsncpy_s(tzi.DaylightName, setTimeInfoRequest->timeZoneDaylightName->Data(), _TRUNCATE);
    if (!SystemTimeFromISO8601(setTimeInfoRequest->timeZoneDaylightDate->Data(), tzi.DaylightDate))
    {
        throw DMExceptionWithErrorCode("Error: invalid date/time format. Error Code = ", GetLastError());
    }
    tzi.DaylightBias = setTimeInfoRequest->timeZoneDaylightBias;

    // Set it...
    if (!SetTimeZoneInformation(&tzi))
    {
        throw DMExceptionWithErrorCode("Error: failed to set time zone information. Error Code = ", GetLastError());
    }

    TRACE(L"Time settings have been applied successfully.");
}

void TimeCfg::SetNtpServer(const std::wstring& ntpServer)
{
    TRACE(__FUNCTION__);
    TRACEP(L"New NTP Server = ", ntpServer.c_str());

    wstring command = L"";
    command += L"w32tm /config /manualpeerlist:";
    command += ntpServer;
    command += L" /syncfromflags:manual /reliable:yes /update";

    unsigned long returnCode = 0;
    std::string output;
    Utils::LaunchProcess(command, returnCode, output);
    if (returnCode != 0)
    {
        throw DMExceptionWithErrorCode("Error: w32tm.exe returned an error.", returnCode);
    }
}

GetTimeInfoResponse^ TimeCfg::Get()
{
    TRACE(__FUNCTION__);
    GetTimeInfoResponse ^response;
    try
    {
        TimeInfo info;
        Get(info);
        response = ref new GetTimeInfoResponse(ResponseStatus::Success);
        response->localTime = ref new String(info.localTime.c_str());
        response->ntpServer = ref new String(info.ntpServer.c_str());
        response->timeZoneBias = info.timeZoneInformation.Bias;
        response->timeZoneStandardName = ref new String(info.timeZoneInformation.StandardName);
        response->timeZoneStandardDate = ref new String(Utils::ISO8601FromSystemTime(info.timeZoneInformation.StandardDate).c_str());
        response->timeZoneStandardBias = info.timeZoneInformation.StandardBias;
        response->timeZoneDaylightName = ref new String(info.timeZoneInformation.DaylightName);
        response->timeZoneDaylightDate = ref new String(Utils::ISO8601FromSystemTime(info.timeZoneInformation.DaylightDate).c_str());
        response->timeZoneDaylightBias = info.timeZoneInformation.DaylightBias;
    }
    catch (...)
    {
        response = ref new GetTimeInfoResponse(ResponseStatus::Failure);
    }
    return response;
}
