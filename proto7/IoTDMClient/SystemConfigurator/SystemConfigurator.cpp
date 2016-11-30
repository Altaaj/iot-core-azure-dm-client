#include "stdafx.h"
#include "..\include\dm_request.h"

class sysconfig_exception : public std::exception
{
    int error;
public:

    sysconfig_exception() : error(GetLastError()) {}
};

class security_attributes
{
    SECURITY_ATTRIBUTES _securityAttributes;
    PSID pEveryoneSID;
    PACL pACL;
    PSECURITY_DESCRIPTOR pSD;
public:
    security_attributes(DWORD permissions) : pEveryoneSID(nullptr), pACL(nullptr), pSD(nullptr)
    {
        SID_IDENTIFIER_AUTHORITY SIDAuthWorld = SECURITY_WORLD_SID_AUTHORITY;

        // Create a well-known SID for the Everyone group.
        if (!AllocateAndInitializeSid(&SIDAuthWorld, 1, SECURITY_WORLD_RID, 0, 0, 0, 0, 0, 0, 0, &pEveryoneSID))
        {
            printf("AllocateAndInitializeSid Error %u\n", GetLastError());
            return;
        }

        EXPLICIT_ACCESS ea = { 0 };
        ea.grfAccessPermissions = permissions;
        ea.grfAccessMode = SET_ACCESS;
        ea.grfInheritance = NO_INHERITANCE;
        ea.Trustee.TrusteeForm = TRUSTEE_IS_SID;
        ea.Trustee.TrusteeType = TRUSTEE_IS_WELL_KNOWN_GROUP;
        ea.Trustee.ptstrName = (LPTSTR)pEveryoneSID;

        // Create a new ACL that contains the new ACEs.
        DWORD dwRes = SetEntriesInAclW(1, &ea, NULL, &pACL);
        if (dwRes != ERROR_SUCCESS)
        {
            printf("SetEntriesInAcl Error %u\n", GetLastError());
            throw sysconfig_exception();
        }

        // Initialize a security descriptor.  
        pSD = (PSECURITY_DESCRIPTOR)LocalAlloc(LPTR, SECURITY_DESCRIPTOR_MIN_LENGTH);
        if (pSD == NULL)
        {
            printf("LocalAlloc Error %u\n", GetLastError());
            throw sysconfig_exception();
        }

        if (!InitializeSecurityDescriptor(pSD, SECURITY_DESCRIPTOR_REVISION))
        {
            printf("InitializeSecurityDescriptor Error %u\n", GetLastError());
            throw sysconfig_exception();
        }

        // Add the ACL to the security descriptor. 
        if (!SetSecurityDescriptorDacl(pSD, TRUE, pACL, FALSE))
        {
            printf("SetSecurityDescriptorDacl Error %u\n", GetLastError());
            throw sysconfig_exception();
        }

        // Initialize a security attributes structure.
        _securityAttributes.nLength = sizeof(SECURITY_ATTRIBUTES);
        _securityAttributes.lpSecurityDescriptor = pSD;
        _securityAttributes.bInheritHandle = FALSE;
    }

    ~security_attributes()
    {
        if (pEveryoneSID)
            FreeSid(pEveryoneSID);
        if (pACL)
            LocalFree(pACL);
        if (pSD)
            LocalFree(pSD);
    }

    LPSECURITY_ATTRIBUTES get_sa()
    {
        return &_securityAttributes;
    }
};

#define BUFSIZE 4096

dm_response process_command(const dm_request& request);

int main()
{
    HANDLE hPipe;

    security_attributes sa(GENERIC_WRITE | GENERIC_READ);

    const wchar_t* pipename = L"\\\\.\\pipe\\dm-client-pipe";

    hPipe = CreateNamedPipeW(
        pipename,
        PIPE_ACCESS_DUPLEX,
        PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE | PIPE_WAIT,
        PIPE_UNLIMITED_INSTANCES,
        BUFSIZE,
        BUFSIZE,
        NMPWAIT_USE_DEFAULT_WAIT,
        sa.get_sa());

    if (hPipe != INVALID_HANDLE_VALUE)
    {
        while (true)
        {
            printf("Waiting for someone to connect...\n");
            if (ConnectNamedPipe(hPipe, NULL) != FALSE)
            {
                dm_request request;
                DWORD dwRead;
                //while (ReadFile(hPipe, buffer, sizeof(buffer) - 1, &dwRead, NULL) != FALSE)
                BOOL readResult = ReadFile(hPipe, &request, sizeof(request), &dwRead, NULL);
                if(readResult && dwRead == sizeof(request))
                {
                    dm_response response = process_command(request);

                    DWORD dwWritten;
                    BOOL writeResult = WriteFile(hPipe,
                        &response,
                        sizeof(response),
                        &dwWritten,
                        NULL);

                    if (!writeResult) {
                        printf("WriteFile Error %d\n", GetLastError());
                        throw sysconfig_exception();
                    }

                }
                else {
                    printf("ReadFile Error %d\n", GetLastError());
                    throw sysconfig_exception();
                }
            }
            else
            {
                printf("ConnectNamedPipe Error %d\n", GetLastError());
                throw sysconfig_exception();
            }
            DisconnectNamedPipe(hPipe);
        }
    }
    else
    {
        printf("CreateNamedPipe Error %d\n", GetLastError());
        throw sysconfig_exception();
    }

    return 0;
}


