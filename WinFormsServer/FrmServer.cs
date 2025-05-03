using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Microsoft.AspNet.SignalR;
using System.ComponentModel;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Newtonsoft.Json;
using SgTest3.Model;
using System.Net.Http;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNet.SignalR.Hosting;
using iTextSharp.text.pdf.parser;
using SgTest3.Properties;
using iTextSharp.text.pdf;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;

//using static System.Net.WebRequestMethods;

namespace WinFormsServer
{
    public partial class FrmServer : Form
    {
        private IDisposable _signalR;
        private BindingList<ClientItem> _clients = new BindingList<ClientItem>();
        private BindingList<string> _groups = new BindingList<string>();

        public FrmServer()
        {
            SetStartup();
            InitializeComponent();
            //bindListsToControls();
            StartServer();
            //Register to static hub events
            SimpleHub.ClientConnected += SimpleHub_ClientConnected;
            SimpleHub.ClientDisconnected += SimpleHub_ClientDisconnected;
            SimpleHub.ClientNameChanged += SimpleHub_ClientNameChanged;
            SimpleHub.ClientJoinedToGroup += SimpleHub_ClientJoinedToGroup;
            SimpleHub.ClientLeftGroup += SimpleHub_ClientLeftGroup;
            SimpleHub.MessageReceived += SimpleHub_MessageReceived;
            // InitializeComponent();
            // this.Load += (s, e) => this.Hide();
            // this.ShowInTaskbar = false;
            // this.WindowState = FormWindowState.Minimized;

        }

        private void bindListsToControls()
        {
            //Clients list
            //cmbClients.DisplayMember = "Name";
            //cmbClients.ValueMember = "Id";
            //cmbClients.DataSource = _clients;

            //Groups list
            //cmbGroups.DataSource = _groups;
        }

        private void SimpleHub_ClientConnected(string clientId)
        {
            //Add client to our clients list
            this.BeginInvoke(new Action(() => _clients.Add(new ClientItem() { Id = clientId, Name = clientId })));

            writeToLog($"Client connected: {clientId}");
        }

        private void SimpleHub_ClientDisconnected(string clientId)
        {
            //Remove client from the list
            this.BeginInvoke(new Action(() =>
            {
                var client = _clients.FirstOrDefault(x => x.Id == clientId);
                if (client != null)
                    _clients.Remove(client);
            }));

            writeToLog($"Client disconnected: {clientId}");
        }

        private void SimpleHub_ClientNameChanged(string clientId, string newName)
        {
            //Update the client's name if it exists
            this.BeginInvoke(new Action(() =>
            {
                var client = _clients.FirstOrDefault(x => x.Id == clientId);
                if (client != null)
                    client.Name = newName;
            }));

            writeToLog($"Client name changed. Id: {clientId}");
            writeToLog($"Name: {newName}");
        }

        private void SimpleHub_ClientJoinedToGroup(string clientId, string groupName)
        {
            //Only add the groups name to our groups list
            this.BeginInvoke(new Action(() =>
            {
                var group = _groups.FirstOrDefault(x => x == groupName);
                if (group == null)
                    _groups.Add(groupName);
            }));

            writeToLog($"Client joined to group. Id: {clientId}, Group:{groupName}");
        }

        private void SimpleHub_ClientLeftGroup(string clientId, string groupName)
        {
            writeToLog($"Client left group. Id: {clientId}, Group:{groupName}");
        }

