# SmartInspect 工业视觉检测学习作品

这是一个 C# / .NET 8 Windows 工业软件演示项目：WinForms HMI、OpenCvSharp 视觉测量、SQLite 历史记录与 Modbus TCP 握手组成完整检测闭环。当前使用图片或文件夹模拟相机、使用本机 Modbus 从站模拟 PLC，不宣称已完成真实相机或生产设备验收。

运行步骤见 [DEMO_GUIDE.md](DEMO_GUIDE.md)，调用关系见 [ARCHITECTURE.md](ARCHITECTURE.md)。解决方案仍包含原有八个项目，Verification 是不加入解决方案的独立验收工具。

## 项目职责

| 项目 | 职责 |
| --- | --- |
| SmartInspect.Core | Product、上下限 OK/NG 判定、统计、配置模型、检测状态机、报警模型 |
| SmartInspect.Data | SQLite 初始化/保存/历史、JSON 配置、按天文件日志 |
| SmartInspect.Vision | 图像读取、轮廓测量、模板匹配、结果图标注、单图/文件夹图像源 |
| SmartInspect.Communication | 原 TCP 工具、带超时及互斥访问的 Modbus TCP、寄存器地址表 |
| SmartInspect.WinForms | 操作界面、异步检测调度、报警显示、PLC 轮询和结果回传 |
| SmartInspect.PlcSimulator | 127.0.0.1:5020、从站号 1 的 Modbus TCP 寄存器模拟器 |
| SmartInspect.Console | 保留现有视觉命令行学习入口 |
| SmartInspect.TcpTest | 保留现有通信学习测试入口 |

## 检测闭环

手动检测或新的 PLC Trigger → IImageSource 取一张 → Vision 检测 → 第1目标宽度(mm) → Product → InspectionProcess → SQLite 保存一次 → PLC Result/Done → 等待下一次触发。

多目标按中心 X 排序。为兼容原业务，只有第1目标决定当前产品结果；其余目标绘制黄色框和尺寸，不暗示它们参与整件 OK/NG 判定。NG 是正常检测结果，不是设备异常。

## Modbus 寄存器（0 起始协议地址）

| 地址 | 名称 | 含义/写入方 |
| --- | --- | --- |
| 0 | Trigger | PLC/模拟触发按钮：0→1 发起检测 |
| 1 | Busy | PC：1 表示处理，0 表示结束或复位 |
| 2 | Result | PC：0 无有效结果；1 OK；2 NG |
| 3 | Done | PC：1 结果已保存并完成回传；0 未完成或异常 |
| 4 | Heartbeat | PC 每约1秒递增并回读，65535后回到1 |

从站号为1。Busy/Result/Done 使用一次 FC16 多寄存器写入；读取使用 FC03，单寄存器触发/心跳写入使用 FC06。

正常握手：Trigger=1 → Busy=1/Result=0/Done=0 → 采集/检测/保存 → Busy=0/Result=1或2/Done=1。PLC 消费 Done 后应清零 Trigger，PC 观察到0后重新布防，下一次上升沿才检测。PC 检测完成后**不自行清除外部 Trigger**，避免持续高电平重复入库。HMI“模拟触发”按钮扮演 PLC 生产者，先写0再写1；每点击一次代表一件新的产品。

重连先安全复位 Busy/Result/Done，但忽略连接时已有的 Trigger=1；必须看到0或使用模拟触发按钮生成新边沿。Modbus 连接/读写约3秒超时、无自动协议重试，所有操作串行保护，不阻塞 UI。心跳在后台检测期间继续轮询。

异常尽力写 Busy=0/Result=0/Done=0，不报告 OK。**断网时无法保证远端复位成功**；真实 PLC 必须实现独立心跳超时保护。当前模拟器是被动寄存器服务，没有安全 PLC 看门狗，不可用于真实运动控制。Heartbeat 回读证明链路响应，不等同于真实 PLC 程序健康认证。

## Vision 与标定

算法保留：BGR→Gray → MedianBlur(5) → BinaryInv+Otsu → FindContours(External) → 过滤面积小于1000的轮廓 → MinAreaRect → 长边作为宽度、短边作为高度 → 像素÷PixelsPerMillimeter。

默认10 px/mm。测试矩形栅格占300×160像素，但轮廓坐标跨度通常测得约299×159，所以常见输出29.90×15.90mm；这不是毫米换算错误。演示阈值29.5～30.5可覆盖标准件。真实系统需标定镜头畸变、光照和边缘误差，不能把合成素材结果作为计量精度证明。

