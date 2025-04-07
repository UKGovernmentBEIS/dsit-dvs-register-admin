﻿using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using QRCoder;
using System.Text;

namespace DVSAdmin.CommonUtility
{
    public static class Helper
    {
        public static string GenerateQRCode(string secretKey, string email)
        {
            string qrCodeDataString = $"otpauth://totp/cognito-client:{email}?secret={secretKey}&issuer=DSIT-Platform";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeDataString, QRCodeGenerator.ECCLevel.Q);
            Base64QRCode qrCode = new Base64QRCode(qrCodeData);
            string qrCodeImageAsBase64 = qrCode.GetGraphic(20);

            return qrCodeImageAsBase64;
        }

        public static string GetLocalDateTime(DateTime? dateTime, string format)
        {
            DateTime dateTimeValue = Convert.ToDateTime(dateTime);
            TimeZoneInfo localTimeZone = TimeZoneInfo.Local; // Get local time zone
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeValue, localTimeZone); // Convert to local time
            string time = localTime.ToString(format);
            return time;
        }

        public static string ConcatenateKeyValuePairs(Dictionary<string, List<string>> data)
        {
            var result = new StringBuilder();

            foreach (var kvp in data)
            {
                result.Append(kvp.Key + ": ");
                if(kvp.Value.Count > 1) 
                {
                    string values = string.Join(",", kvp.Value);
                    result.Append(values);                   
                }
                else
                {
                    result.Append(kvp.Value[0]);
                }
                
                result.AppendLine();
            }

            return result.ToString();
        }
    }
}
