using System;

namespace LegacyRenewalApp
{
    public class RenewalCalculator
    {
        public RenewalCalculation Calculate(
            Customer customer,
            SubscriptionPlan plan,
            int seatCount,
            string normalizedPlanCode,
            string normalizedPaymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            var calculation = new RenewalCalculation();

            calculation.BaseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            string notes = string.Empty;

            calculation.DiscountAmount = CalculateSegmentDiscount(customer, plan, calculation.BaseAmount, ref notes);
            calculation.DiscountAmount += CalculateYearsDiscount(customer, calculation.BaseAmount, ref notes);
            calculation.DiscountAmount += CalculateSeatDiscount(seatCount, calculation.BaseAmount, ref notes);
            calculation.DiscountAmount += CalculateLoyaltyPointsDiscount(customer, useLoyaltyPoints, ref notes);

            decimal subtotalAfterDiscount = calculation.BaseAmount - calculation.DiscountAmount;
            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes += "minimum discounted subtotal applied; ";
            }

            calculation.SupportFee = CalculateSupportFee(normalizedPlanCode, includePremiumSupport, ref notes);
            calculation.PaymentFee = CalculatePaymentFee(normalizedPaymentMethod, subtotalAfterDiscount, calculation.SupportFee, ref notes);

            decimal taxRate = GetTaxRate(customer);
            decimal taxBase = subtotalAfterDiscount + calculation.SupportFee + calculation.PaymentFee;
            calculation.TaxAmount = taxBase * taxRate;
            calculation.FinalAmount = taxBase + calculation.TaxAmount;

            if (calculation.FinalAmount < 500m)
            {
                calculation.FinalAmount = 500m;
                notes += "minimum invoice amount applied; ";
            }

            calculation.Notes = notes.Trim();
            return calculation;
        }

        private decimal CalculateSegmentDiscount(Customer customer, SubscriptionPlan plan, decimal baseAmount, ref string notes)
        {
            if (customer.Segment == "Silver")
            {
                notes += "silver discount; ";
                return baseAmount * 0.05m;
            }
            else if (customer.Segment == "Gold")
            {
                notes += "gold discount; ";
                return baseAmount * 0.10m;
            }
            else if (customer.Segment == "Platinum")
            {
                notes += "platinum discount; ";
                return baseAmount * 0.15m;
            }
            else if (customer.Segment == "Education" && plan.IsEducationEligible)
            {
                notes += "education discount; ";
                return baseAmount * 0.20m;
            }

            return 0m;
        }

        private decimal CalculateYearsDiscount(Customer customer, decimal baseAmount, ref string notes)
        {
            if (customer.YearsWithCompany >= 5)
            {
                notes += "long-term loyalty discount; ";
                return baseAmount * 0.07m;
            }
            else if (customer.YearsWithCompany >= 2)
            {
                notes += "basic loyalty discount; ";
                return baseAmount * 0.03m;
            }

            return 0m;
        }

        private decimal CalculateSeatDiscount(int seatCount, decimal baseAmount, ref string notes)
        {
            if (seatCount >= 50)
            {
                notes += "large team discount; ";
                return baseAmount * 0.12m;
            }
            else if (seatCount >= 20)
            {
                notes += "medium team discount; ";
                return baseAmount * 0.08m;
            }
            else if (seatCount >= 10)
            {
                notes += "small team discount; ";
                return baseAmount * 0.04m;
            }

            return 0m;
        }

        private decimal CalculateLoyaltyPointsDiscount(Customer customer, bool useLoyaltyPoints, ref string notes)
        {
            if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
            {
                int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
                notes += $"loyalty points used: {pointsToUse}; ";
                return pointsToUse;
            }

            return 0m;
        }

        private decimal CalculateSupportFee(string normalizedPlanCode, bool includePremiumSupport, ref string notes)
        {
            decimal supportFee = 0m;

            if (includePremiumSupport)
            {
                if (normalizedPlanCode == "START")
                {
                    supportFee = 250m;
                }
                else if (normalizedPlanCode == "PRO")
                {
                    supportFee = 400m;
                }
                else if (normalizedPlanCode == "ENTERPRISE")
                {
                    supportFee = 700m;
                }

                notes += "premium support included; ";
            }

            return supportFee;
        }

        private decimal CalculatePaymentFee(string normalizedPaymentMethod, decimal subtotalAfterDiscount, decimal supportFee, ref string notes)
        {
            decimal paymentFee = 0m;

            if (normalizedPaymentMethod == "CARD")
            {
                paymentFee = (subtotalAfterDiscount + supportFee) * 0.02m;
                notes += "card payment fee; ";
            }
            else if (normalizedPaymentMethod == "BANK_TRANSFER")
            {
                paymentFee = (subtotalAfterDiscount + supportFee) * 0.01m;
                notes += "bank transfer fee; ";
            }
            else if (normalizedPaymentMethod == "PAYPAL")
            {
                paymentFee = (subtotalAfterDiscount + supportFee) * 0.035m;
                notes += "paypal fee; ";
            }
            else if (normalizedPaymentMethod == "INVOICE")
            {
                paymentFee = 0m;
                notes += "invoice payment; ";
            }
            else
            {
                throw new ArgumentException("Unsupported payment method");
            }

            return paymentFee;
        }

        private decimal GetTaxRate(Customer customer)
        {
            decimal taxRate = 0.20m;

            if (customer.Country == "Poland")
            {
                taxRate = 0.23m;
            }
            else if (customer.Country == "Germany")
            {
                taxRate = 0.19m;
            }
            else if (customer.Country == "Czech Republic")
            {
                taxRate = 0.21m;
            }
            else if (customer.Country == "Norway")
            {
                taxRate = 0.25m;
            }

            return taxRate;
        }
    }
}