        private void SimpleHub_MessageReceived(string senderClientId, string message)
        {
            //One of the clients sent a message, log it
            
            

            this.BeginInvoke(new Action(() =>
            {
                string clientName = _clients.FirstOrDefault(x => x.Id == senderClientId)?.Name;
                try
                {
                    var resource = JsonConvert.DeserializeObject<SignInfoResource>(message);
                    writeToLog("");
                    writeToLog("---");
                    writeToLog("");
                    writeToLog($"{clientName}: Đã nhận được thông tin file");
                    #region Sign Code
                    SignAction(resource);
                    #endregion
                }
                catch (Exception ex)
                {
                    writeToLog($"{clientName}: Error - {ex.Message}");
                    var responseClient = new ResponseModel();
                    responseClient.Message = message;
                    responseClient.StatusCode = (int) HttpStatusCode.InternalServerError;
                    responseClient.Content = new ContentModel()
                    {
                        File = "",
                        Image = ""
                    };
                    responseClient.Size = 1;
                    responseClient.MeatadataDto = new MeatadataDto();
                    // responseClient.responseFailed = "Chuỗi ký tự không đúng, vui lòng kiểm tra lại!";
                    ResponseAction(JsonConvert.SerializeObject(responseClient));
                }
            }));
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            txtLog.Clear();

            StartServer();
        }

        private void StartServer ()
        {
            try
            {
                //Start SignalR server with the give URL address
                //Final server address will be "URL/signalr"
                //Startup.Configuration is called automatically
                _signalR = WebApp.Start<Startup>(txtUrl.Text);

                btnStartServer.Enabled = false;
                txtUrl.Enabled = false;
                btnStop.Enabled = true;
                //grpBroadcast.Enabled = true;

                writeToLog("Server started: Waiting connect...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopAction();
        }

        private void StopAction()
        {
            _clients.Clear();
            _groups.Clear();

            SimpleHub.ClearState();

            if (_signalR != null)
            {
                _signalR.Dispose();
                _signalR = null;

                btnStop.Enabled = false;
                btnStartServer.Enabled = true;
                txtUrl.Enabled = true;
                //grpBroadcast.Enabled = false;

                writeToLog("Server stopped.");
            }
        }

        /*private void btnSend_Click(object sender, EventArgs e)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<SimpleHub>();

            if (rdToAll.Checked)
            {
                hubContext.Clients.All.addMessage("SERVER", txtMessage.Text);
            }
            else if (rdToGroup.Checked)
            {
                hubContext.Clients.Group(cmbGroups.Text).addMessage("SERVER", txtMessage.Text);
            }
            else if (rdToClient.Checked)
            {
                hubContext.Clients.Client((string)cmbClients.SelectedValue).addMessage("SERVER", txtMessage.Text);
            }
        }*/
        
        private void ResponseAction(string message)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<SimpleHub>();

            hubContext.Clients.All.addMessage("SERVER", message);
        }

        private void writeToLog(string log)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new Action(() => txtLog.AppendText(log + Environment.NewLine)));
            else
                txtLog.AppendText(log + Environment.NewLine);
        }

