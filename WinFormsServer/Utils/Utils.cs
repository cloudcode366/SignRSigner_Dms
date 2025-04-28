using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http.Headers;
using Microsoft.Win32;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Text;
using System.Security.Cryptography;
using System;
using Org.BouncyCastle.X509;
using iTextSharp.text.pdf.security;
using iTextSharp.text.pdf;
using SgTest3.Model;

namespace WinFormsServer.Utils
{
    public class Utils
    {
        //id-llx,lly,urx,ury
        const string fileNew = @"C:\OnlineSign\Signed\signed-{0}";
        public static X509Certificate2 GetCertificate()
        {
            X509Store userCaStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                userCaStore.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certificatesInStore = userCaStore.Certificates;
                //X509Certificate2Collection findResult = certificatesInStore.Find(X509FindType.FindBySubjectName, "C=VN", false);
                X509Certificate2Collection findResult = certificatesInStore;
                X509Certificate2 clientCertificate = null;
                if (findResult.Count > 0)
                {
                    foreach(var i in findResult)
                    {
                        if (i.Subject.Contains("C=VN"))
                        {
                            clientCertificate = i;
                            break;
                        }
                    }
                    //clientCertificate = findResult[0];
                }
                else
                {
                    throw new Exception("Không nhận diện được chữ kí số, vui lòng kiểm tra lại");
                }
                return clientCertificate;
            }
            catch
            {
                throw;
            }
            finally
            {
                userCaStore.Close();
            }
        }

        /*public static ByteArrayContent SignWithThisCert(X509Certificate2 cert, List<string> fileInputPaths, string type)
        {

            //string SourcePdfFileName = fileInputPath;
            List<string> fileName = new List<string>();

            foreach(var s in fileInputPaths)
            {

            }

            string DestPdfFileName = String.Format(fileNew, fileName);
            Org.BouncyCastle.X509.X509CertificateParser cp = new Org.BouncyCastle.X509.X509CertificateParser();
            Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[] { cp.ReadCertificate(cert.RawData) };
            IExternalSignature externalSignature = new X509Certificate2Signature(cert, "SHA-1");
            PdfReader pdfReader = new PdfReader(SourcePdfFileName);
            //PdfReader pdfReader = new PdfReader(streamFile);
            FileStream signedPdf = new FileStream(DestPdfFileName, FileMode.Create);  //the output pdf file
            PdfStamper pdfStamper = PdfStamper.CreateSignature(pdfReader, signedPdf, '\0');
            PdfSignatureAppearance signatureAppearance = pdfStamper.SignatureAppearance;
            //signatureAppearance.SetVisibleSignature("Signature2");
            signatureAppearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.DESCRIPTION;
            //signatureAppearance.Layer2Text = "Được ký bởi" + cert.GetName().ToString();
            switch (type)
            {
                case "Invoice": //fixed for new invoice template
                    signatureAppearance.SetVisibleSignature(new iTextSharp.text.Rectangle(425, 100, 575, 155), pdfReader.NumberOfPages, null);
                    break;
                case "Sheet":
                    signatureAppearance.SetVisibleSignature(new iTextSharp.text.Rectangle(650, 520, 820, 580), 1, null);
                    break;
                case "Correspondence":
                    signatureAppearance.SetVisibleSignature(new iTextSharp.text.Rectangle(372, 435, 527, 505), pdfReader.NumberOfPages, null);
                    break;
            }
            BaseFont unicode =
                        BaseFont.CreateFont("c:/windows/fonts/times.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            signatureAppearance.Layer2Font = new iTextSharp.text.Font(unicode);
            MakeSignature.SignDetached(signatureAppearance, externalSignature, chain, null, null, null, 0, CryptoStandard.CMS);

            return new ByteArrayContent(ReadFully(fileName));
        }*/

