using SmartInspect.Core;
using SmartInspect.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SmartInspect.WinForms
{
    public partial class Form1 : Form
    {
        private InspectionProcess process = new InspectionProcess();
        private DatabaseService databaseService = new DatabaseService();
        private ConfigService configService = new ConfigService();
        private AppConfig appConfig = new AppConfig();

        // 当前批次产品
        private List<Product> products = new List<Product>();

        // 全部历史产品
        private List<Product> allProducts = new List<Product>();

        // 当前批次编号
        private int currentBatchNumber = 1;

        // 当前是否处于历史查看模式
        private bool isHistoryView = false;

        public Form1()
        {
            InitializeComponent();
            InitializeRuntime();

            BindEvents();
            BindCommunicationEvents();
            try
            {
                databaseService.InitializeDatabase();
                allProducts = databaseService.LoadProducts();
            }
            catch (Exception ex) { RaiseAlarm("DATABASE", "数据库初始化失败：" + ex.Message); }
            currentBatchNumber = GetNextBatchNumber();

            LoadInspectionParameters();
            BindVisionEvents();

            lblCurrentBatch.Text =
                "当前批次：BATCH" +
                currentBatchNumber.ToString("D3");

            cmbBatchFilter.Enabled = false;

            RefreshBatchFilterOptions();
            RefreshProductGrid();
            RefreshStatistics(products);

            txtMeasurement.Focus();
        }

        private void BindEvents()
        {
            btnInspect.Click -= btnInspect_Click_1;
            btnInspect.Click += btnInspect_Click_1;

            btnApplyParameters.Click -= btnApplyParameters_Click;
            btnApplyParameters.Click += btnApplyParameters_Click;

            btnClearBatch.Click -= btnClearBatch_Click;
            btnClearBatch.Click += btnClearBatch_Click;

            btnShowHistory.Click -= btnShowHistory_Click;
            btnShowHistory.Click += btnShowHistory_Click;

            btnShowCurrentBatch.Click -= btnShowCurrentBatch_Click;
            btnShowCurrentBatch.Click += btnShowCurrentBatch_Click;

            btnClearHistory.Click -= btnClearHistory_Click;
            btnClearHistory.Click += btnClearHistory_Click;

            cmbBatchFilter.SelectedIndexChanged -=
                cmbBatchFilter_SelectedIndexChanged;

            cmbBatchFilter.SelectedIndexChanged +=
                cmbBatchFilter_SelectedIndexChanged;

            txtMeasurement.KeyDown -= txtMeasurement_KeyDown;
            txtMeasurement.KeyDown += txtMeasurement_KeyDown;
        }

        private void LoadInspectionParameters()
        {
            try { appConfig = configService.LoadConfig(); }
            catch (Exception ex) { RaiseAlarm("CONFIG", "参数读取失败，使用默认值：" + ex.Message); }

            bool applySuccess = process.TryApplyParameters(
                appConfig.MinMeasurement,
                appConfig.MaxMeasurement);

            if (!applySuccess)
            {
                RaiseAlarm("PARAMETER", "已保存的上下限无效，使用默认参数。", AlarmLevel.Warning);
                appConfig = new AppConfig();

                process.TryApplyParameters(
                    appConfig.MinMeasurement,
                    appConfig.MaxMeasurement);
            }

            txtMinMeasurement.Text =
                process.MinMeasurement.ToString();

            txtMaxMeasurement.Text =
                process.MaxMeasurement.ToString();

            lblActiveParameters.Text =
                "当前生效参数：" +
                process.MinMeasurement +
                " ～ " +
                process.MaxMeasurement;
        }

        private async void btnInspect_Click_1(object? sender, EventArgs e)
        {
            await ExecuteInspectionAsync(false);
        }

        private void btnApplyParameters_Click(
            object? sender,
            EventArgs e)
        {
            bool minSuccess = double.TryParse(
                txtMinMeasurement.Text,
                out double minMeasurement);

            bool maxSuccess = double.TryParse(
                txtMaxMeasurement.Text,
                out double maxMeasurement);

            if (!minSuccess)
            {
                RaiseAlarm("PARAMETER", "最小值不是有效数字。", AlarmLevel.Warning);
                lblResult.Text =
                    "最小值输入无效，请输入数字";

                lblResult.ForeColor = Color.Black;

                txtMinMeasurement.Focus();
                txtMinMeasurement.SelectAll();

                return;
            }

            if (!maxSuccess)
            {
                RaiseAlarm("PARAMETER", "最大值不是有效数字。", AlarmLevel.Warning);
                lblResult.Text =
                    "最大值输入无效，请输入数字";

                lblResult.ForeColor = Color.Black;

                txtMaxMeasurement.Focus();
                txtMaxMeasurement.SelectAll();

                return;
            }

            bool applySuccess = process.TryApplyParameters(
                minMeasurement,
                maxMeasurement);

            if (!applySuccess)
            {
                RaiseAlarm("PARAMETER", "参数必须有限且最小值小于最大值。", AlarmLevel.Warning);
                lblResult.Text =
                    "检测参数无效：最小值必须小于最大值";

                lblResult.ForeColor = Color.Black;

                txtMinMeasurement.Text =
                    process.MinMeasurement.ToString();

                txtMaxMeasurement.Text =
                    process.MaxMeasurement.ToString();

                return;
            }

            appConfig.MinMeasurement =
                process.MinMeasurement;

            appConfig.MaxMeasurement =
                process.MaxMeasurement;

            SaveConfiguration();
            logger.Info($"参数修改 Min={process.MinMeasurement}, Max={process.MaxMeasurement}");

            lblActiveParameters.Text =
                "当前生效参数：" +
                process.MinMeasurement +
                " ～ " +
                process.MaxMeasurement;

            lblResult.Text =
                "参数已应用并保存：" +
                process.MinMeasurement +
                " ～ " +
                process.MaxMeasurement;

            lblResult.ForeColor = Color.Black;

            txtMeasurement.Focus();
        }

        private void txtMeasurement_KeyDown(
            object? sender,
            KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnInspect.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        private void btnClearBatch_Click(
            object? sender,
            EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "确定要结束当前批次并开始新批次吗？",
                "确认切换批次",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                txtMeasurement.Focus();
                return;
            }

            products.Clear();
            currentBatchNumber++;

            lblCurrentBatch.Text =
                "当前批次：BATCH" +
                currentBatchNumber.ToString("D3");

            isHistoryView = false;
            cmbBatchFilter.Enabled = false;

            RefreshProductGrid();
            RefreshStatistics(products);

            lblResult.Text =
                "已开始新批次：BATCH" +
                currentBatchNumber.ToString("D3");

            lblResult.ForeColor = Color.Black;

            txtMeasurement.Clear();
            txtMeasurement.Focus();
        }

        private void btnShowHistory_Click(
            object? sender,
            EventArgs e)
        {
            isHistoryView = true;
            cmbBatchFilter.Enabled = true;

            try { allProducts = databaseService.LoadProducts(); }
            catch (Exception ex) { RaiseAlarm("DATABASE", "历史读取失败：" + ex.Message); return; }

            RefreshBatchFilterOptions();
            ShowSelectedHistory();
        }

        private void btnShowCurrentBatch_Click(
            object? sender,
            EventArgs e)
        {
            isHistoryView = false;
            cmbBatchFilter.Enabled = false;

            RefreshProductGrid();
            RefreshStatistics(products);

            lblResult.Text =
                "正在查看当前批次：BATCH" +
                currentBatchNumber.ToString("D3");

            lblResult.ForeColor = Color.Black;

            txtMeasurement.Focus();
        }

        private void btnClearHistory_Click(
            object? sender,
            EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "此操作将永久删除全部历史检测记录，" +
                "同时清空当前批次数据。\n\n" +
                "删除后无法恢复，确定继续吗？",
                "确认清空历史",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                txtMeasurement.Focus();
                return;
            }

            try { databaseService.ClearHistory(); }
            catch (Exception ex) { RaiseAlarm("DATABASE", "清空历史失败：" + ex.Message); return; }

            products.Clear();
            allProducts.Clear();

            currentBatchNumber = 1;

            lblCurrentBatch.Text =
                "当前批次：BATCH001";

            RefreshBatchFilterOptions();

            isHistoryView = false;
            cmbBatchFilter.Enabled = false;

            RefreshProductGrid();
            RefreshStatistics(products);

            lblResult.Text =
                "历史记录已清空，已重新开始 BATCH001";

            lblResult.ForeColor = Color.Black;

            txtMeasurement.Clear();
            txtMeasurement.Focus();
        }

        private void cmbBatchFilter_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            if (!isHistoryView)
            {
                return;
            }

            ShowSelectedHistory();
        }

        private void RefreshBatchFilterOptions()
        {
            string? previousSelection =
                cmbBatchFilter.SelectedItem?.ToString();

            cmbBatchFilter.Items.Clear();

            cmbBatchFilter.Items.Add(
                "全部批次");

            List<string> batchNumbers =
                new List<string>();

            foreach (Product product in allProducts)
            {
                if (!batchNumbers.Contains(
                    product.BatchNumber))
                {
                    batchNumbers.Add(
                        product.BatchNumber);
                }
            }

            foreach (string batchNumber in batchNumbers)
            {
                cmbBatchFilter.Items.Add(
                    batchNumber);
            }

            if (previousSelection != null &&
                cmbBatchFilter.Items.Contains(
                    previousSelection))
            {
                cmbBatchFilter.SelectedItem =
                    previousSelection;
            }
            else
            {
                cmbBatchFilter.SelectedIndex = 0;
            }
        }

        private void ShowSelectedHistory()
        {
            if (cmbBatchFilter.SelectedItem == null)
            {
                return;
            }

            string selectedBatch =
                cmbBatchFilter.SelectedItem.ToString()!;

            if (selectedBatch == "全部批次")
            {
                dgvProducts.DataSource = null;
                dgvProducts.DataSource = allProducts;
                FormatProductGrid();

                RefreshStatistics(allProducts);

                lblResult.Text =
                    "正在查看全部历史记录，共 " +
                    allProducts.Count +
                    " 条";

                lblResult.ForeColor = Color.Black;

                return;
            }

            List<Product> filteredProducts =
                new List<Product>();

            foreach (Product product in allProducts)
            {
                if (product.BatchNumber ==
                    selectedBatch)
                {
                    filteredProducts.Add(product);
                }
            }

            dgvProducts.DataSource = null;
            dgvProducts.DataSource =
                filteredProducts;
            FormatProductGrid();

            RefreshStatistics(filteredProducts);

            lblResult.Text =
                "正在查看历史批次：" +
                selectedBatch +
                "，共 " +
                filteredProducts.Count +
                " 条";

            lblResult.ForeColor = Color.Black;
        }

        private void RefreshProductGrid()
        {
            dgvProducts.DataSource = null;
            dgvProducts.DataSource = products;
            FormatProductGrid();
        }

        private void FormatProductGrid()
        {
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            if (dgvProducts.Columns["Measurement"] is DataGridViewColumn measurementColumn)
            {
                measurementColumn.DefaultCellStyle.Format = "F2";
            }
            if (dgvProducts.Columns["InspectTime"] is DataGridViewColumn timeColumn) timeColumn.DisplayIndex = dgvProducts.Columns.Count - 1;

            foreach (DataGridViewColumn column in dgvProducts.Columns)
            {
                column.FillWeight = column.Name switch
                {
                    "BatchNumber" => 30,
                    "SerialNumber" => 25,
                    "Measurement" => 25,
                    "Result" => 20,
                    _ => column.FillWeight
                };
                column.MinimumWidth = column.GetPreferredWidth(DataGridViewAutoSizeColumnMode.ColumnHeader, true);
            }
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void RefreshStatistics(
            List<Product> source)
        {
            int okCount =
                process.GetOkCount(source);

            int failedCount =
                process.GetFailedCount(source);

            double yieldRate =
                process.CalculateYield(source);

            lblTotal.Text =
                "Total：" + source.Count;

            lblOkCount.Text =
                "OK：" + okCount;

            lblNgCount.Text =
                "NG：" + failedCount;

            lblYield.Text =
                "Yield：" +
                yieldRate.ToString("F2") +
                "%";
        }

        private int GetNextBatchNumber()
        {
            int maxBatchNumber = 0;

            foreach (Product product in allProducts)
            {
                string batchNumber =
                    product.BatchNumber;

                if (!batchNumber.StartsWith("BATCH"))
                {
                    continue;
                }

                string numberPart =
                    batchNumber.Substring(5);

                bool parseSuccess = int.TryParse(
                    numberPart,
                    out int parsedNumber);

                if (!parseSuccess)
                {
                    continue;
                }

                if (parsedNumber > maxBatchNumber)
                {
                    maxBatchNumber = parsedNumber;
                }
            }

            return maxBatchNumber + 1;
        }

        // 以下空事件暂时保留，
        // 用于兼容 Designer 之前遗留的事件绑定。

        private void Form1_Load(
            object? sender,
            EventArgs e)
        {
        }

        private void lblTotal_Click(
            object? sender,
            EventArgs e)
        {
        }

        private void button1_Click(
            object? sender,
            EventArgs e)
        {
        }

        private void button1_Click_1(
            object? sender,
            EventArgs e)
        {
        }

        private void button1_Click_2(
            object? sender,
            EventArgs e)
        {
        }

        private void lblResult_Click(object sender, EventArgs e)
        {

        }
    }
}
