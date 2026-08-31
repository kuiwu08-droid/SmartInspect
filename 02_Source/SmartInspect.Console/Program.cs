using System.Collections.Generic;

namespace SmartInspect.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("SmartInspect v0.05");
            System.Console.WriteLine("System Ready");

            int okCount = 0;

            List<Product> products = new List<Product>();
            InspectionProcess process = new InspectionProcess();

            for (int i = 0; i < 3; i = i + 1)
            {
                Product product = new Product();
                product.SerialNumber = "SN" + (i + 1);

                ReadMeasurement(product);
                process.InspectProduct(product);

                if (product.Result == "OK")
                {
                    okCount++;
                }

                products.Add(product);
            }

            int failedCount = GetFailedCount(products.Count, okCount);
            double yieldRate = CalculateYield(products.Count, okCount);

            System.Console.WriteLine();
            System.Console.WriteLine("===== 产品记录 =====");

            foreach (Product product in products)
            {
                System.Console.WriteLine(product.SerialNumber + " | 尺寸：" + product.Measurement + " | 结果：" + product.Result);
            }

            System.Console.WriteLine();
            System.Console.WriteLine("===== 检测汇总 =====");
            System.Console.WriteLine("检测总量：" + products.Count);
            System.Console.WriteLine("合格数量：" + okCount);
            System.Console.WriteLine("不合格数量：" + failedCount);
            System.Console.WriteLine("良品率：" + yieldRate.ToString("F2") + "%");
        }

        static void ReadMeasurement(Product product)
        {
            while (true)
            {
                System.Console.WriteLine("请输入 " + product.SerialNumber + " 产品尺寸：");

                var input = System.Console.ReadLine();
                bool parseSuccess = double.TryParse(input, out double measurement);

                if (parseSuccess)
                {
                    product.Measurement = measurement;
                    break;
                }

                System.Console.WriteLine("输入无效，请输入数字");
            }
        }

        static int GetFailedCount(int total, int ok)
        {
            return total - ok;
        }

        static double CalculateYield(int total, int ok)
        {
            return (double)ok / total * 100;
        }
    }
}
