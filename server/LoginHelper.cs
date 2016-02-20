using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.FluentValidation.Internal;
using ServiceStack.Text;
using System.IO;

namespace MattermostCrmService
{
    [DataContract]
    public class LoginHelperCredential 
    {
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; }
    }
    public class LoginHelper
    {
        public static LoginHelper Instance { get; private set; }
        public static void Init(string key)
        {
            Instance = new LoginHelper(key); 
        }

        private readonly string m_key;
        private LoginHelper(string key)
        {
            m_key = key;
        }

        public string GenerateToken(string username, string password)
        {
            var login = new LoginHelperCredential() {Username = username, Password = password};
            return Encryption.AESThenHMAC.SimpleEncryptWithPassword(login.SerializeToString(), m_key);
        }

        public LoginHelperCredential ParseToken(string token)
        {
            return Encryption.AESThenHMAC.SimpleDecryptWithPassword(token, m_key).FromJson<LoginHelperCredential>();
        }
    }
}
