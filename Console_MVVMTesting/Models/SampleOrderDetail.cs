using System;

namespace Console_MVVMTesting.Models
{
    // Remove this class once your pages/features are using your data.
    // This is used by the SampleDataService.
    // It is the model class we use to display data on pages like Grid, Chart, and ListDetails.
    public class SampleOrderDetail
    {
        public SampleOrderDetail()
        {
            //System.Diagnostics.Debug.WriteLine("[{0}] {1} ({2:x8})", DateTime.Now.ToString("HH:mm:ss.ff"), "SampleOrderDetail::SampleOrderDetail()", this.GetHashCode());
        }

        public long ProductID { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public double Discount { get; set; }

        public string QuantityPerUnit { get; set; }

        public double UnitPrice { get; set; }

        public string CategoryName { get; set; }

        public string CategoryDescription { get; set; }

        public double Total { get; set; }

        public string ShortDescription => $"Product ID: {ProductID} - {ProductName}";
    }
}
