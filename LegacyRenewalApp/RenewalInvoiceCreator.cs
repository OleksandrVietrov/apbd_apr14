using System;

namespace LegacyRenewalApp
{
    public class RenewalInvoiceCreator
    {
        public RenewalInvoice Create(
            Customer customer,
            int customerId,
            string normalizedPlanCode,
            string normalizedPaymentMethod,
            int seatCount,
            RenewalCalculation calculation)
        {
            return new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{normalizedPlanCode}",
                CustomerName = customer.FullName,
                PlanCode = normalizedPlanCode,
                PaymentMethod = normalizedPaymentMethod,
                
                SeatCount = seatCount,
                BaseAmount = Math.Round(calculation.BaseAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(calculation.DiscountAmount, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(calculation.SupportFee, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(calculation.PaymentFee, 2, MidpointRounding.AwayFromZero),
                TaxAmount = Math.Round(calculation.TaxAmount, 2, MidpointRounding.AwayFromZero),
                FinalAmount = Math.Round(calculation.FinalAmount, 2, MidpointRounding.AwayFromZero),
                Notes = calculation.Notes,
                GeneratedAt = DateTime.UtcNow
            };
        }
    }
}
