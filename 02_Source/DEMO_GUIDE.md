# SmartInspect 演示操作指南

## 1. 启动前

1. 打开 `D:\MachineVision\05_projects\SmartInspect\02_Source\SmartInspect.sln`。
2. 停止旧调试实例后生成解决方案。目标是0错误0警告。
3. 如果VS询问外部修改，请先保护你未保存的编辑，再重新加载磁盘版本，不要用旧编辑器覆盖工程配置。
4. 只允许Form1.cs使用设计器；其余Form1.*.cs只查看代码。
5. 不删除bin目录，它可能含正式数据库和配置。展示前建议备份 `SmartInspect.WinForms\bin\Debug\net8.0-windows\smartinspect.db` 与 `appsettings.json`。

## 2. 启动模拟器和HMI

先启动SmartInspect.PlcSimulator，再启动SmartInspect.WinForms。可在VS配置这两个为多启动项目，也可以分别在两个终端执行：

```powershell
dotnet run --project .\SmartInspect.PlcSimulator
dotnet run --project .\SmartInspect.WinForms
```

模拟器监听127.0.0.1:5020，从站号1。端口被占用通常是旧模拟器还在运行，不要重复开第二个。它是Modbus服务，不需要输入START/READY文本。

## 3. 单图手动演示（不需要PLC连接）

1. 最小值输入29.5，最大值输入30.5，点“应用参数”。
2. “选择图片” → `VisionImages\Calibration\calibration_part.png`。
3. “手动检测” → 尺寸约29.90mm、OK、预览绿色框和OK、当前表格新增一行。
4. 选择 `VisionImages\Basic\02_rect_small.png`，再手动检测 → 约21.90mm、NG，红色框。
5. 表格Measurement均显示两位小数。InspectTime在最后一列。统计按实际记录变化。
6. “查看历史”显示SQLite历史；选择批次筛选；“当前批次”返回当前视图。

手动检测也会真实写入当前程序运行目录的数据库。演示不是只预览，记录会保留。“清空历史”会删除正式历史，不是演示必需步骤，请勿随意点击确认。

## 4. PLC自动闭环

1. IP填127.0.0.1、端口5020，点击“连接PLC”。
2. 观察心跳数字变化、状态WaitingTrigger。
3. 选择标准件图，点击“模拟触发”。这个按钮只写触发寄存器；轮询收到后才执行视觉/数据库完整链路。
4. 观察 `Result=1, Done=1`、新增一条记录和OK标注。
5. 不再点击，等待数秒：Trigger保持1也不应继续增加记录。
6. 换成小件，点一次“模拟触发”：按钮模拟PLC先清零再置1，产生新的请求，应得到Result=2。
7. 点击“断开PLC”：尽力写Busy=0/Result=0/Done=0后断开。

外部PLC程序应在消费Done后将Trigger清零，再发下一个上升沿。PC不主动循环清除高电平请求。重连时已有高电平被忽略，以免旧请求重复保存。

## 5. 文件夹模拟相机

1. 选择文件夹 → `VisionImages\Basic`。
2. 当前模式是文件夹顺序取图；不会自动连续采集。
3. 每点一次“手动检测”或“模拟触发”，只消费下一张图：01正常件通常OK，02小件NG，03大件NG。
4. 后续可展示旋转、低对比度、噪声和多目标。多目标仍由最左侧第1目标决定整件结果，其余框仅标注。
5. 第8张后再触发会报警“图像源耗尽”，不新增产品。重新选择文件夹可从第一张开始。
6. “选择图片”切换回单图模式。模式/目录会保存，下次启动文件夹序列从头开始。

## 6. 故障演示与恢复

- 断线：检测空闲时关闭模拟器，HMI在轮询中报告通信异常并进入Error，不应崩溃。重启模拟器后点“连接PLC”，再产生新触发。
- 无轮廓/坏图：选择白图或无效图触发，不应写产品记录；报警保留，选择正常图片后可再次检测。
- 图像源耗尽：重新选择目录。
- 参数非法：不能将NaN/Infinity或最小值≥最大值作为有效参数。
- SQLite写入失败：保留报警、不增加产品计数；修复数据库权限/磁盘后重新触发，不删除旧数据库。自动验证会在独立临时库注入此故障，不需要你破坏正式库。
- “复位报警”清除最近报警；与PLC连接时先确认安全寄存器复位。修复原因是前提，不是盲目反复复位。

远端已断网时无法保证Busy真正清零，必须由真实PLC看门狗处理。当前模拟器未实现设备安全动作。

## 7. 查日志和配置

默认VS Debug运行目录：

`D:\MachineVision\05_projects\SmartInspect\02_Source\SmartInspect.WinForms\bin\Debug\net8.0-windows`

这里有：`smartinspect.db`、`appsettings.json`、`Logs\SmartInspect_yyyy-MM-dd.log`。Release或独立测试的实际目录不同，依据AppContext.BaseDirectory。

日志中搜索：Trigger、Measurement、SQLite、PLC Result、ERROR、异常恢复。新的配置字段缺省也能启动；修改PixelsPerMillimeter后重启生效。默认10px/mm。

## 8. 可复现自动验收

```powershell
powershell -ExecutionPolicy Bypass -File .\Verification\Run.ps1
```

终端应输出所有PASS和`FINAL failures=0`。结尾路径中的hmi-result.png是成功检测的界面渲染，hmi-verification.png是验收结束快照；Logs与数据库都在该临时目录。每次使用新目录，不复用有数据的测试库。

## 9. 答辩时明确的边界

真实实现：OpenCV计算、SQLite读写、Modbus网络通信、异步HMI、状态/日志/异常恢复。

模拟部分：图像采集使用磁盘文件，PLC是本机寄存器服务，不是接了真实工业相机与机械设备。模板匹配有算法接口及测试，不参与当前尺寸判定主链。像素边界测量存在约1像素差异；真实标定和安全互锁需要硬件阶段验收。
