## 1.IDA逆向分析

​	拿到可执行文件crackme.exe，导入IDA，找到 int main()程序段，发现只有一个主要函数，将其rename为func1，进入func1

![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/33366280-e6aa-43c7-9d67-7fcce8620e5c)


​	func1的前半部分有大量对xmm寄存器的移位和扩展操作，但是我们需要寻找的3个关键部分应该是①打印`请输入flag:`的函数、②读入flag的函数、③输出`错误`或`正确`的函数。故继续向下翻，找到一个行可疑指令，`8FB823EFh`比较突兀，起初猜测和打印的字符串有关，但其附近没有程序调用call，故先存疑，继续往下。

![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/9d016256-88ac-4b87-b4ad-e9b7cc2b3897)


​	终于找到关键的部分，注意到`Format`变量的内容是`%s`，这与输入输出有关，可猜测前后两次`call`的函数分别为`printf`和`scanf`。另外注意到这里有个可疑的**子线程创建**，但先标记一下，继续向下分析。

![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/af530d70-831e-48a9-83d2-c1107c2c0b28)


​	下面找到一个分支结构，也是func1的函数末尾，合理猜测是对flag的判断并输出''正确''或''错误''。

![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/58706d00-5383-4c81-bd8c-e7cdfd3246bf)


​	F5查看这一段的反汇编伪代码，`v37`猜测是之前scanf()函数读入的首地址并赋值给`v31`，`v31-48`猜测是为了取正确flag的对应字节地址，而`v33`是最后if判断的标志，即`test edx, edx`为1则错误为0则正确。

​	显然，在两个分支中进行赋值的可能是“正确”和“错误”的GBK2312编码，但别忽略分支结束后的6行xor异或指令！

​                                                  ![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/2a1a0572-3e8a-4b81-80dd-245cb6a4ccbc)
     

![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/112b9351-55a6-4331-8bd7-c511e7729092)


​	用以下c++脚本进行异或验证，最终结果表明前面的分析都是正确的。

```cpp
#include<cstdio>
using namespace std;
typedef unsigned char uch;
int main()
{
    uch code_hex_1[] = {
        0xd5,
        0xcb,
        0xff,
        0x8f,
        0x33,
        0x37,
        0x00,
        0x00
    };

    uch code_hex_2[] = {
        0xb4,
        0xdb,
        0xf9,
        0xcb,
        0x33,
        0x37,
        0x00,
        0x00
    };
    /// 0037338fffcbd5
    printf("异或前: ");
    for (int i = 7; i >= 0; i--)
        printf("%02x", code_hex_1[i]);
    printf("\n");
    for (uch i = 1; i <= 6; ++i)
        code_hex_1[i] ^= (uch)0x35 + i; 
    
    printf("异或后: ");       
    for (int i = 7; i >= 0; i--)
        printf("%02x", code_hex_1[i]);
    printf("\n%s", code_hex_1);

    printf("\n");
	
    /// 003733cbf9dbb4
    printf("异或前: ");
    for (int i = 7; i >= 0; i--)
        printf("%02x", code_hex_2[i]);
    printf("\n");
    for (uch i = 1; i <= 6; ++i)
        code_hex_2[i] ^= (uch)0x35 + i;   
    
    printf("异或后: ");     
    for (int i = 7; i >= 0; i--)
        printf("%02x", code_hex_2[i]);
    printf("\n%s", code_hex_2);

    return 0;
}
```

![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/dec3dde4-727f-4edb-bcc9-00317db5028e)


​	之后开启IDA debugger，对scanf()函数之后任意点下断点，直接访问`v32`内存，再往前平移`0x30`(10进制48)行就能发现flag啦！


![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/118de52a-f887-477c-a528-6961afff97f2)




![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/0f917d18-814e-4d55-8e8f-f7871928bfb2)


​	图中相对`db '111'` 向上0x30行处就是flag`HiGWDUuXQS6wVHBTp0ERfJe6VqprMqD1`

![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/2f37efb2-9d57-440c-b407-f0132dd29e83)




## 2.dll远程注入

​	

