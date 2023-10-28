#include "./simulator_service.h"
#include <stdio.h>
#include <dlfcn.h>

int main() {
    // 加载动态链接库
    void *handle = dlopen("./libInfrastSim.so", RTLD_LAZY);  // 注意使用库的正确路径和名称

    if (!handle) {
        fprintf(stderr, "Could not load the dynamic library: %s\n", dlerror());
        return -1;
    }

    // 清除任何现有的错误
    dlerror();

    // 获取CreateSimulator函数的地址
    int (*pCreateSimulator)() = (int (*)())dlsym(handle, "CreateSimulator");

    const char *dlsym_error = dlerror();
    if (dlsym_error) {
        fprintf(stderr, "Could not locate the function CreateSimulator: %s\n", dlsym_error);
        dlclose(handle);
        return -1;
    }

    // 调用CreateSimulator函数
    int simId = pCreateSimulator();

    // 获取GetData函数的地址
    const char *(*pGetData)(int, int) = (const char *(*)(int, int))dlsym(handle, "GetData");

    dlsym_error = dlerror();
    if (dlsym_error) {
        fprintf(stderr, "Could not locate the function GetData: %s\n", dlsym_error);
        dlclose(handle);
        return -1;
    }

    // 调用GetData函数
    const char *data = pGetData(simId, 1);
    printf("Data: %s\n", data);

    // 释放动态链接库
    dlclose(handle);

    return 0;
}
