﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using RaptorDB;
using RaptorDB.Common;
using SampleViews;

namespace datagridbinding
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}


        //RaptorDB.RaptorDB rap;
        RaptorDBClient rap;
		
		private void Form1_Load(object sender, EventArgs e)
		{
            dataGridView1.DoubleBuffered(true);
            //rap = RaptorDB.RaptorDB.Open(@"..\..\..\RaptorDBdata");

            //rap.RegisterView(new SalesInvoiceView());
            //rap.RegisterView(new SalesItemRowsView());
            //rap.RegisterView(new newview());

            rap = new RaptorDBClient("127.0.0.1", 90, "admin", "admin");

            Query();
		}
		
		void TextBox1KeyPress(object sender, KeyPressEventArgs e)
		{
			if(e.KeyChar == (char)Keys.Return)
                Query();
		}

        private void Query()
        {
            string[] s = textBox1.Text.Split(',');

            try
            {
                DateTime dt = FastDateTime.Now;
                var q = rap.Query(s[0].Trim(), s[1].Trim());
                toolStripStatusLabel2.Text = "Query time (sec) = " + FastDateTime.Now.Subtract(dt).TotalSeconds;
                dataGridView1.DataSource = q.Rows;
                toolStripStatusLabel1.Text = "Count = " + q.Count.ToString("#,0");
            }
            catch { }
        }

        private void sumQueryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime dt = FastDateTime.Now;
            var q = rap.Query(typeof(SalesItemRowsView), (LineItem l) => (l.Product == "prod 1" || l.Product == "prod 3"));

            List<SalesItemRowsView.RowSchema> list = q.Rows.Cast<SalesItemRowsView.RowSchema>().ToList();
            var res = from item in list
                    group item by item.Product into grouped
                    select new
                    {
                        Product = grouped.Key,
                        TotalPrice = grouped.Sum(product => product.Price),
                        TotalQTY = grouped.Sum(product => product.QTY)
                    };

            dataGridView1.DataSource = res.ToList();
            toolStripStatusLabel2.Text = "Query time (sec) = " + FastDateTime.Now.Subtract(dt).TotalSeconds;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rap.Shutdown();
            this.Close();
        }

        private object _lock = new object();
        private void insert100000DocumentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (_lock)
            {
                DialogResult dr = MessageBox.Show("Do you want to insert?", "Continue?", MessageBoxButtons.OKCancel, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2);
                if (dr == System.Windows.Forms.DialogResult.Cancel)
                    return;
                toolStripProgressBar1.Value = 0;
                DateTime dt = FastDateTime.Now;
                int count = 100000;
                int step = 5000;
                toolStripProgressBar1.Maximum = (count / step) + 1;

                for (int i = 0; i < count; i++)
                {
                    var inv = new SalesInvoice()
                    {
                        Date = FastDateTime.Now,
                        Serial = i % 10000,
                        CustomerName = "me " + i % 10,
                        Status = (byte)(i % 4),
                        Address = "df asd sdf asdf asdf"
                    };
                    inv.Items = new List<LineItem>();
                    for (int k = 0; k < 5; k++)
                        inv.Items.Add(new LineItem() { Product = "prod " + k, Discount = 0, Price = 10 + k, QTY = 1 + k });
                    if (i % step == 0)
                        toolStripProgressBar1.Value++;
                    rap.Save(inv.ID, inv);
                }
                MessageBox.Show("Insert done in (sec) : " + FastDateTime.Now.Subtract(dt).TotalSeconds);
                toolStripProgressBar1.Value = 0;
            }
        }
	}
}