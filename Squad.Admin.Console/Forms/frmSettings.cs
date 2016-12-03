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
using System.Xml;
using System.Xml.Linq;

namespace Squad.Admin.Console.Forms
{
    public partial class frmSettings : Form
    {
        private List<ContextMenuMessage> warningMessages = new List<ContextMenuMessage>();
        private List<ContextMenuMessage> kickMessages = new List<ContextMenuMessage>();
        private List<ContextMenuMessage> banMessages = new List<ContextMenuMessage>();

        private string currentList = string.Empty;

        public frmSettings()
        {
            InitializeComponent();
            LoadMessageDropdown();
            LoadMessages();

            cboMessageTypes.SelectedIndexChanged += CboMessageTypes_SelectedIndexChanged;
            dataGridView1.UserAddedRow += DataGridView1_UserAddedRow;
            dataGridView1.UserDeletedRow += DataGridView1_UserDeletedRow;
            dataGridView1.CellEndEdit += DataGridView1_CellEndEdit;
        }

        private void DataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (!btnApply.Enabled) btnApply.Enabled = true;
        }

        private void DataGridView1_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            if (!btnApply.Enabled) btnApply.Enabled = true;
        }

        private void DataGridView1_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            if (!btnApply.Enabled) btnApply.Enabled = true;
        }

        private void CboMessageTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindingSource bs = new BindingSource();
            
            // save the current list before loading a new one
            if (currentList != string.Empty)
            {
                var bindingSource = (BindingSource)dataGridView1.DataSource;
                switch (currentList)
                {
                    case "Warning":
                        warningMessages = ((List<ContextMenuMessage>)bindingSource.DataSource).Cast<ContextMenuMessage>().ToList();
                        break;
                    case "Kick":
                        kickMessages = ((List<ContextMenuMessage>)bindingSource.DataSource).Cast<ContextMenuMessage>().ToList();
                        break;
                    case "Ban":
                        banMessages = ((List<ContextMenuMessage>)bindingSource.DataSource).Cast<ContextMenuMessage>().ToList();
                        break;
                }
            }

            // Load the selected messages
            switch (((ComboBox)sender).SelectedItem.ToString())
            {
                case "Warning":
                    bs.DataSource = warningMessages;
                    break;
                case "Kick":
                    bs.DataSource = kickMessages;
                    break;
                case "Ban":
                    bs.DataSource = banMessages;
                    break;
                default:
                    break;
            }

            // track the current list so it can be saved next time the list type is changed
            currentList = ((ComboBox)sender).SelectedItem.ToString();
            // bind the grid to the list and show the data
            dataGridView1.DataSource = bs;
            dataGridView1.Columns[1].Width = 550;
        }

        private void LoadMessageDropdown()
        {
            cboMessageTypes.Items.Clear();
            cboMessageTypes.Items.Add("Warning");
            cboMessageTypes.Items.Add("Kick");
            cboMessageTypes.Items.Add("Ban");
        }

        private void LoadMessages()
        {
            try
            {
                XDocument reasonMsgs = XDocument.Load(@"MenuReasons.xml");

                foreach (XElement w in reasonMsgs.Root.Element("WarnReasons").Elements())
                {
                    warningMessages.Add(new ContextMenuMessage() { MsgType = "Warning", Message = w.Value.ToString() });
                }

                foreach (XElement w in reasonMsgs.Root.Element("KickReasons").Elements())
                {
                    kickMessages.Add(new ContextMenuMessage() { MsgType = "Kick", Message = w.Value.ToString() });
                }

                foreach (XElement w in reasonMsgs.Root.Element("BanReasons").Elements())
                {
                    banMessages.Add(new ContextMenuMessage() { MsgType = "Ban", Message = w.Value.ToString() });
                }

            }
            catch (Exception) { }

        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            // Top level element
            XElement menuFile = new XElement("MenuReasons");

            XElement warnReasons = new XElement("WarnReasons", warningMessages.Select(x => new XElement("WarnReason", x.Message)));
            XElement kickReasons = new XElement("KickReasons", kickMessages.Select(x => new XElement("KickReason", x.Message)));
            XElement banReasons = new XElement("BanReasons", banMessages.Select(x => new XElement("BanReason", x.Message)));

            menuFile.Add(warnReasons);
            menuFile.Add(kickReasons);
            menuFile.Add(banReasons);

            menuFile.Save(@"MenuReasons.xml");

            this.Close();
            this.Dispose();
           
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }
    }

    internal class ContextMenuMessage
    {
        public string MsgType { get; set; }
        public string Message { get; set; }
        public ContextMenuMessage() { }
    }
}
