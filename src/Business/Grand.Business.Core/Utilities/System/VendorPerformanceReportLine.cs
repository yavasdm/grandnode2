namespace Grand.Business.Core.Utilities.System;

public class VendorPerformanceReportLine
{
    public string VendorId { get; set; }
    public double TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public double AverageOrderValue { get; set; }
}
