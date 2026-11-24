using Coravel.Invocable;
using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using AQUA.Models;

namespace AQUA.Services
{
    public class ProcessOrder : IInvocable
    {
        private readonly ILogger<ProcessOrder> _logger;
        private readonly IOrderConnector _orderConnector;
        private readonly IFluentEmailFactory _emailFactory;

        public ProcessOrder(
            ILogger<ProcessOrder> logger,
            IOrderConnector orderConnector,
            IFluentEmailFactory emailFactory)
        {
            _logger = logger;
            _orderConnector = orderConnector;
            _emailFactory = emailFactory;
        }

        public async Task Invoke()
        {
            try
            {
                _logger.LogInformation("Checking for pending subscription orders...");

                var nextOrder = await _orderConnector.GetNextOrder();

                if (nextOrder == null)
                {
                    _logger.LogInformation("No pending subscriptions found.");
                    return;
                }

                _logger.LogInformation(
                    "Processing subscription {OrderId} for {Email}",
                    nextOrder.OrderId,
                    nextOrder.CustomerEmail
                );

                // ===============================
                // EMAIL TEMPLATE (HTML)
                // ===============================
                var emailTemplate = @"
<html>
<body style='font-family: Arial, sans-serif'>
    <h2>🎉 Welcome to Our Newsletter!</h2>
    <p>Dear @Model.CustomerName,</p>
    <p>Thank you for subscribing to our newsletter.</p>
    <p>You will now receive updates directly in your inbox.</p>
    <br/>
    <p>
        Best regards,<br/>
        <strong>Newsletter Team</strong>
    </p>
</body>
</html>";

                // ===============================
                // 1️⃣ EMAIL TO SUBSCRIBER
                // ===============================
                var subscriberEmail = _emailFactory.Create();

                var subscriberResult = await subscriberEmail
                    .To(nextOrder.CustomerEmail)
                    .Subject($"Welcome {nextOrder.CustomerName}! 🎉")
                    .UsingTemplate(emailTemplate, nextOrder)
                    .SendAsync();

                if (!subscriberResult.Successful)
                {
                    _logger.LogError(
                        "Failed to send subscriber email: {Errors}",
                        string.Join(", ", subscriberResult.ErrorMessages)
                    );
                    return;
                }

                // ===============================
                // 2️⃣ EMAIL TO OWNER
                // ===============================
                var ownerEmail = _emailFactory.Create();

                var ownerResult = await ownerEmail
                    .To("limzheyu7@gmail.com")
                    .Subject("📩 New Newsletter Subscription")
                    .Body(
                        $"New subscriber received:\n\n" +
                        $"Name: {nextOrder.CustomerName}\n" +
                        $"Email: {nextOrder.CustomerEmail}\n" +
                        $"Order ID: {nextOrder.OrderId}",
                        isHtml: false
                    )
                    .SendAsync();

                if (!ownerResult.Successful)
                {
                    _logger.LogError(
                        "Failed to send owner notification: {Errors}",
                        string.Join(", ", ownerResult.ErrorMessages)
                    );
                    return;
                }

                // ===============================
                // REMOVE ORDER FROM QUEUE
                // ===============================
                await _orderConnector.RemoveOrder(nextOrder);

                _logger.LogInformation(
                    "Subscription {OrderId} processed successfully",
                    nextOrder.OrderId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subscription orders");
            }
        }
    }
}
