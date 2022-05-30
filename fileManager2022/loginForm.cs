using System;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace fileManager2022
{
    public partial class loginForm : Form
    {
        private AccountInfo accountInfo;
        public loginForm()
        {
            InitializeComponent();

            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream("auth.dat", FileMode.Open, FileAccess.Read, FileShare.None);
                accountInfo = (AccountInfo)formatter.Deserialize(stream);
                stream.Close();

                accountInfo.SetLogin(Security.Decrypt(accountInfo.GetLogin()));
                accountInfo.SetPassword(Security.Decrypt(accountInfo.GetPassword()));
            }
            catch (Exception ex)
            {
                accountInfo = new AccountInfo();
            }
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            if ((!accountInfo.GetLogin().Equals("")) && (!accountInfo.GetPassword().Equals("")))
            {
                if ((accountInfo.GetLogin().Equals(logInBox.Text)) && (accountInfo.GetPassword().Equals(passBox.Text)))
                {
                    this.Hide();
                    new Form1().Show();
                }
                else MessageBox.Show("Incorrect login or password");
            } else { MessageBox.Show("No user signed up"); }
        }

        private void signUpButton_Click(object sender, EventArgs e)
        {
            if ((accountInfo.SetLogin(logInBox.Text)) && (accountInfo.SetPassword(passBox.Text)))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                Stream stream = new FileStream("auth.dat", FileMode.Create, FileAccess.Write, FileShare.None);
                binaryFormatter.Serialize(stream, accountInfo);
                stream.Close();

                accountInfo.SetLogin(Security.Decrypt(accountInfo.GetLogin()));
                accountInfo.SetPassword(Security.Decrypt(accountInfo.GetPassword()));
            }
            else MessageBox.Show("Login or password are too short", "Incorrect login or password");
        }
    }
}
