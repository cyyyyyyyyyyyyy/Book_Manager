using System;
using System.Runtime.Serialization;

namespace fileManager2022
{
    [Serializable] internal class AccountInfo
    {
        private string login;
        private string password;
        internal AccountInfo() { login = ""; password = ""; }
        internal bool SetLogin(string login)
        {
            if (login.Length > 2)
            {
                this.login = login;
                return true;
            }
            return false;
        }
        internal bool SetPassword(string pass)
        {
            if (pass.Length > 5)
            {
                this.password = pass;
                return true;
            }
            return false;
        }
        internal string GetLogin() => login;
        internal string GetPassword() => password;

        [OnSerializing]
        internal void OnSerializing(StreamingContext context)
        {
            login = Security.Encrypt(login);
            password = Security.Encrypt(password);
        }
        [OnDeserializing]
        internal void OnDeserializing(StreamingContext context)
        {
            /*login = Security.Decrypt(login);
            password = Security.Decrypt(password);*/
        }
    }
}
