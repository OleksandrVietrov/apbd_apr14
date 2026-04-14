namespace LegacyRenewalApp
{
    public class RenewalEmailSender
    {
        public void Send(Customer customer, string normalizedPlanCode, RenewalInvoice invoice, IBillingGateway billingGateway)
        {
            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";
                string body =
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                    $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

                billingGateway.SendEmail(customer.Email, subject, body);
            }
        }
    }
}
