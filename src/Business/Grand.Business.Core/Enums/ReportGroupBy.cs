namespace Grand.Business.Core.Enums;

/// <summary>
///     Represents the time grouping granularity used when generating reports
/// </summary>
public enum ReportGroupBy
{
    /// <summary>
    ///     Group report data by day
    /// </summary>
    Day = 0,

    /// <summary>
    ///     Group report data by week
    /// </summary>
    Week = 1,

    /// <summary>
    ///     Group report data by month
    /// </summary>
    Month = 2
}
