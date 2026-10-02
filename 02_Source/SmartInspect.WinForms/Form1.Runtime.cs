using SmartInspect.Core;
using SmartInspect.Data;
using SmartInspect.Communication;
using SmartInspect.Vision;

namespace SmartInspect.WinForms;

public partial class Form1
{
    private readonly InspectionStateMachine stateMachine = new();
    private readonly LogService logger = new();
    private readonly Label lblMachineState = new() { AutoSize = true };
    private readonly Label lblAlarm = new() { AutoSize = true, MaximumSize = new Size(800, 0) };
    private readonly Button btnSelectFolder = new() { Text = "选择文件夹", AutoSize = true };
    private readonly Button btnResetAlarm = new() { Text = "复位报警", AutoSize = true };
    private AlarmInfo? currentAlarm;
    private IImageSource? imageSource;
    private string? imageSourceError;
    private Task<Product?>? activeInspection;

    private void InitializeRuntime()
    {
        Text = "SmartInspect 工业视觉检测（文件模拟采集）";
        ClientSize = new Size(Math.Max(ClientSize.Width, 1060), Math.Max(ClientSize.Height, 755));
        AutoScroll = true;
        lblResult.MaximumSize = new Size(1020, 35);
        txtImagePath.SetBounds(12, 238, 1020, 23);
        btnSelectImage.SetBounds(12, 270, 100, 26);
        btnSelectFolder.Location = new Point(125, 270);
        dgvProducts.SetBounds(12, 320, 580, 260);
        picVisionPreview.SetBounds(620, 320, 420, 260);
        lblBatchFilter.Location = new Point(620, 285);
        cmbBatchFilter.Location = new Point(710, 282);
        lblTotal.Location = new Point(12, 595);
        lblOkCount.Location = new Point(145, 595);
        lblNgCount.Location = new Point(255, 595);
        lblYield.Location = new Point(365, 595);
        btnClearBatch.Location = new Point(12, 625);
        btnShowCurrentBatch.Location = new Point(120, 625);
        btnShowHistory.Location = new Point(228, 625);
        btnClearHistory.Location = new Point(336, 625);
        lblMachineState.Location = new Point(12, 665);
        lblAlarm.Location = new Point(12, 705);
        lblAlarm.MaximumSize = new Size(1020, 0);
        btnResetAlarm.Location = new Point(860, 665);
        Controls.AddRange([lblMachineState, lblAlarm, btnSelectFolder, btnResetAlarm]);
        btnSelectFolder.Click += (_, _) => SelectImageFolder();
        btnResetAlarm.Click += async (_, _) =>
        {
            if (stateMachine.IsRunning || plcCycleRunning) return;
            triggerArmed = false;
            if (modbusTcpService.IsConnected && !await ResetPlcAfterErrorAsync()) return;
            currentAlarm = null;
            lblAlarm.Text = "报警：无";
            stateMachine.MoveTo(modbusTcpService.IsConnected ? InspectionState.WaitingTrigger : InspectionState.Idle);
            logger.Info("操作员复位报警，等待新的触发边沿");
        };
        stateMachine.Changed += state =>
        {
            lblMachineState.Text = "检测状态：" + (state switch
            {
                InspectionState.Idle => "待机",
                InspectionState.WaitingTrigger => "等待触发",
                InspectionState.AcquiringImage => "获取图像",
                InspectionState.Inspecting => "视觉检测",
                InspectionState.Saving => "保存记录",
                InspectionState.Reporting => "回传结果",
                InspectionState.Completed => "检测完成",
                _ => "异常"
            });
            logger.Info("状态 → " + state);
        };
        lblMachineState.Text = "检测状态：待机";
        lblAlarm.Text = "报警：无";
        txtMeasurement.ReadOnly = true;
        picVisionPreview.SizeMode = PictureBoxSizeMode.Zoom;
        txtImagePath.ReadOnly = true;
        logger.Info("程序启动");
    }

    private void RaiseAlarm(string code, string message, AlarmLevel level = AlarmLevel.Error)
    {
        currentAlarm = new AlarmInfo(code, level, message, DateTime.Now);
        lblAlarm.Text = $"最近报警 [{code}]：{message}";
        lblAlarm.ForeColor = level == AlarmLevel.Error ? Color.Red : Color.DarkOrange;
        if (level == AlarmLevel.Error) logger.Error($"{code}: {message}");
        else logger.Warning($"{code}: {message}");
        lblResult.Text = message;
        lblResult.ForeColor = level == AlarmLevel.Error ? Color.Red : Color.DarkOrange;
    }

    private void SaveConfiguration()
    {
        try { configService.SaveConfig(appConfig); }
        catch (Exception ex) { RaiseAlarm("CONFIG", "配置保存失败：" + ex.Message); }
    }

    private void SetInspectionControls(bool enabled)
    {
        foreach (Control control in new Control[] { btnInspect, btnApplyParameters, btnSelectImage, btnSelectFolder, btnClearBatch, btnClearHistory, btnShowHistory, btnShowCurrentBatch, cmbBatchFilter, btnDisconnectPlc, btnConnectPlc, btnResetAlarm })
            control.Enabled = enabled;
        cmbBatchFilter.Enabled = enabled && isHistoryView;
    }

