using System.Collections.Generic;

namespace SmartInspect.Core
{
    public class InspectionProcess
    {
        public double MinMeasurement { get; set; } = 9.5;
        public double MaxMeasurement { get; set; } = 10.5;

        public bool TryApplyParameters(
               double minMeasurement,
               double maxMeasurement)
        {
            if (!double.IsFinite(minMeasurement) || !double.IsFinite(maxMeasurement) || minMeasurement >= maxMeasurement)
            {
                return false;
            }

            MinMeasurement = minMeasurement;
            MaxMeasurement = maxMeasurement;

            return true;
        }

        public bool ValidateParameters()
        {
            return MinMeasurement < MaxMeasurement;
        }

        public void InspectProduct(Product product)
        {
            if (product.Measurement >= MinMeasurement &&
                product.Measurement <= MaxMeasurement)
            {
                product.Result = "OK";
            }
            else
            {
                product.Result = "NG";
            }
        }

        public int GetOkCount(List<Product> products)
        {
            int okCount = 0;

            foreach (Product product in products)
            {
                if (product.Result == "OK")
                {
                    okCount++;
                }
            }

            return okCount;
        }

        public int GetFailedCount(List<Product> products)
        {
            int okCount = GetOkCount(products);

            return products.Count - okCount;
        }

        public double CalculateYield(List<Product> products)
        {
            if (products.Count == 0)
            {
                return 0;
            }

            int okCount = GetOkCount(products);

            return (double)okCount / products.Count * 100;
        }
    }
}