```cpp
/// 以下是根据视频总结的dll远程注入模版，共分为7步
void CreateRemoteThread_inject(char *ProcessName)
{
	//1. 先获取PID，FindProcess需要自己实现!
	if (FindProcess(ProcName, dwProcess, tids))
	{
		//2. 获取进程句柄
		HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, dwProcess);
		if (!hProcess){
			printf("[ERROR] Open Process failed !\n");
			return;}
		//3. 获取 kernel32.dll 模块的句柄
		HMODULE hKernel32 = GetModuleHandle(L"kernel32.dll");
		if (!hKernel32) {
			printf("[ERROR] Get handle of kernel32.dll failed !\n");
			return;}
		//4. 获取 LoadLibraryA 函数的地址
		FARPROC pLoadLibraryA = GetProcAddress(hKernel32, "LoadLibraryA");
		if (!pLoadLibraryA) {
			printf("[ERROR] Get adress of LoadLibraryA failed !\n");
			return;}
		//5. 分配进程中内存
		LPVOID allocatedMem = VirtualAllocEx(hProcess, NULL, sizeof(myDll), MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
		if (!allocatedMem){
			printf("[ERROR] Allocate memory failed !\n");
			return;}
		//6. 写入dll
		if (!WriteProcessMemory(hProcess, allocatedMem, myDll, sizeof(myDll), NULL)){
			printf("[ERROR] Write process memory failed !\n");
			return;}
		//7. 用LoadLibraryA地址创建远程线程
		// 这里LLA的地址是全win系统固定的，不需要专门去获取，库中有定义好的地址
		if(!CreateRemoteThread(hProcess, 0, 0, (LPTHREAD_START_ROUTINE)pLoadLibraryA, allocatedMem, 0, 0)){
			printf("[ERROR] Create remote thread failed !\n");
			return;
		}CloseHandle(hProcess);
	}else{
		printf("[ERROR] Process not found !\n");
		return;}
}
```

​	如果直接使用视频课程中给的示例代码，基本就可以实现dll远程注入。但是我在实际操作过程中发现如果crack.exe程序**打开时间过长**后会出现无法注入的情况。回到上文提到的func1前半部分`CreateThread`，猜测是新建的子线程防止了注入。

​	双击转到程序段，发现两次Sleep调用，且时间设置为10 s，又发现`0xC3`其实是`ret`指令的shellcode。

![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/a8392ee4-0724-46e6-9add-1af1c6699516)


​	F5反汇编查看伪代码，注意`ModuleName`中存放的是`kernelbase.dll`，`ProcName`中存放的是`LoadLibraryExW`，可以推断出`*v30 = -61`直接**将LoadLibraryExW函数的第一个字节改为ret**，相当于直接返回，这会导致**`LoadLibraryA`函数无法动态链接到注入的dll库**。

>   网上查找的资料显示LoadLibraryA和LoadLibraryExW函数可能有调用关系或共享某一段代码，它们都是用于加载动态链接库dll，而直接将LoadLibraryExW修改为ret会导致LoadLibraryA也无法正常工作

![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/cdec9ec1-ac15-4aaf-91a8-fbf0103d4b48)


![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/aec54499-8342-469f-873e-9129a9521287)


​	经过以上分析，在注入器代码中补充了注入前的LoadLibraryExW首字节检查，如果被修改则将其还原，下面是代码片段和演示：

```cpp
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
				if (byteValue == 0xC3) // 已经被篡改，需要还原
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
```

![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/40734062-f0a5-4a1b-9461-1aa96fdeda07)


​	下图显示如果首字节被篡改为`0xC3`，则还原为`0x40`，显示注入进程成功。

![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/823fff63-8eaf-40ed-a44f-081d1881e25b)


## 3.Hook程序代码

​	通过dll注入来hook程序代码，思路是直接用shellcode修改，最简单的hook方法应该就是将`test edx, edx`修改为`cmp edx, edx`因为后者可以永远保证结果为0，进入正确判断的分支。

![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/f3571842-bc9f-468a-992f-a463bb822626)


​	IDA中查看代码行地址，`test edx, edx`地址为`00007FF6FB4318E7`，结合进程首地址为`00007FF6FB430000`可推断该行代码地址偏移为`offset = 0x18e7`，dll中编写hook代码，修改进程在对应偏移地址下的内存即可。

>   test edx, edx 的shellcode 为 0x85 0xd2
>
>   cmp edx, edx的shellcode 为 0x39 0xd2

```
void hook()
{
	BYTE hookcode[10] = { 0x39, 0xd2 };
	BYTE rawcode[10];
    DWORD64 codeAdress;
    DWORD64 codeOffset = 0x18e7; // test edx, edx
    HMODULE hModule = GetModuleHandle(nullptr);
    DWORD_PTR baseAddress = reinterpret_cast<DWORD_PTR>(hModule);
    codeAdress = (DWORD64)baseAddress + codeOffset;
    printf("\nhook code address : % llXH\n", codeAdress);
    // 获取进程句柄
    HANDLE hProcess = GetCurrentProcess();
    // 修改内存保护权限
    VirtualProtect((LPVOID)codeAdress, 2, PAGE_EXECUTE_READWRITE, 0);
    //SIZE_T numberOfBytesWritten;
    // 读取原test shellcode，进行确认
    if(ReadProcessMemory(hProcess, (LPVOID)codeAdress, rawcode, 2, NULL))
        for (int i = 1; i >= 0; i--)printf(" 0x%02x ", rawcode[i]);
    else
        printf(" read failed !\n");
    // hook test code
    if (WriteProcessMemory(hProcess, (LPVOID)codeAdress, hookcode, 2, NULL))
        printf(" \nhook success !\n");
    else
        printf(" \nhook failed !\n");
}
```

![image](https://github.com/ZAIQuQ/TencentClientReverseLearning/assets/96275921/bd427156-54ee-4789-ae31-25668c46ed8e)