模板匹配保留 MatchTemplate(CCoeffNormed)，最低分数默认0.8，返回最佳位置和分数。标准素材得分约1、位置(760,400)。这是独立算法接口/验收项，**当前主检测链按尺寸判断，没有额外增加模板匹配否决规则**。TemplateMinimumScore 是配置默认值，可传入 MatchTemplate，并未增加模板选择 HMI。

结果标注在 VisionAnnotationService 内生成 PNG 字节：旋转框、尺寸、第1目标 Result。Mat 均释放，WinForms 从字节克隆 Bitmap 后释放临时图像；替换预览时释放旧图，不长期锁定原文件。PictureBox 使用 Zoom。

## 图像源

- IImageSource.GetNextImagePath()：当前为文件路径型采集边界。
- SingleImageSource：每次读取选择的同一图片。
- FolderImageSource：目录第一层 png/jpg/jpeg/bmp，按文件名不区分大小写排序；每次触发一张，默认末尾停止并报警，重新选择目录从头开始。可在类构造中显式开启循环，HMI不启用循环。
- 真实相机适配器将采集帧保存为临时文件后返回路径即可复用主流程；若未来追求吞吐量，可以另行演进为内存帧接口，当前不安装任何厂商 SDK。

## 状态、报警和日志

Idle/WaitingTrigger → AcquiringImage → Inspecting → Saving → Reporting → Completed → WaitingTrigger（连接PLC）或Idle（单机）。非法转换会被拒绝，运行阶段拒绝第二次检测。传输轮询的防重入标志不承担检测状态职责。

异常 → Error → 安全复位成功后回到等待/单机状态；复位无法确认则保持Error，需修复链路并重连。报警保留在“最近报警”区域，操作员可复位。图片不存在、解码失败、无轮廓、图像源耗尽、数据库失败、参数错误、通信失败与未知检测异常都有可读提示和日志。不会因检测异常弹出无限重复模态对话框。

Logs位于实际运行目录，以 `SmartInspect_yyyy-MM-dd.log` 按天写入，包含时间、INFO/WARNING/ERROR、启动、参数、图像、状态、尺寸、保存、PLC结果、异常与恢复。日志失败记录在 LogService.LastWriteError，不让日志故障使主程序崩溃。

## SQLite 与 JSON

运行目录内的 smartinspect.db，InspectionRecords 表保持既有结构：Id、BatchNumber、SerialNumber、Measurement、Result、InspectTime。无DROP表、无结构迁移。本次补齐模型和读取路径中的InspectTime；保存使用参数化SQL。保存失败不增加内存计数。已保存但回传失败的记录保留，不自动重做检测，避免重复。

appsettings.json 同样位于实际运行目录。保留 MinMeasurement/MaxMeasurement，增加默认：PixelsPerMillimeter=10、TemplateMinimumScore=0.8、LastImagePath=""、ImageSourceMode="Single"、ImageFolder=""。旧JSON缺少新字段仍能读取。选择图像/目录或应用参数时保存，启动不主动覆盖旧配置。手动修改配置前关闭程序并备份文件。

## 自动验收

在解决方案目录执行 `powershell -ExecutionPolicy Bypass -File .\Verification\Run.ps1`。脚本创建全新的临时输出，启动真实Form1并驱动其业务方法，使用临时SQLite与本机临时端口Modbus服务，不操作正式数据库，不需真实PLC。

覆盖视觉素材、模板正反例、F2/历史、状态路径、数据库失败、持续高触发、后台心跳、断开/不回复、重新连接、文件夹两次触发、报警日志。输出PASS及最终failures=0，另有HMI渲染图及隔离日志。测试不是对真实硬件或所有DPI/屏幕分辨率的验收。

## 设计器保护与范围

仅Form1.cs为Form；Form1.Communication.cs、Form1.Vision.cs、Form1.Runtime.cs为Code。最终元数据在SDK导入之后设置，辅助resx排除构建。只用主Form1打开设计器，辅助文件用F7查看代码。运行时补充状态、报警、目录选择并局部整理表格/预览，不改主Designer资源。

当前是可演示学习作品，不是安全认证生产软件：不提供跨进程崩溃/断电的端到端Exactly-Once保证、物理相机控制、PLC安全互锁、数据库归档或日志自动清理。长期使用需另行规划容量和备份。未创建Git commit。
