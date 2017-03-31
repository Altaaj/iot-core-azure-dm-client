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
#pragma once
#include <thread>
#include <atomic>
#include <future>
#include "TaskQueue.h"
#include "Utilities\Limpet.h"
#include "AzureProxy.h"
#include "AzureModels\ModelManager.h"

class IoTDMService
{
    class ConnectionString
    {
    public:
        ConnectionString()
        {
            _expirationTimeSpan = std::chrono::hours(24); // 1 day
            _threshold = std::chrono::minutes(15);        // 15 minutes

            Generate();
        }

        std::string Generate()
        {
            _expiration = std::chrono::system_clock::now() + _expirationTimeSpan;
            return Limpet::GetInstance().GetSASToken(0, _expiration);
        }

        bool IsAboutToExpire()
        {
            return _expiration - std::chrono::system_clock::now() <= _threshold;
        }
    private:
        std::chrono::time_point<std::chrono::system_clock> _expiration;
        std::chrono::duration<int, std::ratio<3600i64>> _expirationTimeSpan; 
        std::chrono::duration<int, std::ratio<60i64>> _threshold;
    };

public:

    IoTDMService(const std::wstring& serviceName);

    static void IoTDMService::Install(
        const std::wstring&  serviceName,
        const std::wstring&  displayName,
        DWORD startType,
        const std::wstring&  dependencies,
        const std::wstring&  account,
        const std::wstring&  password);

    static void IoTDMService::Uninstall(const std::wstring&  serviceName);

    static void Run(IoTDMService &service);

private:

    // Methods
    static void WINAPI ServiceMain(DWORD argc, LPWSTR *argv);
    static void WINAPI ServiceCtrlHandler(DWORD ctrl);

    static void ServiceWorkerThread(void* context);
    void ServiceWorkerThreadHelper();

    void Start(DWORD argc, PWSTR *argv);
    void Stop();
    void Pause();
    void Continue();
    void Shutdown();

    virtual void OnStart(DWORD argc, LPWSTR *lpszArgv);
    virtual void OnStop();

    void DisableEnqueue();

    // Helpers
    void SetServiceStatus(DWORD currentState, DWORD win32ExitCode = NO_ERROR);

    void WriteEventLogEntry(const std::wstring& message, WORD type);
    void WriteErrorLogEntry(const std::wstring& function, DWORD errorCode = GetLastError());

    static void RenewConnectionString(IoTDMService* pThis);

    // Data members
    static IoTDMService* s_service;

    std::wstring _name;
    SERVICE_STATUS _status;
    SERVICE_STATUS_HANDLE _statusHandle;

    ConnectionString _connectionString;

    // threads
    Utils::JoiningThread _workerThread;
    Utils::JoiningThread _connectionRenewerThread;

    TaskQueue _taskQueue;

    // Synchronization between worker thread and main thread for exiting...
    std::atomic<bool> _stopSignaled;
    bool _renewConnectionString;

    // Azure proxy
    AzureProxy _cloudProxy;

    // Models
    ModelManager _modelManager;
};
