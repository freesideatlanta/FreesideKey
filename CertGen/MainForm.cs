using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using System.IO;


namespace CertGen
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            SHA256Radio.Checked = true;
            CNTextBox.Text = "Fetch Heavy Industries";
            StartDateTimePicker.Value = DateTime.Now;
            EndDateTimePicker.Value = DateTime.Now.AddYears(20);  //Valid for 20 Years
        }


        static void MakeRSACert(String CN, DateTime Start, DateTime End, String Password, HashAlgorithmName Hash, String OutputFilePrivate, String OutputFilePublic)
        {
            
            var rsa = RSA.Create(2048); // generate asymmetric key pair
            var req = new CertificateRequest("CN=" + CN, rsa, Hash, RSASignaturePadding.Pkcs1);

            req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DataEncipherment, false));
            req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));
            

            var cert = req.CreateSelfSigned(new DateTimeOffset(Start), new DateTimeOffset(End));
            
            // Create PFX (PKCS #12) with private key
            File.WriteAllBytes(OutputFilePrivate, cert.Export(X509ContentType.Pfx, Password));

            // Create Base 64 encoded CER (public key only)
            File.WriteAllText(OutputFilePublic,
                "-----BEGIN CERTIFICATE-----\r\n"
                + Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks)
                + "\r\n-----END CERTIFICATE-----");
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    
        private void SaveCertButton_Click(object sender, EventArgs e)
        {
            //Check Passwords MAtch
            if (PassTextBox.Text != PassConfirmTextBox.Text)
            {
                MessageBox.Show("Passwords Do Not Match", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (PassTextBox.Text == "")
            {
                DialogResult r = MessageBox.Show("Password is Blank. Press OK to Proceed", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (r != DialogResult.OK)
                    return;
            }

            if (MD5Radio.Checked)
            {
                DialogResult r = MessageBox.Show("MD5 Is A Terrible Choice. Are You Sure?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r != DialogResult.Yes)
                    return;
            }

            if (SHA1Radio.Checked)
            {
                DialogResult r = MessageBox.Show("SHA1 Is a bit Dated. Are You Sure?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r != DialogResult.Yes)
                    return;
            }

            //Private Key Location
            saveFileDialog.Title = "Private Key";
            saveFileDialog.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            saveFileDialog.Filter = "Private Key | *.pfx";
            saveFileDialog.FileName = "MyPrivateKey";

            DialogResult saveDlgResult;

            saveDlgResult = saveFileDialog.ShowDialog();
            if (saveDlgResult != DialogResult.OK)
                return;

            String privateFileName = saveFileDialog.FileName;

            String FileNameNoExt = privateFileName.LastIndexOf('\\') > 0 ? privateFileName.Substring(privateFileName.LastIndexOf('\\') + 1) : privateFileName;
            FileNameNoExt = FileNameNoExt.LastIndexOf('.') > 0 ? FileNameNoExt.Substring(0, FileNameNoExt.LastIndexOf('.')) : FileNameNoExt;

            //Public Key Location
            saveFileDialog.Title = "Public Key";
            saveFileDialog.Filter = "Public Key | *.cer";
            saveFileDialog.FileName = FileNameNoExt == "MyPrivateKey" ? "MyPublicKey" : FileNameNoExt+"Public";

            DialogResult saveDlgResult2 = saveFileDialog.ShowDialog();
            if (saveDlgResult != DialogResult.OK)
                return;

            String publicFileName = saveFileDialog.FileName;


            HashAlgorithmName Hash;

            if (SHA512Radio.Checked)
                Hash = HashAlgorithmName.SHA512;
            if (SHA384Radio.Checked)
                Hash = HashAlgorithmName.SHA384;
            if (SHA256Radio.Checked)
                Hash = HashAlgorithmName.SHA256;
            if (SHA1Radio.Checked)
                Hash = HashAlgorithmName.SHA1;
            if (MD5Radio.Checked)
                Hash = HashAlgorithmName.MD5;

            MakeRSACert(CNTextBox.Text, StartDateTimePicker.Value, EndDateTimePicker.Value, PassTextBox.Text, Hash, privateFileName, publicFileName);

            return;
        }

    }
}
