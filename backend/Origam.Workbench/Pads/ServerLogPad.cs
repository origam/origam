#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Origam.Extensions;

namespace Origam.Workbench.Pads;

/// <summary>
/// Summary description for LogPad.
/// </summary>
public class ServerLogPad : OutputPad
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    private Timer timer;
    private System.ComponentModel.IContainer components;
    private int _lastPosition = 0;
    private CheckBox pauseCheckbox;
    private Label lblLogSize;
    private bool _isFirst = true;
    private const int INTERVAL = 500;

    public ServerLogPad()
        : base()
    {
        InitializeComponent();
        this.timer.Interval = INTERVAL;
        this.TabText = "Server Log";
        this.Text = "Server Log";
        timer.Start();
        toolBar.Show();
    }

    void pauseCheckbox_CheckedChanged(object sender, EventArgs e)
    {
        if (pauseCheckbox.Checked)
        {
            timer.Stop();
        }
        else
        {
            // restart with the default interval so it is possible
            // to manually restart after log downtime escalated the interval
            // too high
            timer.Interval = INTERVAL;
            timer.Start();
        }
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources =
            new System.ComponentModel.ComponentResourceManager(typeof(ServerLogPad));
        this.timer = new System.Windows.Forms.Timer(this.components);
        this.pauseCheckbox = new System.Windows.Forms.CheckBox();
        this.lblLogSize = new System.Windows.Forms.Label();
        this.toolBar.SuspendLayout();
        this.SuspendLayout();
        //
        // toolBar
        //
        this.toolBar.Controls.Add(this.lblLogSize);
        this.toolBar.Controls.Add(this.pauseCheckbox);
        this.toolBar.Size = new System.Drawing.Size(292, 27);
        //
        // timer
        //
        this.timer.Tick += new System.EventHandler(this.timer_Tick);
        //
        // pauseCheckbox
        //
        this.pauseCheckbox.Location = new System.Drawing.Point(13, 6);
        this.pauseCheckbox.Name = "pauseCheckbox";
        this.pauseCheckbox.Size = new System.Drawing.Size(56, 17);
        this.pauseCheckbox.TabIndex = 0;
        this.pauseCheckbox.Text = "Pause";
        this.pauseCheckbox.CheckedChanged += new System.EventHandler(
            this.pauseCheckbox_CheckedChanged
        );
        //
        // lblLogSize
        //
        this.lblLogSize.AutoSize = true;
        this.lblLogSize.Location = new System.Drawing.Point(85, 7);
        this.lblLogSize.Name = "lblLogSize";
        this.lblLogSize.Size = new System.Drawing.Size(0, 13);
        this.lblLogSize.TabIndex = 1;
        //
        // ServerLogPad
        //
        this.ClientSize = new System.Drawing.Size(292, 271);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "ServerLogPad";
        this.toolBar.ResumeLayout(false);
        this.toolBar.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private void timer_Tick(object sender, EventArgs e)
    {
        try
        {
            OrigamSettings settings =
                ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
            if (settings == null)
            {
                SetOutputText("Configuration not selected.");
                return;
            }
            string url = settings.ServerLogUrl;
            if (url == "" || url == null)
            {
                SetOutputText("ServerLogUrl not set in the settings.");
                return;
            }
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Timeout = 1000;
            int size = GetFileSize(url);
            lblLogSize.Text = "Log Size: " + size.ToString("#,0") + " bytes";
            if (_lastPosition == 0)
            {
                request.AddRange(-GetInitialSize(url));
            }
            else
            {
                if (size == _lastPosition)
                {
                    // nothing changed, we do not continue
                    return;
                }

                if (size >= _lastPosition)
                {
                    // file grew, we request the increment
                    request.AddRange(_lastPosition, size);
                }
                else
                {
                    // file is smaller (different file), we go from the beginning
                    request.AddRange(-GetInitialSize(url));
                }
            }
            RetrieveLogFromServer(request);
            timer.Interval = INTERVAL;
        }
        catch (Exception ex)
        {
            timer.Interval *= 10;
            string message =
                "Could not load server log. Will retry in " + (timer.Interval / 1000) + " seconds.";
            SetOutputText(message);
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError(message, ex);
            }
        }
    }

    private void RetrieveLogFromServer(HttpWebRequest request)
    {
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
            using (Stream responseStream = response.GetResponseStream())
            {
                string range = response.Headers["Content-Range"];
                if (range != null)
                {
                    _lastPosition = Convert.ToInt32(range.Substring(range.IndexOf("/") + 1));
                }
                string result = HttpTools.Instance.ReadResponseText(response, responseStream);
                AddText(result);
                if (_isFirst)
                {
                    _isFirst = false;
                }
            }
        }
    }

    private static int GetInitialSize(string url)
    {
        int initSize = 10240;
        int size = GetFileSize(url);
        if (size < initSize)
        {
            return size;
        }

        return initSize;
    }

    private static int GetFileSize(string url)
    {
        ServicePointManager.SecurityProtocol =
            SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        int size;
        HttpWebRequest headRequest = (HttpWebRequest)HttpWebRequest.Create(url);
        headRequest.Timeout = 1000;
        headRequest.Method = "HEAD";
        using (HttpWebResponse headResponse = (HttpWebResponse)headRequest.GetResponse())
        {
            size = Convert.ToInt32(headResponse.ContentLength);
        }
        return size;
    }
}
