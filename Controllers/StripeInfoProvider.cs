using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Stripe;
using Newtonsoft.Json;

namespace StripeApp.Controllers
{
    public class CardInfo
    {
        [JsonProperty("card_number")]
        public string CardNumber { get; set; }

        [JsonProperty("card_expiry_mm")]
        public string CardExpiryMonth { get; set; }

        [JsonProperty("card_expiry_yyyy")]
        public string CardExpiryYear { get; set; }

        [JsonProperty("card_cvc")]
        public string CardCVC { get; set; }
        
        [JsonProperty("amount")]
        public int Amount { get; set; }

    }
    public class ConfirmPaymentInfo
    {
        [JsonProperty("payment_intent_client_secret")]
        public string PaymentClientSecret { get; set; }
        [JsonProperty("redirect_url")]
        public string RedirectUrl { get; set; }

        [JsonProperty("payment_intent_id")]
        public string PaymentIntentId { get; set; }
    }
    public class StripeInfoProvider
    {
        public const string StripeConfigKey = "StripeSettings:SecretKey";
        public const string StripeConfigRedirectUrl = "StripeSettings:RedirectUrl";
        private IConfiguration configuration;
        public StripeInfoProvider(IConfiguration configuration){
            this.configuration = configuration;
        }
        public dynamic createPayment(CardInfo cardInfo){
            try
            {
                int paymentAmount = cardInfo.Amount;
                if (cardInfo.CardNumber != null)
                {
                    StripeConfiguration.ApiKey = GetEnvironmentConfigVar(StripeConfigKey, this.configuration.GetValue<string>(StripeConfigKey));
                    var options = new PaymentMethodCreateOptions
                    {
                        Type = "card",
                        Card = new PaymentMethodCardCreateOptions
                        {
                            Number = cardInfo.CardNumber,
                            ExpMonth = Convert.ToInt32(cardInfo.CardExpiryMonth),
                            ExpYear = Convert.ToInt32(cardInfo.CardExpiryYear),
                            Cvc = cardInfo.CardCVC,
                        },
                    };

                    var service = new PaymentMethodService();
                    var paymentMethod = service.Create(options);
                    PaymentIntent paymentIntentObj = createPaymentIntent(paymentMethod.Id,paymentAmount);
                    return new { payment_method_id = paymentMethod.Id,payment_intent_id= paymentIntentObj.Id ,status = paymentIntentObj.Status};
                    
                }else{
                     return new { customError = "Something is wrong. Pleas fill out all the information" };
                }

            }
            catch (StripeException e)
            {
                return  new { error = e.StripeError.Message };
            }
            return  new { customError = "Payment method not created" };
        }
        public dynamic confirmPayment(ConfirmPaymentInfo confirmPaymentInfo){
            string stripeRedirectUrl = String.IsNullOrEmpty(confirmPaymentInfo.RedirectUrl)? GetEnvironmentConfigVar(StripeConfigKey, this.configuration.GetValue<string>(StripeConfigRedirectUrl))+"/Home/ConfirmPayment":confirmPaymentInfo.RedirectUrl;
            var paymentIntentService = new PaymentIntentService();
            PaymentIntent paymentIntent = null;

            try
            {
                if (confirmPaymentInfo.PaymentIntentId != null)
                {
                    paymentIntent = paymentIntentService.Get(confirmPaymentInfo.PaymentIntentId);
                    if(paymentIntent.Status=="requires_payment_method"){
                        generatePaymentResponse(paymentIntent);
                    }else{
                        var confirmOptions = new PaymentIntentConfirmOptions { ReturnUrl = stripeRedirectUrl };
                        paymentIntent = paymentIntentService.Confirm(
                            confirmPaymentInfo.PaymentIntentId,
                            confirmOptions
                        );
                    }
                }
            }
            catch (StripeException e)
            {
                return new { error = e.StripeError.Message };
            }
            return generatePaymentResponse(paymentIntent);
        }
        private dynamic generatePaymentResponse(PaymentIntent intent)
        {
            // Note that if your API version is before 2019-02-11, 'requires_action'
            // appears as 'requires_source_action'.requires_payment_method
            if (intent.Status == "requires_action" && intent.NextAction.Type == "use_stripe_sdk")
            {
                // Tell the client to handle the action
                return new
                {
                    requires_action = true,
                    payment_intent_id = intent.Id,
                    intent = intent

                };
            }
            if(intent.Status == "requires_action" && intent.NextAction.Type == "redirect_to_url")
            {
                return new
                {
                    requires_action = true,
                    redirectUrl = intent.NextAction.RedirectToUrl,
                    payment_intent_id = intent.Id
                };
            }else if(intent.Status == "requires_payment_method"){
                // The payment didn’t need any additional actions and failed to authenticate using 3DSecure 2
                return new { error = "Payment failed" };
            }
            else if (intent.Status == "succeeded")
            {
                // The payment didn’t need any additional actions and completed successfully using 3DSecure 2
                return new { success = true };
            }
            else
            {
                // Invalid status
                return new { error = "Invalid PaymentIntent status" };
            }
        }
        private PaymentIntent createPaymentIntent(string paymentMethodId,int paymentAmount)
        {
            var paymentIntentService = new PaymentIntentService();
            PaymentIntent paymentIntent = null;
            if (paymentMethodId != null)
                {
                    // Create the PaymentIntent
                    var createOptions = new PaymentIntentCreateOptions
                    {
                        PaymentMethodId = paymentMethodId,
                        Amount = paymentAmount,
                        Currency = "usd",
                        ConfirmationMethod = "manual",
                        Confirm = true,
                    };
                    paymentIntent = paymentIntentService.Create(createOptions);

                }
            return paymentIntent;
        }
        private string GetEnvironmentConfigVar(string variableName,string defaultValue)
        {
            string variableValue = Environment.GetEnvironmentVariable(variableName);
            if (!String.IsNullOrEmpty(variableValue))
            {
                return variableValue;
            }
            return defaultValue;
        }
    }
}