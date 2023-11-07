// Ptrace-based injector
#include <stdio.h>
#include <string.h>
#include <sys/ptrace.h>
#include <sys/mman.h>
#include <sys/wait.h>
#include <sys/user.h>
#include <sys/types.h>
#include <link.h>
#include <dlfcn.h>

typedef unsigned long ul;
const char libdl_dir[] = "/apex/com.android.runtime/lib/bionic/libdl.so"; // dlopen函数在libdl.so文件中
const char libc_dir[] = "/apex/com.android.runtime/lib/bionic/libc.so"; // malloc函数在libc.so文件中

void *d1open(pid_t pid, char *filename, int flag);

void *mall0c(pid_t pid, size_t len);

int ptrace_poketext(pid_t pid, size_t len, void *text, void *addr);

void *get_lib_base(pid_t pid, const char *lib_dir);

int main(int argc, const char *argv[])
{
    /* 
        第一个参数是进程名(apk包名)
        第二个参数是需要注入的.so库文件路径
    */
    pid_t pid = 0;
    FILE *fp = NULL;
    char process[256] = "";
    char pbuffer[256] = "";
    if (argc != 3)
    {
        printf("usage:\n%s [process_name] [lib_path]\n", argv[0]);
        return 0;
    }
    /// 1. 获取进程pid
    sprintf(process, "pidof %s", argv[1]); // 用shell指令得到pid
    if((fp = popen(process, "r")) == NULL )
    {
        printf("[WARNING] process not found !\n");
        return -1;
    }
    fread(pbuffer, 1, 256, fp);
    sscanf(pbuffer, "%d", &pid);
    printf("[MESSAGE] pid is %d\n", pid);
    pclose(fp);

    /// 2. 附加到进程
    const char *lib_dir = argv[2];
    if(ptrace(PTRACE_ATTACH, pid, NULL, NULL) < 0)
    {
        perror("[WARNING] failed to attach to the process !\n");
        return -1;
    }

    int status;

    waitpid(pid, &status, 0);

    printf("[MESSAGE] STATUS : 0x%x\n", status); // 0x137f

    printf("[MESSAGE] succeed to attach to the process !\n");

    /// 3. 远程函数调用
   
    // 远程调用malloc，为hook分配内存，写入lib文件路径

    void *buffer = mall0c(pid, 0x800);
    if( buffer == NULL )
    {
        perror("[WARNING] malloc failed !\n");

        printf("\n getchar() debugging ......\n");
        getchar();

        ptrace(PTRACE_CONT, pid, NULL, NULL);
        ptrace(PTRACE_DETACH, pid, NULL, NULL);
        return -1;
    }

    // 将hook lib写入进程内存

    ptrace_poketext(pid, strlen(lib_dir) + 1, (void *)lib_dir, buffer);

    // while(getchar() == 'q')
    //     ;

    // 将lib文件加载到进程

    if( d1open(pid, buffer, RTLD_LAZY) == NULL )
    {
        perror("[WARNING] load lib to process failed !\n");
        return -1;
    }

    /// 4. 解除附加
    printf("[MESSAGE] injection success !\n");
    ptrace(PTRACE_CONT, pid, NULL, NULL);
    ptrace(PTRACE_DETACH, pid, NULL, NULL);

    // printf("\n getchar() debugging ......\n");
    // getchar();

    return 0;
}

void *d1open(pid_t pid, char *filename, int flag)
{
    /*
        dlopen 2个参数 分别压入R0和R1中
        返回值在R0中
        LR寄存器存返回地址
        手动传参后，修改PC寄存器为目标函数地址，修改LR寄存器为0
        函数返回时触发异常，获取返回值
    */

    int status;
    struct user_regs pushed_regs;
    struct user_regs regs;
    void *local_lib_base;
    void *local_dlopen_addr;
    void *remote_lib_base;
    ul dlopen_offset;
    void *dlopen_addr;
    
    ptrace(PTRACE_GETREGS, pid, NULL, (void *)&pushed_regs);

    memcpy(&regs, &pushed_regs, sizeof(struct user_regs)); // 寄存器压栈
    dlopen_offset = 0x1849; // 用IDA读取elf文件libdl.so观察得出


    // 计算dlopen在远程进程中的地址
    remote_lib_base = get_lib_base(pid, libdl_dir);
    dlopen_addr = remote_lib_base + dlopen_offset;

    regs.uregs[13] -= 0x50;
    regs.uregs[15] = ((ul)dlopen_addr & 0xFFFFFFFE); // 函数地址最低位决定处理器模式
    regs.uregs[0] = (ul)filename; // R0
    regs.uregs[1] = (ul)flag; // R1

    // 这里没问题
    // while(getchar() == 'q')
    //     ;

    printf("[MESSAGE] Remote call: dlopen(0x%lx, 0x%lx)\n",(ul)filename, (ul)flag);

    // 之前是filename的问题，现在解决了
    // while(getchar() == 'q')
    //     ;

    if ((ul)dlopen_addr & 0x1) // PSR寄存器，第5位为1表Thumb模式，为0表ARM模式
    {
        regs.uregs[16] = regs.uregs[16] | 0x20;
    }
    else
    {
        regs.uregs[16] = regs.uregs[16] & 0xFFFFFFDF;
    }
    regs.uregs[14] = 0; // LR寄存器

    if(ptrace(PTRACE_SETREGS, pid, NULL, (void *)&regs) != 0 || ptrace(PTRACE_CONT, pid, NULL, NULL) != 0 )
    {
        perror("[WARNING] call dlopen() failed !\n");
        return NULL;
    }

    waitpid(pid, &status, WUNTRACED);

    printf("[MESSGAE] STATUS: 0x%x\n", status);

    while (status != 0xb7f) //过滤因返回地址为0触发异常而返回的错误码: 0xb7f
    {
        ptrace(PTRACE_CONT, pid, NULL, NULL);
        waitpid(pid, &status, WUNTRACED);
    }

    ptrace(PTRACE_GETREGS, pid, NULL, (void *)&regs);

    void *ret = (void *)regs.uregs[0];

    // 寄存器出栈
    ptrace(PTRACE_SETREGS, pid, NULL, (void *)&pushed_regs);

    printf("[MESSAGE] dlopen ret handle: 0x%lx\n", (ul)ret);
    return ret;
}


