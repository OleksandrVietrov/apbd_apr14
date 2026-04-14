namespace LegacyRenewalApp
{
    public class LegacyBillingGatewayWrapper : IBillingGateway
    {
        //wrapped because the legacy gateway is static
        public void SaveInvoice(RenewalInvoice invoice)
        {
            LegacyBillingGateway.SaveInvoice(invoice);
        }





        public void SendEmail(string email, string subject, string body)
        {
            LegacyBillingGateway.SendEmail(email, subject, body);
        }
    }
}
