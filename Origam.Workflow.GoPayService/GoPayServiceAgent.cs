#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;

using GoPay.api;

namespace Origam.Workflow.GoPayService
{
    /// <summary>
    /// Summary description for GoPayServiceAgent.
    /// </summary>
    public class GoPayServiceAgent : AbstractServiceAgent
    {
        public GoPayServiceAgent()
        {
            GopayConfig.Prostredi = GopayConfig.Environment.PRODUCTION;
        }

        private long CreatePayment(decimal goPayId,
            string productName,
            decimal totalPrice,
            string paymentReference,
            string failedUrl,
            string successUrl,
            string goPayKey,
            string paymentChannelsCsv,
            string firstName,
            string lastName,
            string city,
            string street,
            string postalCode,
            string countryCode,
            string email,
            string phoneNumber
            )
        {
            string[] paymentChannels = paymentChannelsCsv.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            long paymentSessionId = GoPay.api.GopayHelperWS.CreatePayment(Convert.ToInt64(goPayId),
                productName,
                Convert.ToInt64(totalPrice * 100),
                "CZK",
                paymentReference,
                successUrl,
                failedUrl,
                paymentChannels,
                paymentChannels[0],
                goPayKey,
                firstName,
                lastName,
                city,
                street,
                postalCode,
                countryCode,
                email,
                phoneNumber, null, null, null, null, null);

            return paymentSessionId;
        }

        private string GetPaymentGatewayUrl(
            decimal goPayId,
            long paymentSessionId,
            string goPayKey
            )
        {
            string ecryptedSignature = GopayHelper.Encrypt(
                GopayHelper.Hash(
                    GopayHelper.ConcatPaymentSession(
                        Convert.ToInt64(goPayId),
                        paymentSessionId,
                        goPayKey)
                ), goPayKey);

            // Presmerovani na platebni branu
            return GopayConfig.FullIntegrationUrl +
                   "?sessionInfo.targetGoId=" + Convert.ToInt64(goPayId).ToString() +
                   "&sessionInfo.paymentSessionId=" + paymentSessionId +
                   "&sessionInfo.encryptedSignature=" + ecryptedSignature;
        }

        private bool CheckPaymentIdentity(long returnedGoPayId,
            long returnedPaymentSessionId, string returnedVariableSymbol, 
            string encryptedSignature, decimal goPayId, string variableSymbol, 
            string goPayKey)
        {
            return GopayHelper.CheckPaymentIdentity(returnedGoPayId, 
                returnedPaymentSessionId, null, returnedVariableSymbol, 
                encryptedSignature, Convert.ToInt64(goPayId), variableSymbol, goPayKey);
                
        }

        private string IsPaymentDone(long paymentSessionId, decimal goPayId,
            string variableSymbol, decimal totalPrice,
            string productName, string goPayKey)
        {
            CallbackResult result = GopayHelperWS.IsPaymentDone(paymentSessionId, Convert.ToInt64(goPayId),
                variableSymbol, Convert.ToInt64(totalPrice * 100), "CZK",
            productName, goPayKey);
            return result.sessionState ?? "" + " " + result.sessionSubState ?? "" + " " + result.description ?? "";
        }

        #region IServiceAgent Members

        private object _result;
        public override object Result
        {
            get
            {
                object temp = _result;
                _result = null;

                return temp;
            }
        }

        public override void Run()
        {
            switch (this.MethodName)
            {
                case "CreatePayment":
                    // Check input parameters
                    if (!(this.Parameters["GoPayId"] is decimal))
                        throw new InvalidCastException("GoPayId has to be decimal data type.");

                    if (!(this.Parameters["TotalPrice"] is decimal))
                        throw new InvalidCastException("TotalPrice has to be decimal data type.");

                    _result = this.CreatePayment(
                        (decimal)this.Parameters["GoPayId"],
                        (string)this.Parameters["ProductName"],
                        (decimal)this.Parameters["TotalPrice"],
                        (string)this.Parameters["PaymentReference"],
                        (string)this.Parameters["FailedUrl"],
                        (string)this.Parameters["SuccessUrl"],
                        (string)this.Parameters["GoPayKey"],
                        (string)this.Parameters["PaymentChannelsCsv"],
                        (string)this.Parameters["FirstName"],
                        (string)this.Parameters["LastName"],
                        (string)this.Parameters["City"],
                        (string)this.Parameters["Street"],
                        (string)this.Parameters["PostalCode"],
                        (string)this.Parameters["CountryCode"],
                        (string)this.Parameters["Email"],
                        (string)this.Parameters["PhoneNumber"]
                        );
                    break;

                case "PaymentGatewayUrl":
                    // Check input parameters
                    if (!(this.Parameters["GoPayId"] is decimal))

                        throw new InvalidCastException("GoPayId has to be decimal data type.");

                    long paymentSessionId = 0;

                    if (!(this.Parameters["PaymentSessionId"] is long))
                        if (! (this.Parameters["PaymentSessionId"] is string && long.TryParse((string)this.Parameters["PaymentSessionId"], out paymentSessionId)))
                        {
                            throw new InvalidCastException("PaymentSessionId has to be long data type.");
                        }

                    _result = this.GetPaymentGatewayUrl(
                        (decimal)this.Parameters["GoPayId"],
                        (long)paymentSessionId,
                        (string)this.Parameters["GoPayKey"]
                        );
                    break;

                case "CheckPaymentIdentity":
                    // Check input parameters
                    if (this.Parameters["GoPayId"] == null)
                        throw new InvalidCastException("GoPayId cannot be empty");

                    if (this.Parameters["ReturnedGoPayId"] == null)
                        throw new InvalidCastException("ReturnedGoPayId cannot be empty.");

                        if (this.Parameters["ReturnedPaymentSessionId"] == null)
                        throw new InvalidCastException("ReturnedPaymentSessionId cannot be empty");

                    _result = this.CheckPaymentIdentity(
                        Convert.ToInt64(this.Parameters["ReturnedGoPayId"]),
                        Convert.ToInt64(this.Parameters["ReturnedPaymentSessionId"]),
                        (string)this.Parameters["ReturnedVariableSymbol"],
                        (string)this.Parameters["ReturnedEncryptedSignature"],
                        Convert.ToInt64(this.Parameters["GoPayId"]),
                        (string)this.Parameters["VariableSymbol"],
                        (string)this.Parameters["GoPayKey"]
                        );
                    break;

                case "IsPaymentDone":
                    // Check input parameters
                    if (Parameters["GoPayId"] == null)
                        throw new InvalidCastException("GoPayId has to be decimal data type.");

                    if (Parameters["PaymentSessionId"] == null)
                        throw new InvalidCastException("PaymentSessionId has to be string data type.");

                    if (Parameters["TotalPrice"] == null)
                        throw new InvalidCastException("TotalPrice has to be decimal data type.");

                    _result = this.IsPaymentDone(
                        Convert.ToInt64(this.Parameters["PaymentSessionId"]),
                        Convert.ToInt64(this.Parameters["GoPayId"]),
                        (string)this.Parameters["VariableSymbol"],
                        (decimal)this.Parameters["TotalPrice"],
                        (string)this.Parameters["ProductName"],
                        (string)this.Parameters["GoPayKey"]
                        );
                    break;

                default:
                    throw new ArgumentOutOfRangeException("MethodName", this.MethodName, ResourceUtils.GetString("InvalidMethodName"));
            }
        }

        #endregion
    }
}