    private Task<Product?> ExecuteInspectionAsync(bool reportToPlc)
    {
        if (activeInspection is { IsCompleted: false }) return Task.FromResult<Product?>(null);
        activeInspection = RunInspectionAsync(reportToPlc);
        return activeInspection;
    }

    private async Task<Product?> RunInspectionAsync(bool reportToPlc)
    {
        if (!reportToPlc && plcCycleRunning) return null;
        if (!stateMachine.TryStart()) return null;
        SetInspectionControls(false);
        Product? product = null;
        try
        {
            logger.Info(reportToPlc ? "PLC Trigger：检测开始" : "手动检测开始");
            // Do not present a previous successful result while the new cycle is failing.
            var oldPreview = picVisionPreview.Image;
            picVisionPreview.Image = null;
            oldPreview?.Dispose();
            if (reportToPlc) await Task.Run(() => modbusTcpService.WriteInspectionState(1, 0, 0));
            if (imageSourceError != null) throw new IOException(imageSourceError);
            string path = await Task.Run(() => (imageSource ?? new SingleImageSource(currentVisionImagePath)).GetNextImagePath());
            currentVisionImagePath = path;
            txtImagePath.Text = path;
            logger.Info("当前图片：" + path);
            stateMachine.MoveTo(InspectionState.Inspecting);
            var result = await Task.Run(() => visionService.InspectImage(path, appConfig.PixelsPerMillimeter));
            if (!result.Success || result.Objects.Count == 0) throw new InvalidOperationException(result.Message);
            product = new Product { BatchNumber = $"BATCH{currentBatchNumber:D3}", SerialNumber = $"SN{products.Count + 1}", Measurement = result.Objects[0].WidthMillimeters };
            process.InspectProduct(product);
            byte[] annotated = await Task.Run(() => new VisionAnnotationService().CreateAnnotatedImage(path, result, product.Result));
            using (var stream = new MemoryStream(annotated))
            using (var image = Image.FromStream(stream))
            {
                var previous = picVisionPreview.Image;
                picVisionPreview.Image = new Bitmap(image);
                previous?.Dispose();
            }
            logger.Info($"Measurement={product.Measurement:F4}, Result={product.Result}");
            stateMachine.MoveTo(InspectionState.Saving);
            await Task.Run(() => databaseService.SaveProduct(product));
            logger.Info($"SQLite 保存成功 {product.BatchNumber}/{product.SerialNumber}");
            products.Add(product);
            allProducts.Add(product);
            isHistoryView = false;
            RefreshProductGrid();
            RefreshStatistics(products);
            txtMeasurement.Text = product.Measurement.ToString("F2");
            stateMachine.MoveTo(InspectionState.Reporting);
            if (reportToPlc)
            {
                ushort value = product.Result == "OK" ? PlcResultCode.Ok : PlcResultCode.Ng;
                await Task.Run(() => modbusTcpService.WriteInspectionState(0, value, 1));
                logger.Info($"PLC Result={value}, Busy=0, Done=1");
                lblPlcResponse.Text = $"PLC：Result={value}, Done=1，等待 Trigger 归零";
            }
            stateMachine.MoveTo(InspectionState.Completed);
            lblResult.Text = $"{product.SerialNumber} 尺寸 {product.Measurement:F2} mm，{product.Result}（第1目标）";
            lblResult.ForeColor = product.Result == "OK" ? Color.Green : Color.Red;
            stateMachine.MoveTo(modbusTcpService.IsConnected ? InspectionState.WaitingTrigger : InspectionState.Idle);
            return product;
        }
        catch (Exception ex)
        {
            string code = stateMachine.State switch
            {
                InspectionState.AcquiringImage => ex is EndOfStreamException ? "SOURCE_EXHAUSTED" : "ACQUIRE",
                InspectionState.Inspecting => "VISION",
                InspectionState.Saving => "DATABASE",
                InspectionState.Reporting => "PLC_REPORT",
                _ => "INSPECTION"
            };
            stateMachine.MoveTo(InspectionState.Error);
            RaiseAlarm(code, ex.Message);
            if (!reportToPlc || await ResetPlcAfterErrorAsync())
            {
                stateMachine.MoveTo(modbusTcpService.IsConnected ? InspectionState.WaitingTrigger : InspectionState.Idle);
                logger.Info("异常恢复完成；本次不自动重试，等待新触发");
            }
            return null;
        }
        finally { SetInspectionControls(true); }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (activeInspection is { IsCompleted: false } || stateMachine.IsRunning || plcCycleRunning)
        {
            e.Cancel = true;
            RaiseAlarm("BUSY", "检测或通信处理中，请稍后关闭窗口。", AlarmLevel.Warning);
        }
        else
        {
            plcPollTimer.Stop();
            modbusTcpService.Disconnect();
            plcPollTimer.Dispose();
            picVisionPreview.Image?.Dispose();
            logger.Info("程序退出");
        }
        base.OnFormClosing(e);
    }
}
