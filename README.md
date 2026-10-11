# SmartInspect

SmartInspect 是 C# / .NET 8 Windows 工业视觉检测学习项目，使用 WinForms、OpenCvSharp、SQLite 和 Modbus TCP，支持图片检测、结果记录和模拟 PLC 握手。

## 当前项目入口

- 解决方案：[02_Source/SmartInspect.sln](02_Source/SmartInspect.sln)
- 当前功能与边界：[项目说明](02_Source/README.md)
- 启动和演示：[演示指南](02_Source/DEMO_GUIDE.md)
- 模块关系：[架构说明](02_Source/ARCHITECTURE.md)
- 测试说明：[验证报告](02_Source/Verification/TEST_REPORT.md)

使用测试图像及模拟 PLC，不代表真实工业相机或生产设备验收。运行数据、个人配置、凭据及编译输出不上传。

以下保留 v0.05 灾难恢复时的历史记录，不代表当前功能状态。

## 已完成

- `Product` 单件产品数据模型
- `List<Product>` 产品集合
- `SerialNumber`
- `Measurement`
- `Result`
- `ReadMeasurement(Product product)`
- `InspectProduct(Product product)`
- `GetFailedCount(int total, int ok)`
- `CalculateYield(int total, int ok)`
- 非法输入重试
- `9.5 <= Measurement <= 10.5` 判定为 `OK`
- 产品记录输出
- Total / OK / NG / Yield 汇总统计

## 尚未实现

- WinForms
- PLC
- Halcon
- 状态机
- 数据库
- MES

## 恢复说明

这是灾难恢复后重新生成的 README，只记录已经确认的 SmartInspect v0.05 功能状态。
