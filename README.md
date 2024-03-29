## InfrastSim

一个简单的明日方舟基建模拟库和服务器实现
支持跨语言调用: C DLL, HTTP(S) 服务器, wasm

[模拟器在线体验地址](https://infrastsim.zhaozuohong.vip/)
[模拟器UI仓库](https://github.com/Funny-ppt/InfrastSimUI)


### TODO list

 - [ ] 重构SourceGenerator对于干员的部分，使得干员可以使用属性声明甚至实现简单干员完全自动生成
 - [ ] 优化执行流程，实现'编译模式'，该模式生成的代码可以通过编译器优化最大限度移除对字符串字面量的依赖
   - [ ] 更进一步，移除AggerateValue中对于字典的依赖


### Contributor Guide
*WIP*