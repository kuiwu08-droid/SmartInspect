using SmartInspect.Communication;
using SmartInspect.Core;

namespace SmartInspect.WinForms;

public partial class Form1
{
    private readonly ModbusTcpService modbusTcpService = new();
    private readonly System.Windows.Forms.Timer plcPollTimer = new();
    private const byte ModbusSlaveId = 1;
    private bool plcCycleRunning;
    private bool triggerArmed;
    private ushort heartbeatValue;
    private DateTime lastHeartbeatTime = DateTime.MinValue;

    private void BindCommunicationEvents()
    {
        btnConnectPlc.Click -= btnConnectPlc_Click;
        btnConnectPlc.Click += btnConnectPlc_Click;
        btnDisconnectPlc.Click -= btnDisconnectPlc_Click;
        btnDisconnectPlc.Click += btnDisconnectPlc_Click;
        btnSendTest.Click -= btnSendTest_Click;
        btnSendTest.Click += btnSendTest_Click;
        plcPollTimer.Interval = 300;
        plcPollTimer.Tick -= PlcPollTimer_Tick;
        plcPollTimer.Tick += PlcPollTimer_Tick;
    }

    private async void btnConnectPlc_Click(object? sender, EventArgs e)
    {
        if (stateMachine.IsRunning || plcCycleRunning) return;
        if (!int.TryParse(txtPlcPort.Text, out int port) || port < 1 || port > 65535)
        {
            RaiseAlarm("PARAMETER", "PLC 端口必须在 1～65535 之间。", AlarmLevel.Warning);
            return;
        }
        plcPollTimer.Stop();
        SetInspectionControls(false);
        plcCycleRunning = true;
        try
        {
            string host = txtPlcIp.Text.Trim();
            await Task.Run(() => { modbusTcpService.Connect(host, port); modbusTcpService.WriteInspectionState(0, 0, 0); });
            triggerArmed = false;
            heartbeatValue = 0;
            lastHeartbeatTime = DateTime.MinValue;
            stateMachine.MoveTo(InspectionState.WaitingTrigger);
            lblPlcStatus.Text = "PLC：已连接，等待 Trigger=0 后重新触发";
            logger.Info($"PLC connected {host}:{port}");
            plcPollTimer.Start();
        }
        catch (Exception ex)
        {
            await Task.Run(modbusTcpService.Disconnect);
            stateMachine.MoveTo(InspectionState.Error);
            RaiseAlarm("PLC_CONNECT", ex.Message);
        }
        finally { plcCycleRunning = false; SetInspectionControls(true); }
    }

    private async void btnDisconnectPlc_Click(object? sender, EventArgs e)
    {
        if (stateMachine.IsRunning || plcCycleRunning) return;
        plcPollTimer.Stop();
        plcCycleRunning = true;
        SetInspectionControls(false);
        await ResetPlcAfterErrorAsync();
        await Task.Run(modbusTcpService.Disconnect);
        triggerArmed = false;
        stateMachine.MoveTo(InspectionState.Idle);
        lblPlcStatus.Text = "PLC：未连接";
        logger.Info("PLC disconnected");
        plcCycleRunning = false;
        SetInspectionControls(true);
    }

    private async void btnSendTest_Click(object? sender, EventArgs e)
    {
        if (stateMachine.IsRunning || plcCycleRunning) return;
        if (!modbusTcpService.IsConnected) { RaiseAlarm("PLC_OFFLINE", "请先连接 PLC。", AlarmLevel.Warning); return; }
        btnSendTest.Enabled = false;
        plcCycleRunning = true;
        try
        {
            // Simulated producer resets the previous trigger before raising a new one.
            await Task.Run(() => modbusTcpService.WriteRegister(ModbusSlaveId, PlcRegisterMap.Trigger, 0));
            triggerArmed = true;
            await Task.Run(() => modbusTcpService.WriteRegister(ModbusSlaveId, PlcRegisterMap.Trigger, 1));
            logger.Info("模拟 PLC 发出新的 Trigger 边沿");
        }
        catch (Exception ex) { stateMachine.MoveTo(InspectionState.Error); RaiseAlarm("PLC_TRIGGER", ex.Message); }
        finally { plcCycleRunning = false; btnSendTest.Enabled = true; }
    }

    private async void PlcPollTimer_Tick(object? sender, EventArgs e)
    {
        if (plcCycleRunning || !modbusTcpService.IsConnected) return;
        plcCycleRunning = true;
        bool startInspection = false;
        try
        {
            if ((DateTime.UtcNow - lastHeartbeatTime).TotalSeconds >= 1)
            {
                await CheckHeartbeatAsync();
                lastHeartbeatTime = DateTime.UtcNow;
            }
            ushort trigger = await Task.Run(() => modbusTcpService.ReadRegister(ModbusSlaveId, PlcRegisterMap.Trigger));
            if (trigger == 0) triggerArmed = true;
            if (trigger == 1 && triggerArmed && !stateMachine.IsRunning && stateMachine.State != InspectionState.Error)
            {
                triggerArmed = false;
                startInspection = true;
            }
        }
        catch (Exception ex)
        {
            plcPollTimer.Stop();
            triggerArmed = false;
            if (!stateMachine.IsRunning) stateMachine.MoveTo(InspectionState.Error);
            RaiseAlarm("PLC_COMMUNICATION", ex.Message);
            await ResetPlcAfterErrorAsync();
            await Task.Run(modbusTcpService.Disconnect);
            lblPlcStatus.Text = "PLC：通信异常，请重新连接";
        }
        finally { plcCycleRunning = false; }
        if (startInspection) await ExecuteInspectionAsync(true);
    }

    private async Task CheckHeartbeatAsync()
    {
        heartbeatValue = heartbeatValue == ushort.MaxValue ? (ushort)1 : (ushort)(heartbeatValue + 1);
        ushort value = heartbeatValue;
        ushort received = await Task.Run(() =>
        {
            modbusTcpService.WriteRegister(ModbusSlaveId, PlcRegisterMap.Heartbeat, value);
            return modbusTcpService.ReadRegister(ModbusSlaveId, PlcRegisterMap.Heartbeat);
        });
        if (received != value) throw new IOException("Heartbeat 回读不一致。");
        lblPlcStatus.Text = $"PLC：已连接 / Heartbeat={received}";
    }

    private async Task<bool> ResetPlcAfterErrorAsync()
    {
        try
        {
            if (!modbusTcpService.IsConnected) throw new IOException("PLC 已断开，无法确认远端安全状态。");
            await Task.Run(() => modbusTcpService.WriteInspectionState(0, 0, 0));
            logger.Warning("PLC 安全复位 Busy=0, Result=0, Done=0；不重试已保存产品");
            return true;
        }
        catch (Exception ex)
        {
            logger.Error("PLC 复位失败：" + ex.Message);
            RaiseAlarm("PLC_RESET", "远端复位未确认，需要 PLC 心跳超时保护并重新连接：" + ex.Message);
            return false;
        }
    }
}
