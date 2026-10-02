# SmartInspect 最终自动验收记录

日期：2026-09-18。本报告对应本次实现及最后一轮完整运行，不是仅静态检查。

## 构建

- dotnet restore：成功。
- SmartInspect.WinForms Rebuild：0 errors / 0 warnings。
- SmartInspect.sln Rebuild：0 errors / 0 warnings。
- 独立Verification构建：0 errors / 0 warnings。
- Verification最终运行：38项PASS，FINAL failures=0。

## 测试覆盖

| 场景 | 结果 |
| --- | --- |
| 旧JSON仅Min/Max | 标定比例10、模板阈值0.8默认值生效 |
| 标准件+29.5～30.5 | OK，约29.90mm |
| 小件+29.5～30.5 | NG，21.90mm |
| 旋转、低对比度、噪声 | 均成功找到目标 |
| 多目标 | 3个目标 |
| 正确模板 | Score=1.0000，(760,400) |
| 错误模板（圆形场景） | Score=0.4680，小于0.8，拒绝 |
| NaN标定/参数 | 拒绝 |
| 状态链 | 含Saving、Completed并返回等待/待机 |
| 当前、全部历史、筛选历史 | F2正常 |
| SQLite时间 | 保存/读取InspectTime正常 |
| 缺图、白图无轮廓、坏文件 | 无崩溃、无新增产品、报警 |
| 文件夹 | 排序正确，耗尽报警 |
| SQLite强制插入失败 | 无新增产品计数，DATABASE报警 |
| PLC OK | Result=1、Done=1 |
| PLC NG | Busy=0、Result=2、Done=1 |
| 持续Trigger=1 | 未重复检测或入库 |
| 后台慢采集 | 心跳继续变化 |
| 视觉失败 | Busy=0、Result=0、Done=0，无产品记录 |
| PLC连接断开 | Error、日志、断开失效传输，无HMI崩溃 |
| PLC接收连接但不回复 | 有界超时、HMI仍响应 |
| 重连时已有Trigger=1 | 不重做旧请求 |
| 文件夹两次PLC触发 | 第一张OK、第二张NG |
| 主动断开 | 远端Busy/Result/Done先归零 |
| 日志 | SQLite、PLC结果、ERROR均落盘 |
| 日志不可写 | LastWriteError记录故障，主程序不崩溃 |

使用真实Form1控件与业务方法、真实SQLite库及本机TCP上的NModbus从站。测试从站为隔离临时端口、与PlcSimulator采用相同服务库；未声称已连接物理PLC或工业相机。界面通过DrawToBitmap渲染检查，未对所有DPI和屏幕尺寸进行人工操作验收。

## 最后一轮证据位置

`C:\Users\admin\AppData\Local\Temp\SmartInspect-verification-1b9cf5383e94463dbe397cf9d71cdf17\bin\SmartInspect.Verification\debug`

含hmi-result.png、hmi-verification.png、独立smartinspect.db及Logs。临时目录可能被系统清理，可随时用Run.ps1重新生成。测试程序拒绝在已有数据库的输出目录再次运行，不删除或复用用户数据。

## 保护检查

- 正式smartinspect.db及appsettings.json与备份SHA-256一致。
- 主Form1.Designer.cs、Form1.resx、SmartInspect.sln、Console和PlcSimulator入口与修改前一致。
- 最终MSBuild：Form1.cs=Form，Communication/Vision/Runtime辅助partial=Code。
- EmbeddedResource仅Form1.resx，资源名SmartInspect.WinForms.Form1。
- 不存在Form1.Communication.resx、Form1.Vision.resx、Form1.Runtime.resx。
- 未删除运行数据，未创建Git commit。

## 必须保留的边界

断网后不能保证远端寄存器复位；真实PLC需要独立看门狗。保存成功但结果回传失败时记录保留，不自动重试。运行进程内防止同一持续高电平重复处理，不提供跨进程崩溃的端到端Exactly-Once保证。模板匹配为独立测试能力，当前主闭环仍按第1目标尺寸判定。
