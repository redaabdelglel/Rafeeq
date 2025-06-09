using System;
using System.Collections.Generic;

namespace Rafeeq.DTOs.Payments
{
    public class EarningsSummaryDto
    {
        public decimal TotalEarnings { get; set; }
        public decimal ThisMonthEarnings { get; set; }
        public decimal LastMonthEarnings { get; set; }
        public int CompletedSessions { get; set; }
        public int UpcomingSessions { get; set; }
        public List<MonthlyEarning> MonthlyEarnings { get; set; } = new List<MonthlyEarning>();
    }

    public class MonthlyEarning
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
    }
}
