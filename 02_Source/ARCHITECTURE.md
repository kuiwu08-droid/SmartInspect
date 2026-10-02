# SmartInspect 架构与关键约束

```text
真实PLC（未来） / PlcSimulator（当前）
                  │ Modbus TCP Trigger / Heartbeat
                  ▼
       SmartInspect.Communication
       超时 + 串行传输 + 寄存器地址表
                  ▼
       WinForms / InspectionStateMachine
                  │ 接受一个新触发、禁用冲突操作
                  ▼
       IImageSource.GetNextImagePath()
       ├─ SingleImageSource
       └─ FolderImageSource（排序、末尾停止）
                  ▼
       VisionService + VisionAnnotationService
       轮廓/旋转矩形/毫米尺寸/PNG结果图
                  ▼
       Core.Product + InspectionProcess
       第1目标宽度 → OK / NG
                  ▼
       Data.DatabaseService → SQLite
                  │ 保存成功才计数及发布完成
                  ▼
       Communication → PLC Result / Busy / Done

各阶段 → LogService（按天日志）
异常   → AlarmInfo + Error状态 + HMI最近报警 + 尽力安全复位
```

## 线程与所有权

Form1.Runtime.cs统一调度，状态机只在UI线程修改，视觉、采集、SQLite保存、Modbus操作使用Task.Run。UI await后恢复到UI线程更新控件；没有Thread.Sleep阻塞UI。Modbus服务自身使用锁保护master，多次心跳/轮询/回传不会并发损坏传输。Timer每300ms轮询，心跳约1秒；轮询有非重入标志，检测状态由状态机控制，不用该标志表示整个检测周期。

图像源状态只由一次检测消费，运行中禁用源选择与批次/参数更改。检测不重入，SQLite保存只有统一入口的一次调用。故障不自动重试，尤其保存完成但PLC回传失败时保留已保存记录。当前保证运行进程内对保持高电平的触发只处理一次，不承诺进程崩溃后的分布式Exactly-Once；若需要，应引入PLC请求ID和数据库唯一约束作为单独需求。

## 状态图

```text
Idle ──连接──> WaitingTrigger
  └──手动───────┐
WaitingTrigger ─┴─> AcquiringImage → Inspecting → Saving → Reporting
                                                           │
                         Idle / WaitingTrigger <── Completed

任一阶段异常 → Error
  ├─ 本地异常或远端复位成功 → Idle / WaitingTrigger（报警仍可见）
  └─ 远端安全状态未确认 → 保持Error，修复链路、重新连接/复位
```

## PLC协议约定

0=Trigger；1=Busy；2=Result；3=Done；4=Heartbeat，全部是0起始Holding Register地址，Slave ID=1。开始/完成状态用FC16写三个连续寄存器。

- 触发生产者拥有Trigger。接收端看到低才重新布防，高只消费一次。
- 开始：Busy=1、Result=0、Done=0。
- 完成：Busy=0、Result=1(OK)/2(NG)、Done=1。
- 错误：尽力写Busy=0、Result=0、Done=0。
- 网络断开时PC无法确认远端寄存器；真实PLC必须自行监视心跳超时并执行安全动作。
- HMI模拟触发按钮是生产者模拟工具，不是绕过状态机直接检测。

## 持久化与兼容

旧数据库已有InspectTime列，无表结构迁移。Product增加InspectTime字符串以原样读取旧时间格式；新增保存使用本地时间yyyy-MM-dd HH:mm:ss。记录的是软件检测保存时刻，不是硬件曝光时间。旧JSON保留Min/Max，新增属性初始化默认值；不需用户手工补齐。

## 视觉策略与扩展边界

现有算法顺序和面积阈值保留。旋转矩形额外保存原始四角点，避免归一化角度与长短边交换导致标注框旋转错误。使用PNG字节跨越Vision/UI边界，UI不依赖Mat生命周期。模板匹配仍是独立方法，可由未来检测策略接入，但当前不隐式改变尺寸判定。

相机抽象暂时是文件路径接口：新增品牌适配器实现IImageSource，负责曝光/触发/帧释放并返回采集文件路径，HMI选择该实现即可复用检测。真实高速采集若需要零拷贝内存帧，再单独演进接口；本次没有绑定厂商SDK。

## 文件组织和设计器安全

- Form1.cs：保留批次、参数、表格、统计、历史事件。
- Form1.Vision.cs：选择图像/目录和预览。
- Form1.Communication.cs：PLC连接、轮询、心跳和触发锁存。
- Form1.Runtime.cs：统一异步检测、状态/报警/日志与运行时布局。
- Form1.Designer.cs / Form1.resx：唯一主窗体的原设计器与资源。

辅助partial文件在SDK及.user导入之后显式标为Code，并排除相应辅助resx。运行时新增控件不生成Designer资源。原八项目解决方案未改动；Verification工具单独构建，隔离运行数据。