void *mall0c(pid_t pid, size_t len)
{
    /* 
        malloc 1个参数，压入R0中
        返回值在R0中
        LR寄存器存返回地址
        手动传参后，修改PC寄存器为目标函数地址，修改LR寄存器为0
        函数返回时触发异常，获取返回值
    */
    int status;
    struct user_regs pushed_regs;
    struct user_regs regs;
    ul malloc_offset;
    void *remote_lib_base;
    void *malloc_addr;

    malloc_offset = 0x2d685; // 用IDA读取elf文件libc.so观察得出


    // 计算malloc在远程进程中的地址
    remote_lib_base = get_lib_base(pid, libc_dir);
    malloc_addr = remote_lib_base + malloc_offset;

    dlerror();
    if( ptrace(PTRACE_GETREGS, pid, NULL, (void *)&pushed_regs) < 0 )
    {
        perror("[WARNING] failed to push remote regs \n");
    } 
    memcpy(&regs, &pushed_regs, sizeof(struct user_regs)); // 寄存器压栈
    regs.uregs[13] -= 0x50; // 堆栈压80位
    regs.uregs[15] = ((ul)malloc_addr & 0xFFFFFFFE); // PC寄存器，地址最低位决定处理器模式
    regs.uregs[0] = (ul)len; // R0
    printf("[MESSAGE] Remote call: malloc(0x%lx)\n", (ul)len);
    if ((ul)malloc_addr & 0x1) // PSR寄存器，第5位为1表Thumb模式，为0表ARM模式
    {
        regs.uregs[16] = regs.uregs[16] | 0x20;
    }
    else
    {
        regs.uregs[16] = regs.uregs[16] & 0xFFFFFFDF;
    }
    regs.uregs[14] = 0; // LR寄存器

    if(ptrace(PTRACE_SETREGS, pid, NULL, (void *)&regs) < 0 || ptrace(PTRACE_CONT, pid, NULL, NULL) < 0 )
    {
        perror("[WARNING] call malloc() failed !\n");
        return NULL;
    }

    waitpid(pid, &status, WUNTRACED);
    while (status != 0xb7f) // 过滤因返回地址为0触发异常而返回的错误码: 0xb7f
    {
        ptrace(PTRACE_CONT, pid, NULL, NULL);
        waitpid(pid, &status, WUNTRACED);
    }

    ptrace(PTRACE_GETREGS, pid, NULL, (void *)&regs);

    void *ret = (void *)regs.uregs[0];

    // 寄存器出栈
    ptrace(PTRACE_SETREGS, pid, NULL, (void *)&pushed_regs);

    printf("[MESSAGE] malloc ret mem: 0x%lx\n", (ul)ret);
    return ret;
}

int ptrace_poketext(pid_t pid, size_t len, void *text, void *addr)
{
    /*
        向远程进程的指定内存写入内容
    */
    const int batch_size = 4; // 一批4个字节
    unsigned int tmp_text;
    size_t writen;
    for (writen = 0; writen < len; writen += batch_size)
    {
        if (len - writen >= 4)
        {
            tmp_text = *(unsigned int *)(text + writen);
        }
        else
        {
            // 不足4个字节时，需要先读取原数据，保留原地址处的高位数据
            tmp_text = ptrace(PTRACE_PEEKDATA, pid, (void *)(addr + writen), NULL);
            for (int i = 0; i < len - writen; i++)
            {
                *(((unsigned char *)(&tmp_text)) + i) = *(unsigned char *)(text + writen + i);
            }
        }
        if (ptrace(PTRACE_POKEDATA, pid, (void *)(addr + writen), (void *)(tmp_text)) < 0)
        {
            return -1;
        }
    }
    return len;
}

void *get_lib_base(pid_t pid, const char *lib_dir)
{
    /*
        获取目标lib文件的基地址
        若pid = 0, 获取local进程的基地址
        若pid > 0, 获取远程进程的基地址
    */
    FILE *fp = NULL;
    char maps_path[1024] = "";
    char maps_line[1024] = "";
    void *lib_base = NULL;

    if(pid) sprintf(maps_path, "/proc/%d/maps", pid);
    else sprintf(maps_path, "/proc/self/maps");
    fp = fopen(maps_path, "rt");
    while(fgets(maps_line, 1024, fp) != NULL)
    {
        if(strstr(maps_line, lib_dir) != NULL)
        {
            sscanf(maps_line, "%lx", (unsigned long *)(&lib_base));
            break;
        }
    }
    fclose(fp);
    if(lib_base == NULL)
    {
        printf("[WARNING] %s not found in process %d\n", lib_dir, pid);
        return NULL;
    }

    printf("[MESSAGE] %s at mem 0x%lx in process %d\n", lib_dir, (unsigned long)lib_base, pid);

    return lib_base;
}