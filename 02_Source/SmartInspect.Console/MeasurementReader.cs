using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartInspect.Core;

namespace SmartInspect.Console
{
    internal class MeasurementReader
    {
        public void ReadMeasurement(Product product)
        {
            while (true)
            {
                System.Console.WriteLine(
                    "请输入 " + product.SerialNumber + " 产品尺寸：");

                var input = System.Console.ReadLine();

                bool parseSuccess =
                    double.TryParse(input, out double measurement);

                if (parseSuccess)
                {
                    product.Measurement = measurement;
                    break;
                }

                System.Console.WriteLine("输入无效，请输入数字");
            }
        }
    }
}