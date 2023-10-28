InfrastSim.dll
编译: 安装 .NET 8 相关工具后，git 把 InfrastSim 库拉到本地，在根目录调用
	dotnet public -r win-x64(linux-x64) InfrastSimExports
      项目 InfrastSim/InfrastSimExports 构建后得到的 InfrastSimExpors.dll(.so) 文件重命名即为该文件

simulator_service.h	- InfrastSim.dll 的函数原型和枚举定义
example.c		- windows下以动态库的形式调用的例子(适用于任何可以和C互操作的语言)
example_linux.c		- linux下以动态库的形式调用的例子(适用于任何可以和C互操作的语言)
InfrastSim.py		- python以动态库的形式调用的例子
InfrastSimWebBiding.py	- python访问Web服务的例子