        private async void SignAction(SignInfoResource resource)
        {
            //String _uri = "http://localhost:55356/";
            String _uri = ConfigurationManager.AppSettings["DGAPI"].ToString();
            String taxCodeVJAA= ConfigurationManager.AppSettings["TaxCodeVJAA"].ToString();
            String pdf = ConfigurationManager.AppSettings["PDF"].ToString();
            String word = ConfigurationManager.AppSettings["WORD"].ToString();
            String xml = ConfigurationManager.AppSettings["XML"].ToString();
            string folder = @"C:\OnlineSign\Signed\";
            var responseClient = new ResponseModel();
            // responseClient.isSuccess = false;
            responseClient.StatusCode = (int)HttpStatusCode.InternalServerError;
            
            try
            {
                //llx,lly,urx,ury-type-file
                //string loc = "";
                string filePath = "";
                string imagePath = "";

                Directory.CreateDirectory(folder);

                var cert = Utils.Utils.GetCertificate(); // or Convert to xml
                                                         //var cert = new X509Certificate2(@"F:\HDDT\MyCert.pfx");
                if (cert == null)
                {
                    responseClient.Message = "Không nhận diện được chữ kí số, vui lòng kiểm tra lại!";
                    ResponseAction(JsonConvert.SerializeObject(responseClient));
                    writeToLog("Không nhận diện được chữ kí số, vui lòng kiểm tra lại");
                    //throw new Exception("Không nhận diện được chữ kí số, vui lòng kiểm tra lại");
                }
                else
                {
                    string subject = cert.Subject;
                    List<string> subjects = subject.Split(',').ToList();
                    //var mstSubject = subjects.Where(_ => _.Contains("MST")).FirstOrDefault();
                    //var mst = mstSubject.Substring(mstSubject.IndexOf("MST") + 4).Trim();

                    #region Check usb token and sign

                    //if (mst.Equals(taxCodeVJAA))
                    if (true) //tạm chưa cần check dùng đúng chữ ký chưa
                    {
                        var content = new StringContent("");
                        HttpResponseMessage result = null;
                        using (var client = new HttpClient())
                        // using (var content = new StringContent(""))

                        {
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                            client.BaseAddress = new Uri(_uri);
                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", resource.token);
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            result = await client.GetAsync($"api/Document/view-document-for-usb?documentId={resource.documentId}");
                            var responseBody = new ResponseModel();
                            
                            if (result.IsSuccessStatusCode)
                            {
                                var test = await result.Content.ReadAsStringAsync();
                                responseBody = JsonConvert.DeserializeObject<ResponseModel>(test);
                                if (responseBody.StatusCode == 200)
                                {
                                    try
                                    {
                                        filePath = ConvertBase64ToPDF(responseBody.Content.File, folder);
                                        imagePath = ConvertBase64ToPng(responseBody.Content.Image, folder);
                                        result = null;
                                        var uploadFile = new UploadFileResource();
                                        //var responseFile = new FileDownloadResponse();
                                        switch ("pdf".Trim().ToUpper())
                                        {
                                            case var value when value == pdf:
                                                uploadFile = Utils.Utils.SignWithThisCert(cert, filePath, resource, resource.page,imagePath);
                                                // content.Add(uploadFile.File, "File", uploadFile.fileName);
                                                var optionsJson = JsonConvert.SerializeObject(
                                                    uploadFile.File,
                                                    new JsonSerializerSettings()
                                                    );
                                                var root = new JObject
                                                {
                                                    ["file"] = uploadFile.File,
                                                    ["image"] = ""
                                                };
                                                string jsonBody = root.ToString(Formatting.Indented);

            
                                                content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                                                break;
                                            
                                            case var value when value == xml:
                                                //content.Add(Utils.Utils.SignXmlWithCert(cert, filePath), "File");
                                                break;
                                            //case var value when value == word:
                                            //   Utils.Utils.SignWithThisCert(cert, filePath, int.Parse(locs[0]), int.Parse(locs[1]), int.Parse(locs[2]), int.Parse(locs[3]), page);
                                            //    break;
                                            default:
                                                responseClient.Message = "Chưa hổ trợ loại file này!";
                                                ResponseAction(JsonConvert.SerializeObject(responseClient));
                                                writeToLog("Chưa hổ trợ loại file này");
                                                break;
                                        }
                                        result = await client.PostAsync($"api/Document/update-document-from-usb/{resource.documentId}", content);
                                        var tst = await result.Content.ReadAsStringAsync();
                                        // responseBody = JsonConvert.DeserializeObject<ResponseModel>(await result.Content.ReadAsStringAsync());
                                        if (result.IsSuccessStatusCode)
                                        {
                                            // responseClient.isSuccess = true;
                                            responseClient.StatusCode = 200;
                                            responseClient.Message = "Ký thành công!";
                                            ResponseAction(JsonConvert.SerializeObject(responseClient));
                                            writeToLog("Ký thành công");
                                        }
                                        else
                                        {
                                            //responseBody = JsonConvert.DeserializeObject<FileDownloadResponse>(await result.Content.ReadAsStringAsync());
                                            responseClient.StatusCode = 500;
                                            responseClient.Message = "Tải lên hệ thống thất bại!";
                                            ResponseAction(JsonConvert.SerializeObject(responseClient));
                                            writeToLog("Tải lên hệ thống thất bại: " + tst);
                                        }

                                        #region Delete file after sent success
                                        if (File.Exists(filePath))
                                        {
                                            File.Delete(filePath);
                                        }
                                        if (File.Exists(uploadFile.fileName))
                                        {
                                            File.Delete(uploadFile.fileName);
                                        }
                                        #endregion
                                    }
                                    catch (Exception ex)
                                    {
                                        responseClient.Message = "Giải nén file thất bại!";
                                        ResponseAction(JsonConvert.SerializeObject(responseClient));
                                        writeToLog($"Giải nén file thất bại: {ex.Message}");
                                    }
                                }
                                else
                                {
                                    responseClient.Message = "Download file thất bại!";
                                    ResponseAction(JsonConvert.SerializeObject(responseClient));
                                    writeToLog($"Download file thất bại: {responseBody.Message}");
                                }
                            }
                            else
                            {
                                responseClient.Message = "Kết nối server thất bại!";
                                ResponseAction(JsonConvert.SerializeObject(responseClient));
                                writeToLog("Kết nối server thất bại");
                            }
                            
                            
                        }

                    }
                    else
                    {
                        //responseClient.Message = $"Bạn đang dùng sai chữ ký! MST: {mst}!";
                        ResponseAction(JsonConvert.SerializeObject(responseClient));
                        //writeToLog($"Bạn đang dùng sai chữ ký! MST: {mst}");
                        //MessageBox.Show($"Bạn đang dùng sai chữ ký! MST: {mst}");
                    }

                    #endregion
                    
                }

            }
            catch (Exception ex)
            {
                responseClient.Message = $"Lỗi phần mềm!";
                ResponseAction(JsonConvert.SerializeObject(responseClient));
                writeToLog(ex.Message);
                //MessageBox.Show(ex.Message);
            }
            //return "Done";
        }

