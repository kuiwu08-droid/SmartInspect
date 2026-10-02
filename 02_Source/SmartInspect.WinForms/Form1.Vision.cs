using SmartInspect.Vision;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SmartInspect.WinForms
{
    public partial class Form1
    {
        private readonly VisionService visionService = new VisionService();

        private string currentVisionImagePath =
            @"D:\MachineVision\05_projects\SmartInspect\02_Source\VisionImages\Calibration\calibration_part.png";

        private void BindVisionEvents()
        {
            btnSelectImage.Click -= btnSelectImage_Click;
            btnSelectImage.Click += btnSelectImage_Click;

            if (!string.IsNullOrWhiteSpace(appConfig.LastImagePath)) currentVisionImagePath = appConfig.LastImagePath;
            try
            {
                imageSource = appConfig.ImageSourceMode == "Folder" ? new FolderImageSource(appConfig.ImageFolder) : new SingleImageSource(currentVisionImagePath);
            }
            catch (Exception ex) { imageSourceError = ex.Message; RaiseAlarm("SOURCE", ex.Message); }

            txtImagePath.Text = appConfig.ImageSourceMode == "Folder" ? appConfig.ImageFolder : currentVisionImagePath;

            ShowVisionImage(currentVisionImagePath);
        }

        private void btnSelectImage_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog();

            dialog.Title = "选择视觉检测图片";

            dialog.Filter =
                "图片文件|*.png;*.jpg;*.jpeg;*.bmp|" +
                "所有文件|*.*";

            string? directory =
                Path.GetDirectoryName(currentVisionImagePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                dialog.InitialDirectory = directory;
            }

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            currentVisionImagePath = dialog.FileName;
            imageSource = new SingleImageSource(currentVisionImagePath);
            imageSourceError = null;
            appConfig.LastImagePath = currentVisionImagePath;
            appConfig.ImageSourceMode = "Single";
            SaveConfiguration();
            txtImagePath.Text = currentVisionImagePath;

            ShowVisionImage(currentVisionImagePath);

            lblResult.Text =
                "已选择检测图片：" +
                Path.GetFileName(currentVisionImagePath);

            lblResult.ForeColor = Color.Black;
        }

        private void ShowVisionImage(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                return;
            }

            try
            {
                using Image sourceImage =
                    Image.FromFile(imagePath);

                Image previewImage =
                    new Bitmap(sourceImage);

                Image? oldImage =
                    picVisionPreview.Image;

                picVisionPreview.Image =
                    previewImage;

                oldImage?.Dispose();
            }
            catch (Exception ex)
            {
                RaiseAlarm("IMAGE_PREVIEW", "图片预览失败：" + ex.Message);
            }
        }

        private void SelectImageFolder()
        {
            using var dialog = new FolderBrowserDialog { Description = "选择模拟相机目录（按文件名排序，末尾停止）" };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                imageSource = new FolderImageSource(dialog.SelectedPath);
                imageSourceError = null;
                appConfig.ImageSourceMode = "Folder";
                appConfig.ImageFolder = dialog.SelectedPath;
                txtImagePath.Text = dialog.SelectedPath;
                SaveConfiguration();
                logger.Info("文件夹模拟相机：" + dialog.SelectedPath);
                lblResult.Text = "文件夹模式：每次触发一张，末尾停止；重新选择可从头开始。";
            }
            catch (Exception ex) { RaiseAlarm("SOURCE", ex.Message); }
        }
    }
}
