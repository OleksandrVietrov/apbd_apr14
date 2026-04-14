using System;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ISubscriptionPlanRepository _planRepository;
        private readonly RenewalInputValidator _validator;
        private readonly RenewalCalculator _calculator;
        private readonly RenewalInvoiceCreator _invoiceCreator;
        private readonly RenewalEmailSender _emailSender;
        private readonly IBillingGateway _billingGateway;

        //kept empty constructor so existing consumer code still works
        public SubscriptionRenewalService()
            : this(
                new CustomerRepository(),
                new SubscriptionPlanRepository(),
                new RenewalInputValidator(),
                new RenewalCalculator(),
                new RenewalInvoiceCreator(),
                new RenewalEmailSender(),
                new LegacyBillingGatewayWrapper())
        {
        }

        public SubscriptionRenewalService(
            ICustomerRepository customerRepository,
            ISubscriptionPlanRepository planRepository,
            RenewalInputValidator validator,
            RenewalCalculator calculator,
            RenewalInvoiceCreator invoiceCreator,
            RenewalEmailSender emailSender,
            IBillingGateway billingGateway)
        {
            _customerRepository = customerRepository;
            _planRepository = planRepository;
            _validator = validator;
            _calculator = calculator;
            _invoiceCreator = invoiceCreator;
            _emailSender = emailSender;
            _billingGateway = billingGateway;
        }

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            _validator.Validate(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();

            var customer = _customerRepository.GetById(customerId);
            var plan = _planRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            var calculation = _calculator.Calculate(
                customer,
                plan,
                seatCount,
                normalizedPlanCode,
                normalizedPaymentMethod,
                includePremiumSupport,
                useLoyaltyPoints);

            var invoice = _invoiceCreator.Create(
                customer,
                customerId,
                normalizedPlanCode,
                normalizedPaymentMethod,
                seatCount,
                calculation);

            _billingGateway.SaveInvoice(invoice);

            _emailSender.Send(customer, normalizedPlanCode, invoice, _billingGateway);

            return invoice;
        }
    }
}
