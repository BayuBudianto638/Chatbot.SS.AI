using Chatbot.SS.AI.EncryptionLib.Helpers;
using Chatbot.SS.AI.EncryptionLib.Models;
using FluentValidation;
using FluentValidation.Results;
using System.Text;

namespace Chatbot.SS.AI.EncryptionLib.Services
{
    public class EncryptionService
    {
        private readonly EncryptionValidator _validator;

        public EncryptionService()
        {
            _validator = new EncryptionValidator();
        }

        public string EncryptPassword(string password, string username)
        {
            var validationResult = ValidateInput(password, username);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            string key = GenerateSaltedKey(username);
            byte[] encrypted = DSESHelper.Encrypt(password, key);
            return Convert.ToBase64String(encrypted);
        }

        public string DecryptPassword(string encryptedpassword, string username)
        {
            var validationResult = ValidateInput(encryptedpassword, username);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            string key = GenerateSaltedKey(username);
            byte[] decryptedBytes = DSESHelper.Decrypt(Convert.FromBase64String(encryptedpassword), key);
            return Encoding.UTF8.GetString(decryptedBytes).TrimEnd('\0');
        }

        private string GenerateSaltedKey(string username)
        {
            return $"chatbot_{username}_secureKey";  
        }

        private ValidationResult ValidateInput(string password, string username)
        {
            var request = new EncryptionRequest { Password = password, Username = username };
            return _validator.Validate(request);
        }
    }
}
