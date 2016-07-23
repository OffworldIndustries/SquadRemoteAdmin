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
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using Squad.Admin.Console.Utilities;



namespace Squad.Admin.Console.Forms
{

    public partial class frmMainConsole : Form
    {

        ServerConnectionInfo serverConnectionInfo = new ServerConnectionInfo();
        ServerProxy rconServerProxy = new ServerProxy();

        public frmMainConsole()
        {
            InitializeComponent();

            // Bind control event handlers
            this.txtServerIP.Validating += TxtServerIP_Validating;
            this.txtServerPort.Validating += TxtServerPort_Validating;
            this.txtRconPassword.Validating += TxtRconPassword_Validating;
            this.grdPlayers.CellContentClick += GrdPlayers_CellContentClick;

            LoadAutocompleteCommands();
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
            if (this.rconServerProxy.Connect(this.serverConnectionInfo))
            {
                EnableLoginControls(true);
                ListPlayers();
            }

        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            grdPlayers.Rows.Clear();
            EnableLoginControls(false);
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            // turn on or off the password mask by checking the checkbox
            txtRconPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ListPlayers();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            //string x = "ID: 41 | SteamID: 76561198235704656 | Name: DayO1dTuna";
            //string x = "ID: 45 | SteamID: 76561197989313691 | Since Disconnect: 04m.38s | Name: cam";
            //string[] playerInfo = x.Split(':');
            //Trace.WriteLine("Number of elements = " + playerInfo.Length.ToString());
            //Trace.WriteLine("playerInfo[0] = " + playerInfo[0]);
            //Trace.WriteLine("playerInfo[1] = " + playerInfo[1]);
            //Trace.WriteLine("playerInfo[2] = " + playerInfo[2]);
            //Trace.WriteLine("playerInfo[3] = " + playerInfo[3]);
            //Trace.WriteLine("playerInfo[4] = " + playerInfo[4]);

            //Trace.WriteLine("");
            //string[] y = playerInfo[1].Split('|');
            //Trace.WriteLine("Slot: " + y[0]);
            //string[] z = playerInfo[2].Split('|');
            //Trace.WriteLine("SteamId: " + z[0]);
            //string[] a = playerInfo[3].Split('|');
            //Trace.WriteLine("Disconnected: " + a[0]);
            //Trace.WriteLine("PlayerName: " + playerInfo[4]);
        }

        // Launch the browser with the Steam profile of the selected player
        private void GrdPlayers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string sUrl = "http://steamcommunity.com/profiles/" + grdPlayers.Rows[e.RowIndex].Cells[2].Value.ToString();
            ProcessStartInfo sInfo = new ProcessStartInfo(sUrl);
            Process.Start(sInfo);
        }

        #endregion

        #region Helpers

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

        /// <summary>
        /// Fill the Command textbox autocomplete list with all valid commands from the Commands.dat text file
        /// </summary>
        private void LoadAutocompleteCommands()
        {
            AutoCompleteStringCollection commandList = new AutoCompleteStringCollection();

            string[] commands = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "Commands.dat");

            for (int i = 0; i < commands.Length; i++)
            {
                commandList.Add(commands[i].Trim());
            }

            txtCommand.AutoCompleteCustomSource = commandList;
        }

        #endregion

        #region Command and response

        public void ListPlayers()
        {
            grdPlayers.Rows.Clear();

            try
            {
                bool d = false;     // flag used to indicate that we are not processing the recently disconnected players
                string response = this.rconServerProxy.GetPlayerList();
                string currentLine = string.Empty;

                // Take the response and break it into a string array breaking each line off
                string[] responseArray = response.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                for (int i = 0; i < responseArray.Length; i++)
                {
                    // Get the current line
                    currentLine = responseArray[i].Trim();

                    // No blank lines
                    if (currentLine.Length > 0)
                    {
                        // Looking for the list to change to disconnected players - skip this check once we're in the disconnected list
                        if (!d) d = currentLine.Trim().ToUpper() == "----- Recently Disconnected Players [Max of 15] ----".ToUpper();


                        if (!d)
                        {
                            // Process connected player list - skip the first line
                            if (currentLine.Trim().ToUpper() != ("----- Active Players -----").ToUpper())
                            {
                                string[] playerInfo = currentLine.Split(':');

                                string[] slot = playerInfo[1].Split('|');
                                string[] steamId = playerInfo[2].Split('|');
                                string playerName = playerInfo[3];

                                grdPlayers.Rows.Add(slot[0], playerName, steamId[0], "Connected", "");
                                
                            }
                        }
                        else
                        {
                            // Process disconnected player list
                            if (currentLine.Trim().ToUpper() != "----- Recently Disconnected Players [Max of 15] ----".ToUpper())
                            {
                                string[] playerInfo = currentLine.Split(':');

                                string[] slot = playerInfo[1].Split('|');
                                string[] steamId = playerInfo[2].Split('|');
                                string[] time = playerInfo[3].Split('|');
                                string playerName = playerInfo[4];

                                grdPlayers.Rows.Add(slot[0], playerName, steamId[0], "Disconnected", time[0]);
                            }
                        }
                    }
                }

            }
            catch(Exception ex)
            { }

        }

        #endregion

        private void btnSend_Click(object sender, EventArgs e)
        {
            string r = this.rconServerProxy.SendCommand(txtCommand.Text);
            txtResponse.Text = r;
            lstHistory.Items.Insert(0, txtCommand.Text);
        }
    }
}
