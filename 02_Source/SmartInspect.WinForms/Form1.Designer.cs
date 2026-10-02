namespace SmartInspect.WinForms
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">
        /// true if managed resources should be disposed;
        /// otherwise, false.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support.
        /// </summary>
        private void InitializeComponent()
        {
            lblMeasurement = new Label();
            txtMeasurement = new TextBox();
            btnInspect = new Button();
            lblResult = new Label();
            dgvProducts = new DataGridView();
            lblTotal = new Label();
            lblOkCount = new Label();
            lblNgCount = new Label();
            lblYield = new Label();
            lblMinMeasurement = new Label();
            lblMaxMeasurement = new Label();
            txtMinMeasurement = new TextBox();
            txtMaxMeasurement = new TextBox();
            btnApplyParameters = new Button();
            lblActiveParameters = new Label();
            btnClearBatch = new Button();
            lblCurrentBatch = new Label();
            btnShowHistory = new Button();
            btnShowCurrentBatch = new Button();
            btnClearHistory = new Button();
            lblBatchFilter = new Label();
            cmbBatchFilter = new ComboBox();
            lblPlcIpTitle = new Label();
            txtPlcIp = new TextBox();
            lblPlcPortTitle = new Label();
            txtPlcPort = new TextBox();
            btnConnectPlc = new Button();
            btnDisconnectPlc = new Button();
            lblPlcStatus = new Label();
            btnSendTest = new Button();
            lblPlcResponse = new Label();
            txtImagePath = new TextBox();
            btnSelectImage = new Button();
            picVisionPreview = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)dgvProducts).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picVisionPreview).BeginInit();
            SuspendLayout();
            // 
            // lblMeasurement
            // 
            lblMeasurement.AutoSize = true;
            lblMeasurement.Location = new Point(54, 146);
            lblMeasurement.Name = "lblMeasurement";
            lblMeasurement.Size = new Size(68, 17);
            lblMeasurement.TabIndex = 0;
            lblMeasurement.Text = "产品尺寸：";
            // 
            // txtMeasurement
            // 
            txtMeasurement.Location = new Point(114, 166);
            txtMeasurement.Name = "txtMeasurement";
            txtMeasurement.Size = new Size(100, 23);
            txtMeasurement.TabIndex = 1;
            // 
            // btnInspect
            // 
            btnInspect.Location = new Point(220, 166);
            btnInspect.Name = "btnInspect";
            btnInspect.Size = new Size(75, 23);
            btnInspect.TabIndex = 2;
            btnInspect.Text = "手动检测";
            btnInspect.UseVisualStyleBackColor = true;
            btnInspect.Click += btnInspect_Click_1;
            // 
            // lblResult
            // 
            lblResult.AutoSize = true;
            lblResult.Location = new Point(54, 201);
            lblResult.Name = "lblResult";
            lblResult.Size = new Size(78, 17);
            lblResult.TabIndex = 3;
            lblResult.Text = "检测结果：--";
            lblResult.Click += lblResult_Click;
            // 
            // dgvProducts
            // 
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.AllowUserToDeleteRows = false;
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProducts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvProducts.Location = new Point(12, 239);
            dgvProducts.Name = "dgvProducts";
            dgvProducts.ReadOnly = true;
            dgvProducts.Size = new Size(320, 222);
            dgvProducts.TabIndex = 4;
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.Location = new Point(339, 239);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(56, 17);
            lblTotal.TabIndex = 5;
            lblTotal.Text = "Total：0";
            lblTotal.Click += lblTotal_Click;
            // 
            // lblOkCount
            // 
            lblOkCount.AutoSize = true;
            lblOkCount.Location = new Point(339, 270);
            lblOkCount.Name = "lblOkCount";
            lblOkCount.Size = new Size(45, 17);
            lblOkCount.TabIndex = 6;
            lblOkCount.Text = "OK：0";
            // 
            // lblNgCount
            // 
            lblNgCount.AutoSize = true;
            lblNgCount.Location = new Point(339, 306);
            lblNgCount.Name = "lblNgCount";
            lblNgCount.Size = new Size(46, 17);
            lblNgCount.TabIndex = 7;
            lblNgCount.Text = "NG：0";
            // 
            // lblYield
            // 
            lblYield.AutoSize = true;
            lblYield.Location = new Point(339, 343);
            lblYield.Name = "lblYield";
            lblYield.Size = new Size(83, 17);
            lblYield.TabIndex = 8;
            lblYield.Text = "Yield：0.00%";
            // 
            // lblMinMeasurement
            // 
            lblMinMeasurement.AutoSize = true;
            lblMinMeasurement.Location = new Point(54, 23);
            lblMinMeasurement.Name = "lblMinMeasurement";
            lblMinMeasurement.Size = new Size(56, 17);
            lblMinMeasurement.TabIndex = 10;
            lblMinMeasurement.Text = "最小值：";
            // 
            // lblMaxMeasurement
            // 
            lblMaxMeasurement.AutoSize = true;
            lblMaxMeasurement.Location = new Point(54, 72);
            lblMaxMeasurement.Name = "lblMaxMeasurement";
            lblMaxMeasurement.Size = new Size(56, 17);
            lblMaxMeasurement.TabIndex = 11;
            lblMaxMeasurement.Text = "最大值：";
            // 
            // txtMinMeasurement
            // 
            txtMinMeasurement.Location = new Point(114, 23);
            txtMinMeasurement.Name = "txtMinMeasurement";
            txtMinMeasurement.Size = new Size(100, 23);
            txtMinMeasurement.TabIndex = 12;
            txtMinMeasurement.Text = "9.5";
            // 
            // txtMaxMeasurement
            // 
            txtMaxMeasurement.Location = new Point(114, 72);
            txtMaxMeasurement.Name = "txtMaxMeasurement";
            txtMaxMeasurement.Size = new Size(100, 23);
            txtMaxMeasurement.TabIndex = 13;
            txtMaxMeasurement.Text = "10.5";
            // 
            // btnApplyParameters
            // 
            btnApplyParameters.Location = new Point(114, 107);
            btnApplyParameters.Name = "btnApplyParameters";
            btnApplyParameters.Size = new Size(75, 23);
            btnApplyParameters.TabIndex = 14;
            btnApplyParameters.Text = "应用参数";
            btnApplyParameters.UseVisualStyleBackColor = true;
            btnApplyParameters.Click += btnApplyParameters_Click;
            // 
            // lblActiveParameters
            // 
            lblActiveParameters.AutoSize = true;
            lblActiveParameters.Location = new Point(242, 29);
            lblActiveParameters.Name = "lblActiveParameters";
            lblActiveParameters.Size = new Size(153, 17);
            lblActiveParameters.TabIndex = 15;
            lblActiveParameters.Text = "当前生效参数：9.5 ～ 10.5";
            // 
            // btnClearBatch
            // 
            btnClearBatch.Location = new Point(414, 236);
            btnClearBatch.Name = "btnClearBatch";
            btnClearBatch.Size = new Size(75, 23);
            btnClearBatch.TabIndex = 16;
            btnClearBatch.Text = " 清空批次";
            btnClearBatch.UseVisualStyleBackColor = true;
            // 
            // lblCurrentBatch
            // 
            lblCurrentBatch.AutoSize = true;
            lblCurrentBatch.Location = new Point(242, 75);
            lblCurrentBatch.Name = "lblCurrentBatch";
            lblCurrentBatch.Size = new Size(129, 17);
            lblCurrentBatch.TabIndex = 17;
            lblCurrentBatch.Text = "当前批次：BATCH001";
            // 
            // btnShowHistory
            // 
            btnShowHistory.Location = new Point(414, 300);
            btnShowHistory.Name = "btnShowHistory";
            btnShowHistory.Size = new Size(75, 23);
            btnShowHistory.TabIndex = 18;
            btnShowHistory.Text = " 查看历史";
            btnShowHistory.UseVisualStyleBackColor = true;
            btnShowHistory.Click += button1_Click;
            // 
            // btnShowCurrentBatch
            // 
            btnShowCurrentBatch.Location = new Point(414, 270);
            btnShowCurrentBatch.Name = "btnShowCurrentBatch";
            btnShowCurrentBatch.Size = new Size(75, 23);
            btnShowCurrentBatch.TabIndex = 19;
            btnShowCurrentBatch.Text = "当前批次";
            btnShowCurrentBatch.UseVisualStyleBackColor = true;
            btnShowCurrentBatch.Click += button1_Click_1;
            // 
            // btnClearHistory
            // 
            btnClearHistory.Location = new Point(338, 377);
            btnClearHistory.Name = "btnClearHistory";
            btnClearHistory.Size = new Size(75, 23);
            btnClearHistory.TabIndex = 20;
            btnClearHistory.Text = "清空历史";
            btnClearHistory.UseVisualStyleBackColor = true;
            btnClearHistory.Click += button1_Click_2;
            // 
            // lblBatchFilter
            // 
            lblBatchFilter.AutoSize = true;
            lblBatchFilter.Location = new Point(510, 236);
            lblBatchFilter.Name = "lblBatchFilter";
            lblBatchFilter.Size = new Size(68, 17);
            lblBatchFilter.TabIndex = 21;
            lblBatchFilter.Text = "历史批次：";
            // 
            // cmbBatchFilter
            // 
            cmbBatchFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBatchFilter.FormattingEnabled = true;
            cmbBatchFilter.Location = new Point(584, 236);
            cmbBatchFilter.Name = "cmbBatchFilter";
            cmbBatchFilter.Size = new Size(121, 25);
            cmbBatchFilter.TabIndex = 22;
            // 
            // lblPlcIpTitle
            // 
            lblPlcIpTitle.AutoSize = true;
            lblPlcIpTitle.Location = new Point(477, 20);
            lblPlcIpTitle.Name = "lblPlcIpTitle";
            lblPlcIpTitle.Size = new Size(56, 17);
            lblPlcIpTitle.TabIndex = 23;
            lblPlcIpTitle.Text = "PLC IP：";
            // 
            // txtPlcIp
            // 
            txtPlcIp.Location = new Point(540, 20);
            txtPlcIp.Name = "txtPlcIp";
            txtPlcIp.Size = new Size(100, 23);
            txtPlcIp.TabIndex = 24;
            txtPlcIp.Text = "127.0.0.1";
            // 
            // lblPlcPortTitle
            // 
            lblPlcPortTitle.AutoSize = true;
            lblPlcPortTitle.Location = new Point(477, 60);
            lblPlcPortTitle.Name = "lblPlcPortTitle";
            lblPlcPortTitle.Size = new Size(44, 17);
            lblPlcPortTitle.TabIndex = 25;
            lblPlcPortTitle.Text = "端口：";
            // 
            // txtPlcPort
            // 
            txtPlcPort.Location = new Point(540, 60);
            txtPlcPort.Name = "txtPlcPort";
            txtPlcPort.Size = new Size(100, 23);
            txtPlcPort.TabIndex = 26;
            txtPlcPort.Text = "5020";
            // 
            // btnConnectPlc
            // 
            btnConnectPlc.Location = new Point(677, 17);
            btnConnectPlc.Name = "btnConnectPlc";
            btnConnectPlc.Size = new Size(75, 23);
            btnConnectPlc.TabIndex = 27;
            btnConnectPlc.Text = "连接 PLC";
            btnConnectPlc.UseVisualStyleBackColor = true;
            // 
            // btnDisconnectPlc
            // 
            btnDisconnectPlc.Location = new Point(677, 60);
            btnDisconnectPlc.Name = "btnDisconnectPlc";
            btnDisconnectPlc.Size = new Size(75, 23);
            btnDisconnectPlc.TabIndex = 28;
            btnDisconnectPlc.Text = "断开 PLC";
            btnDisconnectPlc.UseVisualStyleBackColor = true;
            // 
            // lblPlcStatus
            // 
            lblPlcStatus.AutoSize = true;
            lblPlcStatus.Location = new Point(477, 97);
            lblPlcStatus.Name = "lblPlcStatus";
            lblPlcStatus.Size = new Size(101, 17);
            lblPlcStatus.TabIndex = 29;
            lblPlcStatus.Text = "PLC状态：未连接";
            // 
            // btnSendTest
            // 
            btnSendTest.Location = new Point(477, 140);
            btnSendTest.Name = "btnSendTest";
            btnSendTest.Size = new Size(75, 23);
            btnSendTest.TabIndex = 30;
            btnSendTest.Text = "模拟触发";
            btnSendTest.UseVisualStyleBackColor = true;
            // 
            // lblPlcResponse
            // 
            lblPlcResponse.AutoSize = true;
            lblPlcResponse.Location = new Point(477, 172);
            lblPlcResponse.Name = "lblPlcResponse";
            lblPlcResponse.Size = new Size(75, 17);
            lblPlcResponse.TabIndex = 31;
            lblPlcResponse.Text = "PLC回复：--";
            // 
            // txtImagePath
            // 
            txtImagePath.Location = new Point(301, 166);
            txtImagePath.Name = "txtImagePath";
            txtImagePath.ReadOnly = true;
            txtImagePath.Size = new Size(79, 23);
            txtImagePath.TabIndex = 32;
            txtImagePath.Text = "留空";
            // 
            // btnSelectImage
            // 
            btnSelectImage.Location = new Point(301, 195);
            btnSelectImage.Name = "btnSelectImage";
            btnSelectImage.Size = new Size(75, 23);
            btnSelectImage.TabIndex = 33;
            btnSelectImage.Text = "选择图片";
            btnSelectImage.UseVisualStyleBackColor = true;
            // 
            // picVisionPreview
            // 
            picVisionPreview.BorderStyle = BorderStyle.FixedSingle;
            picVisionPreview.Location = new Point(528, 289);
            picVisionPreview.Name = "picVisionPreview";
            picVisionPreview.Size = new Size(300, 220);
            picVisionPreview.SizeMode = PictureBoxSizeMode.Zoom;
            picVisionPreview.TabIndex = 34;
            picVisionPreview.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(857, 577);
            Controls.Add(picVisionPreview);
            Controls.Add(btnSelectImage);
            Controls.Add(txtImagePath);
            Controls.Add(lblPlcResponse);
            Controls.Add(btnSendTest);
            Controls.Add(lblPlcStatus);
            Controls.Add(btnDisconnectPlc);
            Controls.Add(btnConnectPlc);
            Controls.Add(txtPlcPort);
            Controls.Add(lblPlcPortTitle);
            Controls.Add(txtPlcIp);
            Controls.Add(lblPlcIpTitle);
            Controls.Add(cmbBatchFilter);
            Controls.Add(lblBatchFilter);
            Controls.Add(btnClearHistory);
            Controls.Add(btnShowCurrentBatch);
            Controls.Add(btnShowHistory);
            Controls.Add(lblCurrentBatch);
            Controls.Add(btnClearBatch);
            Controls.Add(lblActiveParameters);
            Controls.Add(btnApplyParameters);
            Controls.Add(txtMaxMeasurement);
            Controls.Add(txtMinMeasurement);
            Controls.Add(lblMaxMeasurement);
            Controls.Add(lblMinMeasurement);
            Controls.Add(lblYield);
            Controls.Add(lblNgCount);
            Controls.Add(lblOkCount);
            Controls.Add(lblTotal);
            Controls.Add(dgvProducts);
            Controls.Add(lblResult);
            Controls.Add(btnInspect);
            Controls.Add(txtMeasurement);
            Controls.Add(lblMeasurement);
            Name = "Form1";
            Text = "D";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dgvProducts).EndInit();
            ((System.ComponentModel.ISupportInitialize)picVisionPreview).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblMeasurement;
        private TextBox txtMeasurement;
        private Button btnInspect;
        private Label lblResult;

        private DataGridView dgvProducts;

        private Label lblTotal;
        private Label lblOkCount;
        private Label lblNgCount;
        private Label lblYield;

        private Label lblMinMeasurement;
        private Label lblMaxMeasurement;

        private TextBox txtMinMeasurement;
        private TextBox txtMaxMeasurement;

        private Button btnApplyParameters;
        private Label lblActiveParameters;
        private Button btnClearBatch;
        private Label lblCurrentBatch;
        private Button btnShowHistory;
        private Button btnShowCurrentBatch;
        private Button btnClearHistory;
        private Label lblBatchFilter;
        private ComboBox cmbBatchFilter;
        private Label lblPlcIpTitle;
        private TextBox txtPlcIp;
        private Label lblPlcPortTitle;
        private TextBox txtPlcPort;
        private Button btnConnectPlc;
        private Button btnDisconnectPlc;
        private Label lblPlcStatus;
        private Button btnSendTest;
        private Label lblPlcResponse;
        private TextBox txtImagePath;
        private Button btnSelectImage;
        private PictureBox picVisionPreview;
    }
}