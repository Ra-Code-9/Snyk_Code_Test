using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace ClientProfile.Scripts
{
    /// <summary>
    /// Summary description for View.
    /// </summary>
    public partial class CustomSQL : ClientProfilePage
    {
        protected Guid userID;

        public CustomSQL()
        {
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            SetPageTitle("Custom SQL");
            ResultsPanel.Visible = false;
            ErrorLabel.Visible = false;

            try
            {
                userID = SecurityUser.UserId;
            }
            catch (Exception ex)
            {
                ClientProfileError.SystemError(new StackTrace(true).GetFrame(0), ex);
            }
        }

        protected void Page_Command(object sender, CommandEventArgs e)
        {
            try
            {
                switch (e.CommandName)
                {
                    case "Cancel":
                        {
                            string url = String.Format(@"../Scripts/default.aspx");
                            Response.Redirect(url);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                ClientProfileError.SystemError(new StackTrace(true).GetFrame(0), ex);
            }
        }

        protected void SaveButton_Click(object sender, System.EventArgs e)
        {
            // Dump out special header and the file content and end the response
            Response.Clear();
            Response.ClearHeaders();
            Response.ClearContent();

            // This header (RFC 1806) lets us set the suggested filename
            Response.AddHeader("Content-Disposition", "attachment; filename=query.sql");
            Response.Write(QueryTextbox.Text);
            Response.End();
        }

        protected void LoadButton_Click(object sender, System.EventArgs e)
        {
            // Grab file from post data
            HttpPostedFile file = FileUploadInput.PostedFile;

            int length = file.ContentLength;

            byte[] buff = new byte[length];
            file.InputStream.Read(buff, 0, length);

            //Convert from byte array to string
            StringBuilder qsb = new StringBuilder();
            for (int i = 0; i < length; i++)
                qsb.Append(Convert.ToChar(buff[i]));

            QueryTextbox.Text = qsb.ToString();
        }

        protected void ClearButton_Click(object sender, System.EventArgs e)
        {
            ErrorLabel.Text = string.Empty;
            lblSQL.Text = string.Empty;
            QueryTextbox.Text = string.Empty;
        }

        protected void ExecuteButton_Click(object sender, System.EventArgs e)
        {
            ErrorLabel.Text = string.Empty;

            string message = string.Empty;
            String theSQL = string.Empty;

            theSQL = QueryTextbox.Text;

            var customSqlAccess = new UserCustomSqlAccess();

            if (!customSqlAccess.CanAccess)
            {
                ResultsPanel.Visible = false;
                ErrorLabel.Visible = true;
                ErrorLabel.Text = "You do not have permission to run SQL.";
                return;
            }

            if (theSQL.Trim().Length == 0)
            {
                ResultsPanel.Visible = false;
                ErrorLabel.Visible = true;
                ErrorLabel.Text = "Enter a query";
                return;
            }

            if (!customSqlAccess.CanUpdate &&
                (theSQL.ToLower().Contains("update") ||
                 theSQL.ToLower().Contains("insert") ||
                 theSQL.ToLower().Contains("delete") ||
                 theSQL.ToLower().Contains("exec"))
                )
            {
                ResultsPanel.Visible = false;
                ErrorLabel.Visible = true;
                ErrorLabel.Text = "You do not have permission to run SQL that will update the database.  Please use only SELECT statements.";
                return;
            }

            if (txtWRID.Text == string.Empty &&
                (theSQL.ToLower().Contains("alter") ||
                 theSQL.ToLower().Contains("create table") ||
                 theSQL.ToLower().Contains("create view") ||
                 theSQL.ToLower().Contains("create procedure") ||
                 theSQL.ToLower().Contains("drop") ||
                 theSQL.ToLower().Contains("grant") ||
                 theSQL.ToLower().Contains("kill") ||
                 theSQL.ToLower().Contains("restore") ||
                 theSQL.ToLower().Contains("revoke") ||
                 theSQL.ToLower().Contains("shutdown"))
                )
            {
                ResultsPanel.Visible = false;
                ErrorLabel.Visible = true;
                ErrorLabel.Text = "The following commands are not allowed: ALTER, CREATE TABLE, CREATE VIEW, CREATE PROCEDURE, DROP, GRANT, KILL, RESTORE, REVOKE, SHUTDOWN";
                return;
            }

            if (txtWRID.Text == string.Empty &&
                (theSQL.ToLower().Contains("update") ||
                 theSQL.ToLower().Contains("insert") ||
                 theSQL.ToLower().Contains("delete") ||
                 theSQL.ToLower().Contains("exec"))
                )
            {
                if (txtWRID.Text == string.Empty)
                {
                    ResultsPanel.Visible = false;
                    ErrorLabel.Visible = true;
                    ErrorLabel.Text = "Work Request is required - SQL contains at least one of the following: DELETE, EXEC, INSERT or UPDATE";
                    return;
                }
            }

            Guid wrClient = Guid.Empty;
            Guid wrID = Guid.Empty;

            if (txtWRID.Text != string.Empty)
            {
                var workRequest = Utils.GetWorkRequestsByShortId(txtWRID.Text)
                    .Where(r => r.StatusID != 8 && r.StatusID != 18)
                    .FirstOrDefault();

                if (workRequest == null)
                {
                    ResultsPanel.Visible = false;
                    ErrorLabel.Visible = true;
                    ErrorLabel.Text = "Work Request not valid";
                    return;
                }

                wrID = workRequest.ID;
                wrClient = workRequest.CPCltID;

                if (wrClient == Guid.Empty)
                {
                    ResultsPanel.Visible = false;
                    ErrorLabel.Visible = true;
                    ErrorLabel.Text = "Work Request does not contain a valid client";
                    return;
                }
            }

            if (theSQL.ToLower().Contains("update") ||
                 theSQL.ToLower().Contains("insert") ||
                 theSQL.ToLower().Contains("delete") ||
                 theSQL.ToLower().Contains("exec")
                )
            {
                if (!theSQL.ToLower().Contains("manageworkrequestforscript"))
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine(
                        "-- Please add a call to ManageWorkRequestForScript for each Table/Product combination being updated.");
                    sql.AppendLine(string.Empty);
                    sql.AppendLine(
                        "-- exec ManageWorkRequestForScript WRID, CPCltID, SysProdID, DateTime, SysUserID, Table Name, SectionID, SectionName");
                    sql.AppendLine(string.Empty);
                    sql.AppendLine(string.Empty);
                    sql.AppendLine(QueryTextbox.Text);
                    QueryTextbox.Text = sql.ToString();
                    ResultsPanel.Visible = false;
                    ErrorLabel.Visible = true;
                    ErrorLabel.Text = "This script is updating data, include calls to the ManageWorkrequestForScript stored proc for each table/product being updated.  An example has been provided at the beginning of the script";
                    return;
                }
            }

            if ((!theSQL.ToLower().Contains("begin tran") || (!theSQL.ToLower().Contains("commit tran") && !theSQL.ToLower().Contains("rollback tran")))
                &&
                (theSQL.ToLower().Contains("update") ||
                 theSQL.ToLower().Contains("insert") ||
                 theSQL.ToLower().Contains("delete") ||
                 theSQL.ToLower().Contains("exec"))
                )
            {
                ResultsPanel.Visible = false;
                ErrorLabel.Visible = true;
                ErrorLabel.Text = "A transaction is required.  Please add BEGIN TRAN and either COMMIT TRAN or ROLLBACK TRAN in the appropriate places";
                return;
            }

            // try to check for proper admin table management
            if (theSQL.ToLower().Contains("adminbranch") && !theSQL.ToLower().Contains("adminadminbranch"))
            {
                ResultsPanel.Visible = false;
                ErrorLabel.Visible = true;
                ErrorLabel.Text = "The AdminBranch table is being used.  Please verify the AdminAdminBranch table is properly managed.";
                return;
            }
            if (theSQL.ToLower().Contains("insurcarrier") && !theSQL.ToLower().Contains("admininsurcarrier"))
            {
                ResultsPanel.Visible = false;
                ErrorLabel.Visible = true;
                ErrorLabel.Text = "The InsurCarrier table is being used.  Please verify the AdminInsurCarrier table is properly managed.";
                return;
            }
            if (theSQL.ToLower().Contains("integritycheckrule") && !theSQL.ToLower().Contains("adminintegritycheckrule"))
            {
                ResultsPanel.Visible = false;
                ErrorLabel.Visible = true;
                ErrorLabel.Text = "The IntegrityCheckRule table is being used.  Please verify the AdminIntegrityCheckRule table is properly managed.";
                return;
            }
            if (theSQL.ToLower().Contains("list") && !theSQL.ToLower().Contains("adminlist"))
            {
                ResultsPanel.Visible = false;
                ErrorLabel.Visible = true;
                ErrorLabel.Text = "The List table is being used.  Please verify the AdminList table is properly managed.";
                return;
            }
            if (theSQL.ToLower().Contains("prefill") && !theSQL.ToLower().Contains("adminprefill"))
            {
                ResultsPanel.Visible = false;
                ErrorLabel.Visible = true;
                ErrorLabel.Text = "The PreFill table is being used.  Please verify the AdminPreFill table is properly managed.";
                return;
            }
            if (theSQL.ToLower().Contains("subprodquestion") && !theSQL.ToLower().Contains("adminsubprodquestion"))
            {
                ResultsPanel.Visible = false;
                ErrorLabel.Visible = true;
                ErrorLabel.Text = "The v table is being used.  Please verify the AdminSubProdQuestion table is properly managed.";
                return;
            }
            if (theSQL.ToLower().Contains("subproduct") && !theSQL.ToLower().Contains("adminsubproduct"))
            {
                ResultsPanel.Visible = false;
                ErrorLabel.Visible = true;
                ErrorLabel.Text = "The SubProduct table is being used.  Please verify the AdminSubProduct table is properly managed.";
                return;
            }
            if (theSQL.ToLower().Contains("vdn") && !theSQL.ToLower().Contains("adminvdn"))
            {
                ResultsPanel.Visible = false;
                ErrorLabel.Visible = true;
                ErrorLabel.Text = "The VDN table is being used.  Please verify the AdminVDN table is properly managed.";
                return;
            }

            DataTable[] tables = null;
            StringBuilder sb = new StringBuilder();

            try
            {
                //TransactionScope scope = new TransactionScope();
                //using (scope)
                //{
                sb.AppendLine("DECLARE @WRID uniqueidentifier");
                sb.AppendLine("SET @WRID = '" + wrID + "'");
                sb.AppendLine(string.Empty);

                sb.AppendLine("DECLARE @CPCltID uniqueidentifier");
                sb.AppendLine("SET @CPCltID = '" + wrClient + "'");
                sb.AppendLine(string.Empty);

                sb.AppendLine("DECLARE @TheDateTime datetime");
                sb.AppendLine("SET @TheDateTime = getdate()");
                sb.AppendLine(string.Empty);

                sb.AppendLine(theSQL);

                theSQL = sb.ToString();
                lblSQL.Text = sb.ToString();

                SqlConnection myConnection = null;
                myConnection = new SqlConnection(DbProviderFactories.GetFactory().m_sConnectionString);

                ArrayList result = new ArrayList();

                // Add whitespace so the RegEx works
                theSQL += "\r\n";

                // Split query
                Regex regex = new Regex("[\r\n][gG][oO][\r\n]");

                MatchCollection matches = regex.Matches(theSQL);
                int prevIndex = 0;
                string tquery;

                for (int i = 0; i < matches.Count; i++)
                {
                    Match m = matches[i];

                    tquery = theSQL.Substring(prevIndex, m.Index - prevIndex);

                    if (tquery.Trim().Length > 0)
                    {
                        SqlDataAdapter myCommand = new SqlDataAdapter(tquery.Trim(), myConnection);

                        DataSet singleresult = new DataSet();
                        myCommand.Fill(singleresult);

                        for (int j = 0; j < singleresult.Tables.Count; j++)
                        {
                            result.Add(singleresult.Tables[j]);
                        }
                    }

                    prevIndex = m.Index + 3;
                }

                tquery = theSQL.Substring(prevIndex, theSQL.Length - prevIndex);

                if (tquery.Trim().Length > 0)
                {
                    SqlDataAdapter myCommand = new SqlDataAdapter(tquery.Trim(), myConnection);

                    DataSet singleresult = new DataSet();
                    myCommand.Fill(singleresult);

                    for (int j = 0; j < singleresult.Tables.Count; j++)
                    {
                        result.Add(singleresult.Tables[j]);
                    }
                }

                tables = (DataTable[])result.ToArray(typeof(DataTable));

                // update the WR history
                if (wrID != Guid.Empty)
                {
                    Utils.AddWorkRequestHistory(wrID, DateTime.Now, "Script Execution",
                                                "Script successfully executed", theSQL);
                }

                //    scope.Complete();
                //}
            }
            catch (SqlException ex)
            {
                // Show error message
                ResultsPanel.Visible = false;
                ErrorLabel.Visible = true;
                message =
                    "The following error occured while executing the query:<br>\n" +
                    String.Format("Server: Msg {0}, Level {1}, State {2}, Line {3}<br>\n", new object[] { ex.Number, ex.Class, ex.State, ex.LineNumber }) +
                    Server.HtmlEncode(ex.Message).Replace("\n", "<br>") + "<br>\n";
                ErrorLabel.Text = message;
                if (wrID != Guid.Empty)
                {
                    Utils.AddWorkRequestHistory(wrID, DateTime.Now, "Script Execution",
                                                "Script execution failed.  " + message, "Script:  " + theSQL);
                }
            }

            // Print output tables, if they exist
            if (tables != null)
            {
                Label label = new Label();
                //label.Text = string.Empty;
                //ResultsPanel.Controls.Add(label);

                // Loop through all the tables in the DataSet
                for (int i = 0; i < tables.Length; i++)
                {
                    // Only print divider after first table
                    if (i > 0)
                    {
                        // Create new label for grid divider
                        label = new Label();
                        label.Text = "<br><br><hr><br><br>";
                        ResultsPanel.Controls.Add(label);
                    }

                    //DataGrid dataGrid = new DataGrid();
                    //dataGrid.HeaderStyle.CssClass = "tableHeader";
                    //dataGrid.ItemStyle.CssClass = "tableItems";
                    //dataGrid.ItemStyle.Wrap = false;
                    //dataGrid.Width = Unit.Percentage(100);
                    //dataGrid.EnableViewState = false;

                    //dataGrid.PreRender += new EventHandler(DataGrid_PreRender);

                    //dataGrid.DataSource = tables[i];
                    //dataGrid.DataBind();

                    //ResultsPanel.Controls.Add(dataGrid);


                    RadGrid RadGrid1 = new RadGrid();
                    RadGrid1.ID = "RadGrid" + i.ToString();

                    RadGrid1.Width = Unit.Percentage(100);
                    RadGrid1.GridLines = GridLines.Both;
                    RadGrid1.AllowPaging = false;
                    RadGrid1.AutoGenerateColumns = true;
                    RadGrid1.ShowStatusBar = false;

                    RadGrid1.AllowMultiRowSelection = true;
                    RadGrid1.AllowAutomaticDeletes = false;
                    RadGrid1.AllowAutomaticInserts = false;
                    RadGrid1.AllowAutomaticUpdates = false;

                    RadGrid1.MasterTableView.Width = Unit.Percentage(100);

                    RadGrid1.DataSource = tables[i];
                    RadGrid1.DataBind();

                    ResultsPanel.Controls.Add(RadGrid1);
                }

                ResultsPanel.Visible = true;
                ErrorLabel.Visible = false;
            }
        }

        private void DataGrid_PreRender(object sender, EventArgs e)
        {
            // Set the wrapping style of all the cells based on the checkbox, and HTML encode all the cell contents

            DataGrid d = (DataGrid)sender;

            foreach (DataGridItem item in d.Items)
            {
                foreach (TableCell t in item.Cells)
                {
                    t.Wrap = WrapCheckBox.Checked;
                    t.Text = Server.HtmlEncode(t.Text);
                }
            }
        }

        [WebMethod]
        [ScriptMethod]
        public static string GetWRShortID(string wrid)
        {
            string result = string.Empty;
            try
            {
                result = Utils.GetWorkRequestShortID(new Guid(wrid));
            }
            catch (Exception)
            {
                result = string.Empty;
            }
            return result;
        }

    }
}
