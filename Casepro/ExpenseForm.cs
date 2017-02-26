﻿using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Casepro
{
    public partial class ExpenseForm : Form
    {
        DataTable t = new DataTable();
        StringFormat strFormat; //Used to format the grid rows.
        ArrayList arrColumnLefts = new ArrayList();//Used to save left coordinates of columns
        ArrayList arrColumnWidths = new ArrayList();//Used to save column widths
        int iCellHeight = 0; //Used to get/set the datagridview cell height
        int iTotalWidth = 0; //
        int iRow = 0;//Used as counter
        bool bFirstPage = false; //Used to check whether we are printing first page
        bool bNewPage = false;// Used to check whether we are printing a new page
        int iHeaderHeight = 0; //Used for the header height
        string month;
        double totalExpense = 0;
        Dictionary<string, string> ExpenseDictionary = new Dictionary<string, string>();
        public ExpenseForm()
        {
            InitializeComponent();
            month = DateTime.Now.ToString("yyyy-MM");
            LoadData();
            searchCbx.Items.Add("Date");
            searchCbx.Items.Add("Client");
            searchCbx.Items.Add("File");
            searchCbx.Items.Add("Method");
            printDocument1.DefaultPageSettings.Landscape = true;

        }

        private void ExpenseForm_Leave(object sender, EventArgs e)
        {
            this.Close();
        }
        private void LoadData()
        {

            MySqlConnection connection = new MySqlConnection(DBConnect.conn);
            MySqlCommand command = connection.CreateCommand();
            MySqlDataReader Reader;
            command.CommandText = "SELECT *  FROM expenses LEFT JOIN client ON client.clientID = expenses.clientID LEFT JOIN file ON file.fileID = expenses.fileID WHERE expenses.date LIKE '%" + month + "%';";
            connection.Open();
            Reader = command.ExecuteReader();
            // create and execute query  
            t = new DataTable();
            t.Columns.Add("id", typeof(string));
            t.Columns.Add(new DataColumn("Select", typeof(bool)));
            t.Columns.Add("Date", typeof(string));//11
            t.Columns.Add("Client");//20
            t.Columns.Add("File");//38
            t.Columns.Add("Amount", typeof(string));//7
            t.Columns.Add("Invoice no.", typeof(string));//8
            t.Columns.Add("Method", typeof(string));//6
            t.Columns.Add("C/O", typeof(string));//36
            t.Columns.Add("Details", typeof(string));//15
            t.Columns.Add("Paid", typeof(string));//10
            t.Columns.Add("Approved", typeof(string));//12
            t.Columns.Add("Signed", typeof(string));//13
            t.Columns.Add("Reason", typeof(string));//14
            t.Columns.Add("Outcome", typeof(string));//15
            t.Columns.Add("Deadline", typeof(string));//44
            t.Columns.Add("View");  //0 
            t.Columns.Add("Delete");  //0 

            ExpenseDictionary.Clear();

            while (Reader.Read())
            {

                //for (int h = 0; h <= 53; h++)
                //{
                //    try
                //    {
                //        System.Diagnostics.Debug.WriteLine(h + "-" + (Reader.IsDBNull(h) ? "" : Reader.GetString(h)));
                //    }
                //    catch { }
                //}
                t.Rows.Add(new object[] { Reader.GetString(0), false, (Reader.IsDBNull(11) ? "none" : Reader.GetString(11)), (Reader.IsDBNull(20) ? "none" : Reader.GetString(20)), (Reader.IsDBNull(38) ? "none" : Reader.GetString(38)), Convert.ToDouble((Reader.IsDBNull(7) ? "0" : Reader.GetString(7))).ToString("n0"), (Reader.IsDBNull(8) ? "0" : Reader.GetString(8)), (Reader.IsDBNull(6) ? "none" : Reader.GetString(6)), (Reader.IsDBNull(36) ? "none" : Reader.GetString(36)), (Reader.IsDBNull(15) ? "none" : Reader.GetString(15)), (Reader.IsDBNull(10) ? "none" : Reader.GetString(10)), (Reader.IsDBNull(12) ? "none" : Reader.GetString(12)), (Reader.IsDBNull(13) ? "none" : Reader.GetString(13)), (Reader.IsDBNull(14) ? "none" : Reader.GetString(14)), (Reader.IsDBNull(15) ? "none" : Reader.GetString(15)), (Reader.IsDBNull(44) ? "none" : Reader.GetString(44)), "Edit", "Delete" });

                ExpenseDictionary.Add((Reader.IsDBNull(0) ? "none" : Reader.GetString(0)), (Reader.IsDBNull(7) ? "none" : Reader.GetString(7)));

            }
            try
            {
                totalExpense = ExpenseDictionary.Sum(m => Convert.ToDouble(m.Value));
                t.Rows.Add(new object[] { " ", false, "", "", "", "", " ", "", "", "", "", "", "", "", "", "", "", "" });
                t.Rows.Add(new object[] { " ", false, "", "TOTAL EXPENSES:", "", (totalExpense).ToString("n0"), " ", "", "", "", "", "", "", "", "", "", "", "" });
            }
            catch
            {
            }
            dtGrid.DataSource = t;

            this.dtGrid.Columns[0].Visible = false;
            this.dtGrid.Columns[16].DefaultCellStyle.BackColor = Color.Green;
            this.dtGrid.Columns[17].DefaultCellStyle.BackColor = Color.Red;
            // this.dtGrid.Columns[1].Visible = false;


        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FeesForm_Leave(object sender, EventArgs e)
        {
            this.Close();
        }
        string filterField = "Date";
        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            t.DefaultView.RowFilter = string.Format("[{0}] LIKE '%{1}%'", filterField, DateTxt.Text);

        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            PrintPreviewDialog objPPdialog = new PrintPreviewDialog();
            objPPdialog.Document = printDocument1;
            objPPdialog.ShowDialog();
        }
        #region Begin Print Event Handler
        /// <summary>
        /// Handles the begin print event of print document
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void printDocument1_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            try
            {
                strFormat = new StringFormat();
                strFormat.Alignment = StringAlignment.Near;
                strFormat.LineAlignment = StringAlignment.Center;
                strFormat.Trimming = StringTrimming.EllipsisCharacter;

                arrColumnLefts.Clear();
                arrColumnWidths.Clear();
                iCellHeight = 0;
                iRow = 0;
                bFirstPage = true;
                bNewPage = true;

                // Calculating Total Widths
                iTotalWidth = 0;
                foreach (DataGridViewColumn dgvGridCol in dtGrid.Columns)
                {
                    iTotalWidth += dgvGridCol.Width;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Print Page Event
        /// <summary>
        /// Handles the print page event of print document
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            try
            {
                //Set the left margin
                int iLeftMargin = e.MarginBounds.Left;
                //Set the top margin
                int iTopMargin = e.MarginBounds.Top;
                //Whether more pages have to print or not
                bool bMorePagesToPrint = false;
                int iTmpWidth = 0;

                //For the first page to print set the cell width and header height
                if (bFirstPage)
                {
                    foreach (DataGridViewColumn GridCol in dtGrid.Columns)
                    {
                        iTmpWidth = (int)(Math.Floor((double)((double)GridCol.Width /
                                       (double)iTotalWidth * (double)iTotalWidth *
                                       ((double)e.MarginBounds.Width / (double)iTotalWidth))));

                        iHeaderHeight = (int)(e.Graphics.MeasureString(GridCol.HeaderText,
                                    GridCol.InheritedStyle.Font, iTmpWidth).Height) + 11;

                        // Save width and height of headres
                        arrColumnLefts.Add(iLeftMargin);
                        arrColumnWidths.Add(iTmpWidth);
                        iLeftMargin += iTmpWidth;
                    }
                }
                //Loop till all the grid rows not get printed
                while (iRow <= dtGrid.Rows.Count - 1)
                {
                    DataGridViewRow GridRow = dtGrid.Rows[iRow];
                    //Set the cell height
                    iCellHeight = GridRow.Height + 5;
                    int iCount = 0;
                    //Check whether the current page settings allo more rows to print
                    if (iTopMargin + iCellHeight >= e.MarginBounds.Height + e.MarginBounds.Top)
                    {
                        bNewPage = true;
                        bFirstPage = false;
                        bMorePagesToPrint = true;
                        break;
                    }
                    else
                    {
                        if (bNewPage)
                        {
                            //Draw Header
                            e.Graphics.DrawString("Customer Summary", new Font(dtGrid.Font, FontStyle.Bold),
                                    Brushes.Black, e.MarginBounds.Left, e.MarginBounds.Top -
                                    e.Graphics.MeasureString("Customer Summary", new Font(dtGrid.Font,
                                    FontStyle.Bold), e.MarginBounds.Width).Height - 13);

                            String strDate = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToShortTimeString();
                            //Draw Date
                            e.Graphics.DrawString(strDate, new Font(dtGrid.Font, FontStyle.Bold),
                                    Brushes.Black, e.MarginBounds.Left + (e.MarginBounds.Width -
                                    e.Graphics.MeasureString(strDate, new Font(dtGrid.Font,
                                    FontStyle.Bold), e.MarginBounds.Width).Width), e.MarginBounds.Top -
                                    e.Graphics.MeasureString("Customer Summary", new Font(new Font(dtGrid.Font,
                                    FontStyle.Bold), FontStyle.Bold), e.MarginBounds.Width).Height - 13);

                            //Draw Columns                 
                            iTopMargin = e.MarginBounds.Top;
                            foreach (DataGridViewColumn GridCol in dtGrid.Columns)
                            {
                                e.Graphics.FillRectangle(new SolidBrush(Color.LightGray),
                                    new Rectangle((int)arrColumnLefts[iCount], iTopMargin,
                                    (int)arrColumnWidths[iCount], iHeaderHeight));

                                e.Graphics.DrawRectangle(Pens.Black,
                                    new Rectangle((int)arrColumnLefts[iCount], iTopMargin,
                                    (int)arrColumnWidths[iCount], iHeaderHeight));

                                e.Graphics.DrawString(GridCol.HeaderText, GridCol.InheritedStyle.Font,
                                    new SolidBrush(GridCol.InheritedStyle.ForeColor),
                                    new RectangleF((int)arrColumnLefts[iCount], iTopMargin,
                                    (int)arrColumnWidths[iCount], iHeaderHeight), strFormat);
                                iCount++;
                            }
                            bNewPage = false;
                            iTopMargin += iHeaderHeight;
                        }
                        iCount = 0;
                        //Draw Columns Contents                
                        foreach (DataGridViewCell Cel in GridRow.Cells)
                        {
                            if (Cel.Value != null)
                            {
                                e.Graphics.DrawString(Cel.Value.ToString(), Cel.InheritedStyle.Font,
                                            new SolidBrush(Cel.InheritedStyle.ForeColor),
                                            new RectangleF((int)arrColumnLefts[iCount], (float)iTopMargin,
                                            (int)arrColumnWidths[iCount], (float)iCellHeight), strFormat);
                            }
                            //Drawing Cells Borders 
                            e.Graphics.DrawRectangle(Pens.Black, new Rectangle((int)arrColumnLefts[iCount],
                                    iTopMargin, (int)arrColumnWidths[iCount], iCellHeight));

                            iCount++;
                        }
                    }
                    iRow++;
                    iTopMargin += iCellHeight;
                }

                //If more lines exist, print another page.
                if (bMorePagesToPrint)
                    e.HasMorePages = true;
                else
                    e.HasMorePages = false;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            NewExpense frm = new NewExpense(null);
            frm.MdiParent = MainForm.ActiveForm;
            frm.Show();
            this.Close();
        }

        List<string> fileIDs = new List<string>();
        private void dtGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            var senderGrid = (DataGridView)sender;

            if (e.ColumnIndex == dtGrid.Columns[1].Index && e.RowIndex >= 0)
            {
                if (fileIDs.Contains(dtGrid.Rows[e.RowIndex].Cells[0].Value.ToString()))
                {
                    fileIDs.Remove(dtGrid.Rows[e.RowIndex].Cells[0].Value.ToString());
                    Console.WriteLine("REMOVED this id " + dtGrid.Rows[e.RowIndex].Cells[0].Value.ToString());
                }
                else
                {
                    fileIDs.Add(dtGrid.Rows[e.RowIndex].Cells[0].Value.ToString());
                    Console.WriteLine("ADDED ITEM " + dtGrid.Rows[e.RowIndex].Cells[0].Value.ToString());
                }
            }
            if (e.ColumnIndex == dtGrid.Columns[16].Index && e.RowIndex >= 0)
            {
                NewExpense frm = new NewExpense(dtGrid.Rows[e.RowIndex].Cells[0].Value.ToString());
                frm.MdiParent = MainForm.ActiveForm;
                frm.Show();
                this.Close();
            }
            try
            {

                if (e.ColumnIndex == dtGrid.Columns[17].Index && e.RowIndex >= 0)
                {
                    if (MessageBox.Show("YES or NO?", "Are you sure you want to delete this expense? ", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        string Query = "DELETE from expenses WHERE expenseID ='" + dtGrid.Rows[e.RowIndex].Cells[0].Value.ToString() + "'";
                        Helper.Execute(Query, DBConnect.conn);
                        string Query2 = "INSERT INTO `deletion`( `table`, `eid`,`column`, `created`) VALUES ('expenses','" + dtGrid.Rows[e.RowIndex].Cells[0].Value.ToString() + "','expenseID','" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "');";
                        Helper.Execute(Query2, DBConnect.conn);
                        MessageBox.Show("Information deleted");
                    }
                    Console.WriteLine("DELETE on row {0} clicked", e.RowIndex + dtGrid.Rows[e.RowIndex].Cells[0].Value.ToString() + dtGrid.Rows[e.RowIndex].Cells[2].Value.ToString());
                }
            }
            catch { }

        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("YES or NO?", "Are you sure you want to delete these expenses? ", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                foreach (var item in fileIDs)
                {
                    string Query = "DELETE from expenses WHERE expenseID ='" + item + "'";
                    Helper.Execute(Query, DBConnect.conn);

                }
            }
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            NewExpense frm = new NewExpense(null);
            frm.MdiParent = MainForm.ActiveForm;
            frm.Show();
            this.Close();
        }

        private void toolStripButton3_Click_1(object sender, EventArgs e)
        {

        }

        private void searchCbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            filterField = searchCbx.Text;
        }

        private void monthPicker_CloseUp(object sender, EventArgs e)
        {
            month = Convert.ToDateTime(monthPicker.Text).ToString("yyyy-MM");
            LoadData();
        }
    }
}
