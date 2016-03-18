// CmdLine.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include <windows.h>
#include <iostream>

using namespace std;

HANDLE hOutRead, hOutWrite;
HANDLE hInRead, hInWrite;

DWORD WINAPI Read(LPVOID lp)
{
	CHAR Buf[4096] = { 0 };
	DWORD rt;

	while (ReadFile(hOutRead, Buf, 4095, &rt, NULL))
	{
		Buf[rt] = '\0';
		cout << Buf;
		Sleep(200);
	}

	return 0;
}

int _tmain(int argc, _TCHAR* argv[])
{
	SECURITY_ATTRIBUTES SA;

	SA.nLength = sizeof(SECURITY_ATTRIBUTES);
	SA.lpSecurityDescriptor = NULL;
	SA.bInheritHandle = TRUE;

	CreatePipe(&hOutRead, &hOutWrite, &SA, 0);
	CreatePipe(&hInRead, &hInWrite, &SA, 0);

	STARTUPINFO StartInfo;
	PROCESS_INFORMATION ProcInfo;
	ZeroMemory(&StartInfo, sizeof(STARTUPINFO));
	StartInfo.cb = sizeof(STARTUPINFO);
	GetStartupInfo(&StartInfo);
	StartInfo.hStdError = hOutWrite;
	StartInfo.hStdOutput = hOutWrite;
	StartInfo.hStdInput = hInRead;
	StartInfo.wShowWindow = SW_HIDE;
	StartInfo.dwFlags = STARTF_USESHOWWINDOW | STARTF_USESTDHANDLES;

	WCHAR ExeFile[1024] = L"cmd.exe";
	CreateProcess(NULL, ExeFile, NULL, NULL, TRUE, 0, NULL, NULL, &StartInfo, &ProcInfo);
	
	char command[1024];

	DWORD rt;

	HANDLE h = CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Read, 0, 0, &rt);
	CloseHandle(h);

	while (true)
	{
		cin >> command;
		command[strlen(command) + 1] = '\0';
		command[strlen(command)] = '\n';

		WriteFile(hInWrite, command, strlen(command), &rt, NULL);
	}

	return 0;
}
