using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Live;
using System.IO;

namespace SkydriveTest
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        LiveConnectClient client = null;
        string skyDriveFolderID = string.Empty;
        string skyDriveFolderName = "SkyDriveTestApp";

        private void SignInButton_SessionChanged(object sender, Microsoft.Live.Controls.LiveConnectSessionChangedEventArgs e)
        {
            if (e.Error == null && e.Session != null && e.Session.Status == LiveConnectSessionStatus.Connected)
            {
                this.ApplicationTitle.Text = "got session";
                client = new LiveConnectClient(e.Session);
                client.GetCompleted += new EventHandler<LiveOperationCompletedEventArgs>(client_GetCompleted);
                client.GetAsync("me/skydrive/files?filter=folders");
            }
        }

        void client_GetCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                this.ApplicationTitle.Text = "got folder list";
                Dictionary<string, object> folderData = (Dictionary<string, object>)e.Result;
                List<object> folders = (List<object>)folderData["data"];

                foreach (object item in folders)
                {
                    Dictionary<string, object> folder = (Dictionary<string, object>)item;
                    if (folder["name"].ToString() == skyDriveFolderName)
                        skyDriveFolderID = folder["id"].ToString();
                }

                if (skyDriveFolderID == string.Empty)
                {
                    Dictionary<string, object> skyDriveFolderData = new Dictionary<string, object>();
                    skyDriveFolderData.Add("name", skyDriveFolderName);
                    skyDriveFolderData.Add("type", "folder");
                    client.PostCompleted += new EventHandler<LiveOperationCompletedEventArgs>(client_PostCompleted);
                    client.PostAsync("me/skydrive", skyDriveFolderData);
                }
                else
                {
                    this.ApplicationTitle.Text = "found folder";
                    UploadFile();
                }
            }
            else
            {
                MessageBox.Show(e.Error.Message);
            }
        }

        void client_PostCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                this.ApplicationTitle.Text = "created folder";
                skyDriveFolderID = e.Result["id"].ToString();
                UploadFile();
            }
            else
            {
                MessageBox.Show("Folder creation problem:\n\n" + e.Error.ToString());
            }
        }

        private void UploadFile()
        {
            var buffer = System.Text.UTF8Encoding.UTF8.GetBytes("Fürchterlich!\nOder?");
            var stream = new MemoryStream(buffer);
            client.UploadCompleted += new EventHandler<LiveOperationCompletedEventArgs>(client_UploadCompleted);
            client.UploadAsync(skyDriveFolderID, "status.txt", true, stream, null);
        }

        void client_UploadCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            if (e.Cancelled == false && e.Error == null)
            {
                MessageBox.Show("Fertig!");
            }
            else
            {
                MessageBox.Show("Error :(\n\n"+e.Error.ToString());
            }
        }
    }
}