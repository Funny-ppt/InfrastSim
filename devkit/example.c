#include <stdio.h>
#include <windows.h>

int main() {
    // 加载动态链接库
    HINSTANCE hGetProcIDDLL = LoadLibrary("InfrastSim.dll");

    if (!hGetProcIDDLL) {
        printf("Could not load the dynamic library.\n");
        return EXIT_FAILURE;
    }

    // 获取CreateSimulator函数的地址
    int (*pCreateSimulator)() =
        (int (*)())GetProcAddress(hGetProcIDDLL, "CreateSimulator");

    if (!pCreateSimulator) {
        printf("Could not locate the function CreateSimulator.\n");
        return EXIT_FAILURE;
    }

    // 调用CreateSimulator函数
    int simId = pCreateSimulator();

    // 获取GetData函数的地址
    const char *(*pGetData)(int, int) =
        (const char *(*)(int, int))GetProcAddress(hGetProcIDDLL, "GetData");

    if (!pGetData) {
        printf("Could not locate the function GetData.\n");
        return EXIT_FAILURE;
    }

    // 调用GetData函数
    const char *data = pGetData(simId, 1);
    printf("Data: %s\n", data);

    // 释放动态链接库
    FreeLibrary(hGetProcIDDLL);

    return 0;
}