        public static UploadFileResource SignWithThisCert(X509Certificate2 cert, string fileInputPath, SignInfoResource resource, int? page)
        {
            string SourcePdfFileName = fileInputPath;
            var fileName = Path.GetFileName(SourcePdfFileName);
            string DestPdfFileName = String.Format(fileNew, fileName);
            Org.BouncyCastle.X509.X509CertificateParser cp = new Org.BouncyCastle.X509.X509CertificateParser();
            Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[] { cp.ReadCertificate(cert.RawData) };
            IExternalSignature externalSignature = new X509Certificate2Signature(cert, "SHA-1");
            PdfReader pdfReader = new PdfReader(SourcePdfFileName);
            //PdfReader pdfReader = new PdfReader(streamFile);
            FileStream signedPdf = new FileStream(DestPdfFileName, FileMode.Create);  //the output pdf file
            PdfStamper pdfStamper = PdfStamper.CreateSignature(pdfReader, signedPdf, '\0');
            PdfSignatureAppearance signatureAppearance = pdfStamper.SignatureAppearance;
            //signatureAppearance.SetVisibleSignature("Signature2");
            signatureAppearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.DESCRIPTION;
            //signatureAppearance.Layer2Text = "Được ký bởi" + cert.GetName().ToString();

            signatureAppearance.SetVisibleSignature(new iTextSharp.text.Rectangle(resource.llx, resource.lly, resource.urx, resource.ury), page ?? pdfReader.NumberOfPages, null);
            //signatureAppearance.SetVisibleSignature(new iTextSharp.text.Rectangle(425, 100, 575, 155), page ?? pdfReader.NumberOfPages, resource.searchText);
            BaseFont unicode =
                        BaseFont.CreateFont("c:/windows/fonts/times.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            signatureAppearance.Layer2Font = new iTextSharp.text.Font(unicode);
            MakeSignature.SignDetached(signatureAppearance, externalSignature, chain, null, null, null, 0, CryptoStandard.CMS);

            return new UploadFileResource
            {
                fileName = fileName,
                File = ReadFully(fileName)
            };
        }

        public static UploadFileResource SignXmlWithCert(X509Certificate2 cert, string fileInputPath)
        {
            string SourcePdfFileName = fileInputPath;
            //Change
            SourcePdfFileName = SourcePdfFileName.Substring(0, SourcePdfFileName.Length - 4);
            SourcePdfFileName += ".xml";
            var fileName = Path.GetFileName(SourcePdfFileName);
            string DestPdfFileName = String.Format(fileNew, fileName);
            //Org.BouncyCastle.X509.X509CertificateParser cp = new Org.BouncyCastle.X509.X509CertificateParser();
            //Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[] { cp.ReadCertificate(cert.RawData) };
            //IExternalSignature externalSignature = new X509Certificate2Signature(cert, "SHA-1");
            //Sign XML on local
            XmlDocument data = new XmlDocument();
            data.Load(SourcePdfFileName);
            File.WriteAllText(DestPdfFileName, data.OuterXml, Encoding.UTF8);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(File.ReadAllText(DestPdfFileName));
            SignedXmlWithId signedXml = new SignedXmlWithId(doc);
            signedXml.SigningKey = cert.PrivateKey;
            Reference reference = new Reference();


            //data
            reference.Uri = "#data";
            reference.DigestMethod = @"http://www.w3.org/2000/09/xmldsig#sha1";
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(reference);

            //sign date
            //TIMESTAMP
            XmlElement signaturePropertiesRoot = doc.CreateElement("SignatureProperties", "http://www.w3.org/2000/09/xmldsig#");
            DataObject signatureProperties = new DataObject();
            signatureProperties.Id = "idTimeStamp";
            signatureProperties.Data = signaturePropertiesRoot.SelectNodes(".");
            signedXml.AddObject(signatureProperties);

            // and add a reference to the data object
            Reference propertiesRef = new Reference();
            propertiesRef.Uri = "#idTimeStamp";
            propertiesRef.Type = "http://www.w3.org/2000/09/xmldsig#SignatureProperties";
            signedXml.AddReference(propertiesRef);

            XmlElement property = doc.CreateElement("SignatureProperty", "http://www.w3.org/2000/09/xmldsig#");
            //property.SetAttribute("Id", "TamperSealer01TimeStamp");
            //property.SetAttribute("Target", "#" + signedXml.Signature.Id);
            property.SetAttribute("Target", "#idTimeStamp");
            signaturePropertiesRoot.AppendChild(property);

            XmlElement timestamp = doc.CreateElement("SigningTime", "http://www.w3.org/2000/09/xmldsig#");
            //timestamp.SetAttribute("DateTime", String.Format("{0:s}Z", DateTime.Now.ToUniversalTime()));
            timestamp.InnerText = DateTime.Now.ToString("s");
            property.AppendChild(timestamp);
            ///////////////////

            KeyInfo keyInfo = new KeyInfo();
            var kiData = new KeyInfoX509Data(cert);
            kiData.AddSubjectName(cert.Subject);
            keyInfo.AddClause(kiData);
            signedXml.KeyInfo = keyInfo;
            signedXml.ComputeSignature();
            //doc.SelectNodes("");
            XmlElement xmlSig = signedXml.GetXml();
            //doc.DocumentElement.AppendChild(doc.ImportNode(xmlSig, true));
            //doc.SelectSingleNode("/HDon/DSCKS/NBan").ReplaceChild(doc.ImportNode(xmlSig, true), doc.SelectSingleNode("/HDon/DSCKS/NBan/Signature"));
            doc.SelectSingleNode("/TKhai/DSCKS/NNT").RemoveAll();
            doc.SelectSingleNode("/TKhai/DSCKS/NNT").AppendChild(doc.ImportNode(xmlSig, true));
            /*
            switch (type)
            {
                case "RegisterInvoice":
                    doc.SelectSingleNode("/TKhai/DSCKS/NNT").RemoveAll();
                    doc.SelectSingleNode("/TKhai/DSCKS/NNT").AppendChild(doc.ImportNode(xmlSig, true));
                    break;
                case "ErrorMessage":
                    doc.SelectSingleNode("/TBao/DSCKS/NNT").RemoveAll();
                    doc.SelectSingleNode("/TBao/DSCKS/NNT").AppendChild(doc.ImportNode(xmlSig, true));
                    break;
                case "Sumary":
                    doc.SelectSingleNode("/BTHDLieu/DSCKS/NNT").RemoveAll();
                    doc.SelectSingleNode("/BTHDLieu/DSCKS/NNT").AppendChild(doc.ImportNode(xmlSig, true));
                    break;
                case "Invoice":
                    doc.SelectSingleNode("/HDon/DSCKS/NBan").RemoveAll();
                    doc.SelectSingleNode("/HDon/DSCKS/NBan").AppendChild(doc.ImportNode(xmlSig, true));
                    break;
            }
            */

            /*if(type.Equals("RegisterInvoice") || type.Equals("ErrorMessage"))
            {
                doc.SelectSingleNode("/TKhai/DSCKS/NNT").AppendChild(doc.ImportNode(xmlSig, true));
            } else
            {
                doc.SelectSingleNode("/HDon/DSCKS/NBan").AppendChild(doc.ImportNode(xmlSig, true));
            }*/

            //doc.SelectSingleNode("/HDon/DSCKS/NMua").ReplaceChild(doc.ImportNode(xmlSig, true), doc.SelectSingleNode("/HDon/DSCKS/NMua/Signature"));
            File.WriteAllText(DestPdfFileName, doc.OuterXml, Encoding.UTF8);
            //doc.Save(DestPdfFileName);
            //return file

            return new UploadFileResource
            {
                fileName = fileName,
                File = ReadFully(fileName)
            };
        }

        /*public static byte[] ReadFully(String fileName)
        {
            using (FileStream fileStream = File.Open(String.Format(fileNew, fileName), FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    return ms.ToArray();
                }
            }
        }*/

        public static FileStream ReadFully(String fileName)
        {
            /*byte[] bytes;
            using (FileStream fileStream = File.Open(String.Format(fileNew, fileName), FileMode.Open, FileAccess.Read))
            {
                bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, (int)fileStream.Length);
            }*/
            FileStream fileStream = File.Open(String.Format(fileNew, fileName), FileMode.Open, FileAccess.Read);
            return fileStream;
        }

        /// <summary>
        /// Đăng ký Registry để sử dụng URL protocol
        /// </summary>
        /// <param name="appPath">Đường dẫn app</param>
        /// <param name="protocolName">Tên protocol - thường là tên app</param>
        static void RegisterProtocol(string appPath, string protocolName)
        {
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(protocolName); // app protocol subkey

            if (key == null) // register if not
            {
                key = Registry.ClassesRoot.CreateSubKey(protocolName); // app protocol subkey
                key.SetValue(string.Empty, "URL: " + protocolName + " Protocol");
                key.SetValue("URL Protocol", string.Empty);

                key = key.CreateSubKey(@"shell\open\command");
                key.SetValue(string.Empty, appPath + " " + "%1");
                // %1 - param
            }

            key.Close();
        }

        /// <summary>
        /// Lấy token từ server và lưu xuống file text
        /// </summary>
        /// <param name="baseAddress">địa chỉ server ex: (http://localhost:55356)</param>
        /// <param name="username">tên đăng nhập</param>
        /// <param name="password">mật khẩu</param>
        /// <returns>chuỗi token</returns>
        static string GetToken(string baseAddress, string username, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                // api domain
                client.BaseAddress = new Uri(baseAddress);

                // request headers
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                // request body
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password)
                });

                // send request
                var response = client.PostAsync("Token", content).Result;

                // response content
                var json = response.Content.ReadAsStringAsync().Result;

                // get token from response
                if (response.IsSuccessStatusCode)
                {
                    json = json.Replace("{", "");
                    json = json.Replace("}", "");
                    json = json.Replace("\"", "");
                    var rs = json.Split(':', ',');

                    var token = rs[1];
                    var expiredTime = rs[5];

                    // Cần folder token đã được tạo sẵn
                    string resultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token/token.txt");

                    // lưu token xuống file text
                    if (!File.Exists(resultPath))
                    {
                        File.Create(resultPath);
                        TextWriter tw = new StreamWriter(resultPath);
                        tw.WriteLine(token);
                        tw.Write(expiredTime);
                        tw.Close();
                    }
                    else
                    {
                        TextWriter tw = new StreamWriter(resultPath);
                        tw.WriteLine(token);
                        tw.Write(expiredTime);
                        tw.Close();
                    }

                    return token;
                }

                return null;
            }
        }

        /// <summary>
        /// Đọc chuỗi token được lưu từ file text
        /// </summary>
        /// <returns></returns>
        static string ReadToken()
        {
            try
            {
                string resultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token/token.txt");
                TextReader tr = new StreamReader(resultPath);
                // chuỗi token
                var token = tr.ReadLine();
                // thời gian hết hạn
                var expiredTime = double.Parse(tr.ReadLine());
                // last modified
                DateTime modifiedDate = File.GetLastWriteTime(resultPath);
                // Ngày hết hạn
                DateTime expiredDate = modifiedDate.Add(TimeSpan.FromSeconds(expiredTime));

                // kiểm tra thời gian hết hạn
                if (expiredDate < DateTime.Now)
                {
                    return null;
                }

                return token;
            }
            catch (Exception)
            {
                return null;
            }

        }

        // Đọc và ghi file PDF từ server
        static void PDFRead(string baseAddress)
        {
            string token = ReadToken();

            if (string.IsNullOrEmpty(token))
            {
                token = GetToken(baseAddress, "LD", "Aa123456");
            }

            PdfReader reader = null;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync("/api/File/GetInvoicePDF?ERPKey=A010000000112848").Result;
                if (response.IsSuccessStatusCode)
                {
                    reader = new PdfReader(response.Content.ReadAsStreamAsync().Result);
                }
            }

            string resultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/pdffile.pdf");
            PdfStamper stamper = new PdfStamper(reader, new FileStream(resultPath, FileMode.Create));
            stamper.Close();
            reader.Close();
        }
    }

    public class SignedXmlWithId : SignedXml
    {
        public SignedXmlWithId(XmlDocument xml) : base(xml)
        {
        }

        public SignedXmlWithId(XmlElement xmlElement)
            : base(xmlElement)
        {
        }

        public override XmlElement GetIdElement(XmlDocument doc, string id)
        {
            // check to see if it's a standard ID reference
            XmlElement idElem = base.GetIdElement(doc, id);

            if (idElem == null)
            {
                XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
                nsManager.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

                idElem = doc.SelectSingleNode("//*[@wsu:Id=\"" + id + "\"]", nsManager) as XmlElement;
            }

            return idElem;
        }
    }
}
