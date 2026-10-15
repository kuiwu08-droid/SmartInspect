# SmartInspect

**工业视觉检测桌面程序：图像检测、OK/NG 判定、结果追溯与模拟 PLC 通信。**

使用 C# / .NET 8、WinForms 和 OpenCvSharp 处理图像，通过 SQLite 保存检测记录，支持批次统计、历史查询和 JSON 参数配置。Modbus TCP 接口将检测结果回传给模拟 PLC。

控制流程采用 Trigger / Busy / Result / Done 握手，处理持续触发、断连、超时和数据库写入失败。验收报告记录了 38 项测试，包含正常检测与异常恢复场景。

## 查看项目

- [启动与演示步骤](02_Source/DEMO_GUIDE.md)
- [功能说明与运行环境](02_Source/README.md)
- [程序结构](02_Source/ARCHITECTURE.md)
- [测试报告](02_Source/Verification/TEST_REPORT.md)
- [Visual Studio 解决方案](02_Source/SmartInspect.sln)

## 实现内容

- 图像加载、检测与尺寸上下限判定，输出 OK / NG 结果。
- 单件记录保存、批次计数、良率统计和历史查询。
- 模拟 PLC 触发检测、读取状态和接收结果。
- 参数配置、日志记录、通信超时和异常恢复。

项目使用测试图像和模拟 PLC，用于验证检测软件及通信流程。运行数据、个人配置、凭据和编译输出不包含在仓库内。
