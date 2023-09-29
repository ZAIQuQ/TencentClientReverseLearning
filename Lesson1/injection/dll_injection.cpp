#include <Windows.h>
#include <string>
#include <cstdio>
#include <processthreadsapi.h>
#include <WinUser.h>
#include <TlHelp32.h>
#include <Psapi.h>
#include <vector>
using namespace std;
const int MaxLen = 0x100;
//const char myDll[] = "pingdll.dll";
const char myDll[] = "pingdll.dll";

bool FindProcess(wstring& processName, DWORD& dwProcess, vector<DWORD>& tids)
{
	// 获取进程快照
	HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	if (snapshot == INVALID_HANDLE_VALUE) {
		return false;
	}
	PROCESSENTRY32 processEntry;
	processEntry.dwSize = sizeof(PROCESSENTRY32);

	// 遍历
	if (Process32First(snapshot, &processEntry)) {
		do {
			if (_wcsicmp(processEntry.szExeFile, processName.c_str()) == 0) {
				dwProcess = processEntry.th32ProcessID;
				break;
			}
		} while (Process32Next(snapshot, &processEntry));
	}

	// 关闭快照
	CloseHandle(snapshot);

	if (dwProcess == 0) return false;

	return true;
}

void CreateRemoteThread_inject(char *ProcessName)
{

	DWORD dwProcess;
	vector<DWORD> tids;
	wchar_t procName[MaxLen];

	// char to wchar
	MultiByteToWideChar(CP_ACP,
		MB_PRECOMPOSED,
		(LPCCH)ProcessName,
		strlen(ProcessName) + 1,
		procName,
		(int)((strlen(ProcessName) + 1) * sizeof(WCHAR)));

	// wchar to wstring
	wstring wprocName(procName);

	if (FindProcess(wprocName, dwProcess, tids))
	{
		printf("[MESSAGE] PID = %d\n", dwProcess);

		// 获取进程句柄
		HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, dwProcess);
		if (!hProcess)
		{
			printf("[ERROR] Open Process failed !\n");
			return;
		}
		
		// 获取 kernel32.dll 模块的句柄
		HMODULE hKernel32 = GetModuleHandle(L"kernel32.dll");
		if (!hKernel32) {
			printf("[ERROR] Get handle of kernel32.dll failed !\n");
			return;
		}
		// 获取 kernelbase.dll 模块的句柄
		HMODULE hKernelbase = GetModuleHandle(L"kernelbase.dll");
		if (!hKernelbase) {
			printf("[ERROR] Get handle of kernelbase.dll failed !\n");
			return;
		}

		// 获取 LoadLibraryA 函数的地址
		FARPROC pLoadLibraryA = GetProcAddress(hKernel32, "LoadLibraryA");
		if (!pLoadLibraryA) {
			printf("[ERROR] Get adress of LoadLibraryA failed !\n");
			return;
		}
		// 获取 LoadLibraryExW 函数的地址
		FARPROC pLoadLibraryExW = GetProcAddress(hKernelbase, "LoadLibraryExW");
		if (!pLoadLibraryExW) {
			printf("[ERROR] Get adress of LoadLibraryExW failed !\n");
			return;
		}

		BYTE byteValue;
		SIZE_T bytesRead;
		// 读取 LoadLibraryExW 函数的首字节
		if (ReadProcessMemory(hProcess, pLoadLibraryExW, &byteValue, sizeof(byteValue), &bytesRead) )
		{
			if (bytesRead == sizeof(byteValue))
			{
				printf("[MESSAGE] The first byte of LoadLibraryExW is %02XH\n", byteValue);
				if (byteValue == 0xC3)
				{
					byteValue = 0x40;
					printf("[WARNING] The fist byte was tampered, change to 40H\n");
					if (!WriteProcessMemory(hProcess, pLoadLibraryExW, &byteValue, sizeof(byteValue), &bytesRead)) {
						printf("[ERROR] Write the first byte of LoadLibraryExW failed !\n");
						return;
					}
				}
			}
			else
			{
				printf("[ERROR] Read the first byte of LoadLibraryExW failed !\n");
				return;
			}
		}
		else
		{
			printf("[ERROR] Read memory of LoadLibraryExW failed !\n");
			return;
		}

		// 分配进程中内存
		LPVOID allocatedMem = VirtualAllocEx(hProcess, NULL, sizeof(myDll), MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
		if (!allocatedMem)
		{
			printf("[ERROR] Allocate memory failed !\n");
			return;
		}
		printf("[MESSAGE] Allocated memory at %11XH\n", allocatedMem);

		// 写入dll
		if (!WriteProcessMemory(hProcess, allocatedMem, myDll, sizeof(myDll), NULL))
		{
			printf("[ERROR] Write process memory failed !\n");
			return;
		}

		// 用LoadLibraryA进行注入
		// 这里LLA的地址是全win系统固定的，不需要专门去获取，库中有定义好的地址
		if(!CreateRemoteThread(hProcess, 0, 0, (LPTHREAD_START_ROUTINE)pLoadLibraryA, allocatedMem, 0, 0))
		{
			printf("[ERROR] Create remote thread failed !\n");
			return;
		}

		printf("[MESSAGE] Injection success !\n");

		CloseHandle(hProcess);
	}
	else
	{
		printf("[ERROR] Process not found !\n");
		return;
	}
	
}

int main(int argc, char *argv[])
{
	if (argc != 2)
	{
		printf("[ERROR]  arg error !\n");
		printf("[MESSAGE]  please use with %s [ProcessName]", argv[0]);
		return 0;
	}
	CreateRemoteThread_inject(argv[1]);

	return 0;
}

