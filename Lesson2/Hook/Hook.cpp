#include "Hook.h"
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <dlfcn.h>
#include <sys/mman.h>
#include <errno.h>
using namespace std;
typedef unsigned long ul;
unsigned char hookCommand[12] = { 0x00, 0x00, 0x9F, 0xE5, 0x00, 0xF0, 0xA0, 0xE1 };
char line[1024];
void* handle;
#define LOGI(...) ((void)__android_log_print(ANDROID_LOG_INFO, "libhook", __VA_ARGS__))
#define LOGW(...) ((void)__android_log_print(ANDROID_LOG_WARN, "libhook", __VA_ARGS__))

extern "C" {
    /*此简单函数返回平台 ABI，此动态本地库为此平台 ABI 进行编译。*/
    const char* Hook::getPlatformABI()
    {
#if defined(__arm__)
#if defined(__ARM_ARCH_7A__)
#if defined(__ARM_NEON__)
#define ABI "armeabi-v7a/NEON"
#else
#define ABI "armeabi-v7a"
#endif
#else
#define ABI "armeabi"
#endif
#elif defined(__i386__)
#define ABI "x86"
#else
#define ABI "unknown"
#endif
        LOGI("This dynamic shared library is compiled with ABI: %s", ABI);
        return "This native library is compiled with ABI: %s" ABI ".";
    }

    // Hook函数,使stringFromJNI函数恒返回1
    int hookProc()
    {
        LOGI("Succeed to hook, return 1 all the time !");
        return 1;
    }

    void hook()
    {
        void* lib_base = NULL;                  // 用于存放libhook.so的基地址
        unsigned long offset_0x1134 = 0x1134;   // 目标函数偏移地址
        void* addr_0x1134;                      // 目标函数地址
        FILE* fp;

        // 遍历/proc/com.example.crackme1/maps，查找libhook.so地址
        
        fp = fopen("/proc/self/maps", "rt");
        do
        {
            fgets(line, 1024, fp);
            if (strstr(line, "libcrackme1.so") != NULL)
            {
                sscanf(line, "%lx", (ul*)&lib_base);
                LOGI("libcrackme1.so at 0x%lx", (ul)lib_base);

                // 计算目标函数真实地址
                addr_0x1134 = (void*)((ul)lib_base + offset_0x1134);
                LOGI("proc_0x1134 at 0x%lx", (ul)addr_0x1134);
                LOGI("proc_hook at 0x%lx", (ul)hookProc);

                // 修改段保护
                mprotect(lib_base, 0x2000, PROT_READ | PROT_WRITE | PROT_EXEC);

                // 补充跳转地址为hookProc
                *(ul*)(hookCommand + 8) = (unsigned long)hookProc;

                // 写入hook指令
                memcpy(addr_0x1134, (void*)hookCommand, 12);

                return;
            }
        } while (strlen(line));

        if (lib_base == NULL)
        {
            LOGW("libcrackme1.so not found !");
            return;
        }
    }

    Hook::Hook()
    {
        getPlatformABI();
    }

    Hook::~Hook()
    {

    }
}

void init()
{
    //  用constructor属性指定的函数，会在目标文件加载的时候自动执行，发生在main函数执行以前，常常用来隐形得做一些初始化工作。
    //  直接调用函数hook()
    hook();

    return;
}