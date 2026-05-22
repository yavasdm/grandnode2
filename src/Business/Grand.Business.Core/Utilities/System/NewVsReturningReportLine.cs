namespace Grand.Business.Core.Utilities.System;

/// <summary>
///     Represents a new vs. returning customers report line
/// </summary>
public class NewVsReturningReportLine
{
    /// <summary>
    ///     Gets or sets the time period label for this report entry
    /// </summary>
    public string TimePeriod { get; set; }

    /// <summary>
    ///     Gets or sets the number of new customers in the time period
    /// </summary>
    public int NewCustomers { get; set; }

    /// <summary>
    ///     Gets or sets the number of returning customers in the time period
    /// </summary>
    public int ReturningCustomers { get; set; }
}