        private string ConvertBase64ToPDF(string reource, string folder)
        {
            string fileName = DateTime.Now.ToString("ddMMyyyyHHmmss");
            string path = folder + fileName + ".pdf";
            File.WriteAllBytes(path, Convert.FromBase64String(reource));
            return path;
        }
        private string ConvertBase64ToPng(string reource, string folder)
        {
            string fileName = DateTime.Now.ToString("ddMMyyyyHHmmss");
            string path = folder + fileName + ".png";
            File.WriteAllBytes(path, Convert.FromBase64String(reource));
            return path;
        }

        private void FrmServer_MinimumSizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                if (DigitalSignNI.Visible != true) DigitalSignNI.Visible = true;
            }
        }

        /*private void FrmServer_FormClosed(object sender, FormClosedEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }*/

        private void FrmServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(DigitalSignNI.Visible != true) DigitalSignNI.Visible = true;
            e.Cancel = true;
            this.Hide();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenWindow();
        }

        private void MenuItemClose_Click(object sender, EventArgs e)
        {
            //StopAction();
            //DigitalSignNI.Visible = false;
            Application.ExitThread();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWindow();
        }

        private void OpenWindow()
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            DigitalSignNI.Visible = false;
        }

        private void SetStartup()
        {
            // try
            //     {
            //         
            //         string keys =
            //         @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run";
            //         string value = "Automated";
            //         string path = Application.ExecutablePath;
            //         if (Registry.GetValue(keys, value, null) == null)
            //         {
            //             // if key doesn't exist
            //             using (RegistryKey key =
            //             Registry.CurrentUser.OpenSubKey
            //             ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            //             {
            //                 
            //                 key.SetValue("Automated", path);
            //                 key.Dispose();
            //                 key.Flush();
            //             }
            //         }
            //         else
            //         {
            //             //if key Exist
            //         }
            //     }
            //     catch (Exception ex)
            //     {
            //         throw(ex);
            //     }
        }
    }
}
