using SmartInspect.Communication;
using SmartInspect.Core;

ModbusTcpService modbus =
    new ModbusTcpService();

InspectionProcess process =
    new InspectionProcess();

const byte slaveId = 1;

process.MinMeasurement = 9.5;
process.MaxMeasurement = 10.5;

try
{
    Console.WriteLine(
        "正在连接 Modbus PLC...");

    modbus.Connect(
        "127.0.0.1",
        5020);

    Console.WriteLine("连接成功");
    Console.WriteLine();

    // 测试阶段：
    // 临时模拟 PLC 发出检测触发
    modbus.WriteRegister(
        slaveId,
        PlcRegisterMap.Trigger,
        1);

    Console.WriteLine(
        "模拟 PLC：Trigger = 1");

    ushort trigger =
        modbus.ReadRegister(
            slaveId,
            PlcRegisterMap.Trigger);

    Console.WriteLine(
        "SmartInspect 读取 Trigger：" +
        trigger);

    if (trigger == 1)
    {
        Console.WriteLine();
        Console.WriteLine(
            "收到检测触发");

        // 清除上一轮状态
        modbus.WriteRegister(
            slaveId,
            PlcRegisterMap.Done,
            0);

        modbus.WriteRegister(
            slaveId,
            PlcRegisterMap.Result,
            PlcResultCode.None);

        // 告诉 PLC：PC 开始检测
        modbus.WriteRegister(
            slaveId,
            PlcRegisterMap.Busy,
            1);

        Console.WriteLine(
            "Busy = 1");

        // -------------------------
        // 真正使用 SmartInspect.Core
        // -------------------------

        Product product =
            new Product();

        product.BatchNumber =
            "BATCH001";

        product.SerialNumber =
            "SN1";

        // 暂时模拟视觉/传感器测量值
        product.Measurement =
            11;

        process.InspectProduct(
            product);

        Console.WriteLine(
            "Measurement = " +
            product.Measurement);

        Console.WriteLine(
            "检测结果 = " +
            product.Result);

        // 把 C# 的 OK / NG
        // 转换成 PLC 能理解的数字
        ushort plcResult;

        if (product.Result == "OK")
        {
            plcResult =
                PlcResultCode.Ok;
        }
        else
        {
            plcResult =
                PlcResultCode.Ng;
        }

        modbus.WriteRegister(
            slaveId,
            PlcRegisterMap.Result,
            plcResult);

        // 检测完成
        modbus.WriteRegister(
            slaveId,
            PlcRegisterMap.Busy,
            0);

        modbus.WriteRegister(
            slaveId,
            PlcRegisterMap.Done,
            1);

        Console.WriteLine();
        Console.WriteLine(
            "写回 PLC：");

        Console.WriteLine(
            "Result = " +
            plcResult);

        Console.WriteLine(
            "Busy = 0");

        Console.WriteLine(
            "Done = 1");

        // 读取回来确认
        ushort finalBusy =
            modbus.ReadRegister(
                slaveId,
                PlcRegisterMap.Busy);

        ushort finalResult =
            modbus.ReadRegister(
                slaveId,
                PlcRegisterMap.Result);

        ushort finalDone =
            modbus.ReadRegister(
                slaveId,
                PlcRegisterMap.Done);

        Console.WriteLine();
        Console.WriteLine(
            "最终 PLC 状态：");

        Console.WriteLine(
            "Busy = " +
            finalBusy);

        Console.WriteLine(
            "Result = " +
            finalResult);

        Console.WriteLine(
            "Done = " +
            finalDone);
    }
}
catch (Exception ex)
{
    Console.WriteLine(
        "Modbus 通信失败：" +
        ex.Message);
}
finally
{
    modbus.Disconnect();

    Console.WriteLine();
    Console.WriteLine(
        "Modbus 已断开");
}

Console.WriteLine();
Console.WriteLine(
    "按 Enter 退出");

Console.ReadLine();