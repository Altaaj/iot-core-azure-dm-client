#pragma once

#include <mutex>
#include <string>
#include <sstream>
#include "Utils.h"

#define TRACE_FILENAME L"c:\\temp\\IoTDMAgent.log"
#define TRACE_MAX_LEN 512

class Logger
{
public:
    Logger(bool console, const wchar_t* logFileName);

    void Log(const char*  message);
    void Log(const wchar_t*  message);

    template<class T>
    void Log(const wchar_t* format, T param)
    {
        std::basic_ostringstream<wchar_t> message;
        message << format << L' ' << param;
        Log(message.str().c_str());
    }

    void Log(const char* format, const char* param);
    void Log(const char* format, int param);

private:
    std::mutex _mutex;

    bool _console;
    std::wstring _logFileName;
};

Logger __declspec(selectany) gLogger(true /*console output*/, TRACE_FILENAME);

#ifdef _DEBUG
#define TRACE(msg) gLogger.Log(msg)
#define TRACEP(format, param) gLogger.Log(format, param)
#else
#define TRACE(msg)
#define TRACEP(format, param)
#endif
