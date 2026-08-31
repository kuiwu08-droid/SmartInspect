namespace SmartInspect.Console
{
    internal class InspectionProcess
    {
        public void InspectProduct(Product product)
        {
            if (product.Measurement >= 9.5 && product.Measurement <= 10.5)
            {
                product.Result = "OK";
            }
            else
            {
                product.Result = "NG";
            }
        }
    }
}
