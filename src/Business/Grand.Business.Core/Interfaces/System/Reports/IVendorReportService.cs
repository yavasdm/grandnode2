using Grand.Business.Core.Utilities.System;
using Grand.Domain.Payments;

namespace Grand.Business.Core.Interfaces.System.Reports;

/// <summary>
///     Vendor report service interface
/// </summary>
public interface IVendorReportService
{
    /// <summary>
    ///     Get vendor performance report
    /// </summary>
    /// <param name="storeId">Store identifier</param>
    /// <param name="startTimeUtc">Start date</param>
    /// <param name="endTimeUtc">End date</param>
    /// <param name="os">Order status</param>
    /// <param name="ps">Payment status</param>
    /// <returns>Result</returns>
    Task<IList<VendorPerformanceReportLine>> GetVendorPerformanceReport(
        string storeId = "",
        DateTime? startTimeUtc = null,
        DateTime? endTimeUtc = null,
        int? os = null,
        PaymentStatus? ps = null);
}
