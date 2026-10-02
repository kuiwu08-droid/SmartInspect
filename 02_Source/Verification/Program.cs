using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.Data.Sqlite;
using NModbus;
using NModbus.Data;
using SmartInspect.Communication;
using SmartInspect.Core;
using SmartInspect.Data;
using SmartInspect.Vision;
using SmartInspect.WinForms;

internal static class Program
{
    private static string root = "";
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static int failures;

    [STAThread]
    private static int Main(string[] args)
    {
        root = args.Length == 1 ? Path.GetFullPath(args[0]) : throw new ArgumentException("Pass solution root");
        if (File.Exists(Path.Combine(AppContext.BaseDirectory, "smartinspect.db")))
            throw new InvalidOperationException("Use a fresh isolated --artifacts-path; existing database will not be touched.");
        ApplicationConfiguration.Initialize();
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), "{\"MinMeasurement\":29.5,\"MaxMeasurement\":30.5}");
        using var form = new Form1 { StartPosition = FormStartPosition.Manual, Location = new Point(-10000, -10000), ShowInTaskbar = false };
        form.Shown += async (_, _) =>
        {
            try { await Verify(form); }
            catch (Exception ex) { failures++; Console.WriteLine("FAIL: " + ex); }
            finally
            {
                Field<System.Windows.Forms.Timer>(form, "plcPollTimer").Stop();
                Field<ModbusTcpService>(form, "modbusTcpService").Disconnect();
                form.Close();
            }
        };
        Application.Run(form);
        Console.WriteLine($"FINAL failures={failures}");
        return failures == 0 ? 0 : 1;
    }

    private static async Task Verify(Form1 form)
    {
        var vision = new VisionService();
        var config = Field<AppConfig>(form, "appConfig");
        Assert(config.PixelsPerMillimeter == 10 && config.TemplateMinimumScore == 0.8, "old JSON defaults");
        foreach (string name in new[] { "04_rect_rotated.png", "05_low_contrast.png", "06_noise.png", "07_multiple_parts.png" })
        {
            var result = await Task.Run(() => vision.InspectImage(Image("Basic", name), 10));
            Assert(result.Success && (name != "07_multiple_parts.png" || result.Objects.Count == 3), name + $" objects={result.Objects.Count}");
        }
        var match = vision.MatchTemplate(Image("Template", "template_scene.png"), Image("Template", "template_part.png"), 0.8);
        Assert(match.Success && match.Score > 0.99 && match.MatchX == 760 && match.MatchY == 400, $"template score={match.Score:F4} at {match.MatchX},{match.MatchY}");
        var wrong = vision.MatchTemplate(Image("Basic", "08_circle.png"), Image("Template", "template_part.png"), 0.8);
        Assert(!wrong.Success && wrong.Score < 0.8, $"wrong template rejected score={wrong.Score:F4}");
        Assert(!vision.InspectImage(Image("Basic", "01_rect_normal.png"), double.NaN).Success, "invalid calibration rejected");
        Assert(!new InspectionProcess().TryApplyParameters(double.NaN, 30), "invalid threshold rejected");

        var states = new List<InspectionState>();
        Field<InspectionStateMachine>(form, "stateMachine").Changed += states.Add;
        Set(form, "imageSource", new SingleImageSource(Image("Calibration", "calibration_part.png")));
        var first = await Inspect(form, false);
        Assert(first?.Result == "OK" && first.Measurement > 29.5, "manual calibration OK");
        Assert(states.Contains(InspectionState.Saving) && states.Contains(InspectionState.Completed), "state path saving/completed");
        Set(form, "imageSource", new SingleImageSource(Image("Basic", "02_rect_small.png")));
        Assert((await Inspect(form, false))?.Result == "NG", "manual small NG");
        using (var screenshot = new Bitmap(form.Width, form.Height))
        {
            form.DrawToBitmap(screenshot, new Rectangle(0, 0, form.Width, form.Height));
            screenshot.Save(Path.Combine(AppContext.BaseDirectory, "hmi-result.png"));
        }
        var products = Field<List<Product>>(form, "products");
        var grid = Field<DataGridView>(form, "dgvProducts");
        Assert(grid.Rows[1].Cells["Measurement"].FormattedValue?.ToString() == "21.90", "F2 current grid");
        Call(form, "RefreshBatchFilterOptions");
        Call(form, "ShowSelectedHistory");
        Assert(grid.Rows[0].Cells["Measurement"].Style.Format == "" && grid.Columns["Measurement"]!.DefaultCellStyle.Format == "F2", "F2 history column");
        Field<ComboBox>(form, "cmbBatchFilter").SelectedItem = products[0].BatchNumber;
        Call(form, "ShowSelectedHistory");
        Assert(grid.Rows.Count == 2 && grid.Columns["Measurement"]!.DefaultCellStyle.Format == "F2", "F2 filtered history");
        Assert(!string.IsNullOrEmpty(new DatabaseService().LoadProducts()[0].InspectTime), "SQLite InspectTime roundtrip");

        int before = products.Count;
        Set(form, "imageSource", new SingleImageSource(Path.Combine(AppContext.BaseDirectory, "missing.png")));
        Assert(await Inspect(form, false) == null && products.Count == before, "missing image no insert");
        Assert(Field<AlarmInfo>(form, "currentAlarm").Code == "ACQUIRE", "missing image alarm");
        string blank = Path.Combine(AppContext.BaseDirectory, "blank.png");
        using (var bitmap = new Bitmap(100, 100)) { using var graphics = Graphics.FromImage(bitmap); graphics.Clear(Color.White); bitmap.Save(blank); }
        Set(form, "imageSource", new SingleImageSource(blank));
        Assert(await Inspect(form, false) == null && Field<AlarmInfo>(form, "currentAlarm").Code == "VISION", "no contour alarm");
        string invalid = Path.Combine(AppContext.BaseDirectory, "invalid.png");
        File.WriteAllText(invalid, "not an image");
        Set(form, "imageSource", new SingleImageSource(invalid));
        Assert(await Inspect(form, false) == null, "image decode failure recovered");

        var folder = new FolderImageSource(Path.Combine(root, "VisionImages", "Basic"));
        string[] names = Enumerable.Range(0, 8).Select(_ => Path.GetFileName(folder.GetNextImagePath())).ToArray();
        Assert(names[0] == "01_rect_normal.png" && names[7] == "08_circle.png", "folder sorted sequence");
        Set(form, "imageSource", folder);
        Assert(await Inspect(form, false) == null && Field<AlarmInfo>(form, "currentAlarm").Code == "SOURCE_EXHAUSTED", "folder exhaustion alarm");

        using var sql = new SqliteConnection("Data Source=" + Path.Combine(AppContext.BaseDirectory, "smartinspect.db"));
        sql.Open();
        using var command = sql.CreateCommand();
        command.CommandText = "CREATE TRIGGER fail_save BEFORE INSERT ON InspectionRecords BEGIN SELECT RAISE(ABORT, 'test save failure'); END;";
        command.ExecuteNonQuery();
        Set(form, "imageSource", new SingleImageSource(Image("Calibration", "calibration_part.png")));
        Assert(await Inspect(form, false) == null && products.Count == before && Field<AlarmInfo>(form, "currentAlarm").Code == "DATABASE", "SQLite failure no product counted");
        command.CommandText = "DROP TRIGGER fail_save";
        command.ExecuteNonQuery();
        sql.Close();

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var data = new SlaveDataStore();
        var factory = new ModbusFactory();
        using var network = factory.CreateSlaveNetwork(listener);
        network.AddSlave(factory.CreateSlave(1, data));
        var serverTask = network.ListenAsync();
        Field<TextBox>(form, "txtPlcPort").Text = port.ToString();
        Call(form, "btnConnectPlc_Click", null, EventArgs.Empty);
        await Wait(() => !Field<bool>(form, "plcCycleRunning"));
        await Wait(() => Field<bool>(form, "triggerArmed"));
        var state = Field<InspectionStateMachine>(form, "stateMachine");
        Set(form, "imageSource", new SlowSource(Image("Calibration", "calibration_part.png")));
        before = products.Count;
        data.HoldingRegisters.WritePoints(0, [1]);
        await Wait(() => data.HoldingRegisters.ReadPoints(1, 1)[0] == 1);
        ushort heartbeat = data.HoldingRegisters.ReadPoints(4, 1)[0];
        await Wait(() => products.Count == before + 1 && state.State == InspectionState.WaitingTrigger);
        Assert(data.HoldingRegisters.ReadPoints(2, 2).SequenceEqual(new ushort[] { 1, 1 }), "PLC OK Result=1 Done=1");
        Assert(data.HoldingRegisters.ReadPoints(4, 1)[0] != heartbeat, "heartbeat alive during acquisition");
        await Task.Delay(1200);
        Assert(products.Count == before + 1, "held Trigger=1 does not duplicate");
        data.HoldingRegisters.WritePoints(0, [0]);
        await Wait(() => Field<bool>(form, "triggerArmed"));
        Set(form, "imageSource", new SingleImageSource(Image("Basic", "02_rect_small.png")));
        data.HoldingRegisters.WritePoints(0, [1]);
        await Wait(() => products.Count == before + 2 && state.State == InspectionState.WaitingTrigger);
        Assert(data.HoldingRegisters.ReadPoints(1, 3).SequenceEqual(new ushort[] { 0, 2, 1 }), "PLC NG Busy=0 Result=2 Done=1");

        data.HoldingRegisters.WritePoints(0, [0]);
        await Wait(() => Field<bool>(form, "triggerArmed"));
        Set(form, "imageSource", new SingleImageSource(invalid));
        data.HoldingRegisters.WritePoints(0, [1]);
        await Wait(() => Field<AlarmInfo>(form, "currentAlarm").Code == "VISION" && state.State == InspectionState.WaitingTrigger);
        Assert(data.HoldingRegisters.ReadPoints(1, 3).SequenceEqual(new ushort[] { 0, 0, 0 }), "vision failure PLC safe reset");
        Assert(products.Count == before + 2, "vision failure no duplicate record");

        Field<System.Windows.Forms.Timer>(form, "plcPollTimer").Stop();
        await Wait(() => !Field<bool>(form, "plcCycleRunning"));
        network.Dispose();
        listener.Stop();
        Call(form, "PlcPollTimer_Tick", null, EventArgs.Empty);
        await Wait(() => !Field<bool>(form, "plcCycleRunning"), 12000);
        Assert(state.State == InspectionState.Error, "PLC disconnect enters Error without crash");
        Assert(!Field<ModbusTcpService>(form, "modbusTcpService").IsConnected, "failed transport disconnected");
        var silentListener = new TcpListener(IPAddress.Loopback, 0);
        silentListener.Start();
        Field<TextBox>(form, "txtPlcPort").Text = ((IPEndPoint)silentListener.LocalEndpoint).Port.ToString();
        var accepting = silentListener.AcceptTcpClientAsync();
        var timeoutWatch = System.Diagnostics.Stopwatch.StartNew();
        Call(form, "btnConnectPlc_Click", null, EventArgs.Empty);
        using (var silentSocket = await accepting)
        {
            await Wait(() => !Field<bool>(form, "plcCycleRunning"), 9000);
            Assert(state.State == InspectionState.Error && timeoutWatch.Elapsed.TotalSeconds < 8, "silent PLC times out, HMI remains responsive");
        }
        silentListener.Stop();

        var recoveryListener = new TcpListener(IPAddress.Loopback, 0);
        recoveryListener.Start();
        using var recoveryNetwork = factory.CreateSlaveNetwork(recoveryListener);
        recoveryNetwork.AddSlave(factory.CreateSlave(1, data));
        var recoveryTask = recoveryNetwork.ListenAsync();
        Field<TextBox>(form, "txtPlcPort").Text = ((IPEndPoint)recoveryListener.LocalEndpoint).Port.ToString();
        data.HoldingRegisters.WritePoints(0, [1]);
        before = products.Count;
        Call(form, "btnConnectPlc_Click", null, EventArgs.Empty);
        await Wait(() => !Field<bool>(form, "plcCycleRunning"));
        await Task.Delay(700);
        Assert(products.Count == before && state.State == InspectionState.WaitingTrigger, "reconnect ignores stale high Trigger");
        Set(form, "imageSource", new FolderImageSource(Path.Combine(root, "VisionImages", "Basic")));
        Call(form, "btnSendTest_Click", null, EventArgs.Empty);
        await Wait(() => products.Count == before + 1 && state.State == InspectionState.WaitingTrigger);
        Assert(products[^1].Result == "OK", "folder demo first trigger normal OK");
        await Wait(() => !Field<bool>(form, "plcCycleRunning"));
        Call(form, "btnSendTest_Click", null, EventArgs.Empty);
        await Wait(() => products.Count == before + 2 && state.State == InspectionState.WaitingTrigger);
        Assert(products[^1].Result == "NG", "folder demo next trigger small NG");
        Field<System.Windows.Forms.Timer>(form, "plcPollTimer").Stop();
        await Wait(() => !Field<bool>(form, "plcCycleRunning"));
        Call(form, "btnDisconnectPlc_Click", null, EventArgs.Empty);
        await Wait(() => !Field<bool>(form, "plcCycleRunning"));
        Assert(data.HoldingRegisters.ReadPoints(1, 3).SequenceEqual(new ushort[] { 0, 0, 0 }), "operator disconnect resets PLC state");
        recoveryNetwork.Dispose();
        recoveryListener.Stop();
        using var snapshot = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(snapshot, new Rectangle(0, 0, form.Width, form.Height));
        snapshot.Save(Path.Combine(AppContext.BaseDirectory, "hmi-verification.png"));
        string log = string.Join("\n", Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Logs")).Select(File.ReadAllText));
        Assert(log.Contains("SQLite") && log.Contains("PLC Result=1") && log.Contains("ERROR"), "logs persisted");
        var failingLog = new LogService(invalid);
        failingLog.Error("must not crash");
        Assert(failingLog.LastWriteError != null, "log write failure does not crash");
        Console.WriteLine("Evidence directory: " + AppContext.BaseDirectory);
    }

    private static string Image(string folder, string name) => Path.Combine(root, "VisionImages", folder, name);
    private static T Field<T>(Form1 form, string name) => (T)typeof(Form1).GetField(name, Private)!.GetValue(form)!;
    private static void Set(Form1 form, string name, object value) => typeof(Form1).GetField(name, Private)!.SetValue(form, value);
    private static object? Call(Form1 form, string name, params object?[] args) => typeof(Form1).GetMethod(name, Private)!.Invoke(form, args);
    private static Task<Product?> Inspect(Form1 form, bool plc) => (Task<Product?>)Call(form, "ExecuteInspectionAsync", plc)!;
    private static void Assert(bool condition, string description)
    {
        if (!condition) throw new Exception(description);
        Console.WriteLine("PASS: " + description);
    }
    private static async Task Wait(Func<bool> predicate, int timeout = 8000)
    {
        var end = DateTime.UtcNow.AddMilliseconds(timeout);
        while (!predicate()) { if (DateTime.UtcNow > end) throw new TimeoutException("Test condition timed out"); await Task.Delay(25); }
    }
    private sealed class SlowSource(string path) : IImageSource
    {
        public string GetNextImagePath() { Task.Delay(2200).GetAwaiter().GetResult(); return path; }
    }
}
