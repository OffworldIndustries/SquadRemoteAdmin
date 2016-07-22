#region License
/* 
 * Copyright (C) 2013 Myrcon Pty. Ltd. / Geoff "Phogue" Green
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 * IN THE SOFTWARE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using QueryMaster;
using QueryMaster.Steam;
using QueryMaster.GameServer;
using Squad.AdminConsole.Utilities;
using System.Diagnostics;


namespace Squad.Admin.Console.Forms
{
    public partial class frmMainConsole : Form
    {

        Server squadServer = null;
        private ServerConnection serverConnectionInfo = new ServerConnection();

        public frmMainConsole()
        {
            InitializeComponent();
            this.txtServerIP.Validating += TxtServerIP_Validating;
            this.txtServerPort.Validating += TxtServerPort_Validating;
            this.txtRconPassword.Validating += TxtRconPassword_Validating;
        }

        #region control validation events

        private void TxtRconPassword_Validating(object sender, CancelEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            this.serverConnectionInfo.Password = tb.Text.Trim();
            btnConnect.Enabled = this.serverConnectionInfo.IsValid();
        }

        private void TxtServerPort_Validating(object sender, CancelEventArgs e)
        {
            MaskedTextBox tb = (MaskedTextBox)sender;
            this.serverConnectionInfo.ServerPort = Convert.ToInt32(tb.Text.Trim());
            btnConnect.Enabled = this.serverConnectionInfo.IsValid();
        }

        private void TxtServerIP_Validating(object sender, CancelEventArgs e)
        {
            IPAddress ip;
            TextBox tb = (TextBox)sender;
            if (IPAddress.TryParse(tb.Text.Trim(), out ip))
            {
                this.serverConnectionInfo.ServerIP = ip;
            }
            btnConnect.Enabled = this.serverConnectionInfo.IsValid();
        }

        #endregion


        #region control event handlers

        private void btnConnect_Click(object sender, EventArgs e)
        {

            // Squad Test Server
            //Server s = ServerQuery.GetServerInstance(EngineType.Source, new IPEndPoint(IPAddress.Parse("172.93.107.234"), 21114));
            //if (s.GetControl("wT5fmUbkmSRb"))

            // TG
            //Server s = ServerQuery.GetServerInstance(EngineType.Source, new IPEndPoint(IPAddress.Parse("205.251.144.66"), 21114));
            //if (s.GetControl("97M76jvZ"))

            // Local
            //Server s = ServerQuery.GetServerInstance(EngineType.Source, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 21114));
            //if (s.GetControl("password"))

            this.squadServer = ServerQuery.GetServerInstance(EngineType.Source, new IPEndPoint(this.serverConnectionInfo.ServerIP, this.serverConnectionInfo.ServerPort));

            if (this.squadServer.GetControl(this.serverConnectionInfo.Password))
            {
                EnableLoginControls(true);
                txtResponse.Text = this.squadServer.Rcon.SendCommand("ListPlayers", true);
            }

        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            // turn on or off the password mask by checking the checkbox
            txtRconPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }

        #endregion

        private void EnableLoginControls(bool enable)
        {
            btnDisconnect.Enabled = enable;
            btnConnect.Enabled = !enable;
            txtServerIP.Enabled = !enable;
            txtServerPort.Enabled = !enable;
            txtRconPassword.Enabled = !enable;
            txtDisplayName.Enabled = !enable;
            chkShowPassword.Enabled = !enable;
            grpPlayerList.Enabled = enable;
            grpConsole.Enabled = enable;
        }

    }
}
