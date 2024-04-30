using QRCoder;

namespace DVSAdmin.CommonUtility
{
    public static class Helper
    {
        public static string GenerateQRCode(string secretKey, string email)
        {
            string qrCodeDataString = $"otpauth://totp/cognito-client:{email}?secret={secretKey}&issuer=OfDIA-Platform";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeDataString, QRCodeGenerator.ECCLevel.Q);
            Base64QRCode qrCode = new Base64QRCode(qrCodeData);
            string qrCodeImageAsBase64 = qrCode.GetGraphic(20);

            return qrCodeImageAsBase64;
        }
    }
}
