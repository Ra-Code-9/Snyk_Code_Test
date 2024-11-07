using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI.WebControls;
using ClientProfile.Data.ClientProfile;
using Telerik.Web.UI;

namespace ClientProfile.Scripts
{
  /// <summary>
  ///   Summary description for View.
  /// </summary>
  public partial class View : ClientProfilePage
  {
    private int gID;
    private string gScript;

    private void DataGrid_PreRender(object sender, EventArgs e) {
      // Set the wrapping style of all the cells based on the checkbox, and HTML encode all the cell contents

      var d = (DataGrid) sender;

      foreach (DataGridItem item in d.Items) {
        foreach (TableCell t in item.Cells) {
          t.Wrap = true;
          t.Text = Server.HtmlEncode(t.Text);
        }
      }
    }

    private void DisplayMessage(bool isError, string text) {
      var label = (isError) ? lblMessage1 : lblMessage2;
      label.Text = text;
    }

    private string GetConnectionString(string env, string scriptType, Guid tenantId) {
      if (scriptType.ToLower() == "netclaim") {
        var connNcString = string.Empty;
        switch (env) {
          case "Development":
            connNcString = ConfigurationManager.ConnectionStrings["NCConnectionStringDev"].ConnectionString;
            break;
          case "QA":
            connNcString = ConfigurationManager.ConnectionStrings["NCConnectionStringQA"].ConnectionString;
            break;
          case "Training":
            connNcString = ConfigurationManager.ConnectionStrings["NCConnectionStringDemo"].ConnectionString;
            break;
          case "Production":
            connNcString = ConfigurationManager.ConnectionStrings["NCConnectionStringProd"].ConnectionString;
            break;
          default:
            throw new Exception("Invalid environment - " + env);
        }
        return connNcString;
      }
      if (scriptType.ToLower() == "enterprise") {
        var connEnterpriseString = string.Empty;
        switch (env) {
          case "ITQA":
            connEnterpriseString = ConfigurationManager.ConnectionStrings["TNWEntConnectionStringQA"].ConnectionString;
            break;
          case "Production":
            connEnterpriseString = ConfigurationManager.ConnectionStrings["TNWEntConnectionStringProd"].ConnectionString;
            break;
          default:
            throw new Exception("Invalid environment - " + env);
        }
        return connEnterpriseString;
      }
      if (scriptType.ToLower() == "cm tenant") {
        var connTsString = string.Empty;
        switch (env) {
          case "ITQA":
            connTsString = ConfigurationManager.ConnectionStrings["TenantServicesConnectionStringQA"].ConnectionString;
            break;
          case "Production":
            connTsString = ConfigurationManager.ConnectionStrings["TenantServicesConnectionStringProd"].ConnectionString;
            break;
          default:
            throw new Exception("Invalid environment - " + env);
        }
        var programId = Utils.GetCmTenantProgramId(tenantId, connTsString);
        return Utils.GetTenantConnection(tenantId, programId, connTsString);
      }
      return string.Empty;
    }

    [WebMethod, ScriptMethod]
    public static string GetWRShortID(string wrid) {
      var result = string.Empty;
      try {
        result = Utils.GetWorkRequestShortID(new Guid(wrid));
      }
      catch (Exception) {
        result = string.Empty;
      }
      return result;
    }

    private void LoadParameterFields() {
      var theParms = new List<ScriptParam>();
      var db = new CPDataContext(DbProviderFactories.GetFactory().m_sConnectionString);
      var parms = from s in db.ScriptParams
                  where s.ScriptID == gID
                  orderby s.Seq
                  select s;
      foreach (var parm in parms) {
        theParms.Add(parm);
      }
      db.Dispose();

      var tblMain = new Table();

      var sb = new StringBuilder();

      var dbx = new CPDataContext(DbProviderFactories.GetFactory().m_sConnectionString);

      foreach (var parm in theParms) {
        TableCell cellData = null;

//        var rowMain = new TableRow();
        var cellMain = new TableCell();
        var tblDetail = new Table();
        tblDetail.Width = Unit.Percentage(100);
        var rowDetail = new TableRow();

//        rowMain.VerticalAlign = VerticalAlign.Top;

        var cellLabel = new TableCell();
        var label = new Label();
        label.Text = parm.ParamName + ":"; //"  (" + parm.Description + ") ";
        cellLabel.Font.Bold = parm.Required;
        cellLabel.Wrap = false;
        cellLabel.Controls.Add(label);
        cellLabel.HorizontalAlign = HorizontalAlign.Left;
        cellLabel.VerticalAlign = VerticalAlign.Top;
        rowDetail.Cells.Add(cellLabel);

        switch (parm.ControlType) {
          case 1: // Text box
          {
            var textBox = new TextBox();
            textBox.ID = "txt-" + parm.ID;
            sb.Append(textBox.ID);
            sb.Append("~");
            cellData = new TableCell();
            cellData.Controls.Add(textBox);
            break;
          }
          case 2: // Client drop down list
          {
            var ddlMethod = new DropDownList();
            ddlMethod.ID = "ddl-" + parm.ID;
            sb.Append(ddlMethod.ID);
            sb.Append("~");

            var item = new ListItem {Text = "Select a client", Value = string.Empty};
            ddlMethod.Items.Add(item);

            var clients = from s in dbx.Clients
                          orderby s.DisplayName
                          select s;
            foreach (var client in clients) {
              var listItem = new ListItem();
              listItem.Text = client.DisplayName + " - " + client.NCCltID.ToString();
              listItem.Value = client.NCCltID.ToString();
              ddlMethod.Items.Add(listItem);
            }
            cellData = new TableCell();
            cellData.Controls.Add(ddlMethod);
            break;
          }
          case 3: // Custom script list
          {
            var ddlTable = new DropDownList();
            ddlTable.ID = "ddl-" + parm.ID;
            sb.Append(ddlTable.ID);
            sb.Append("~");
            var listItems = from s in dbx.ScriptLists
                            where s.ListName == parm.ControlSource
                            orderby s.ListItem
                            select s;
            foreach (var listItem in listItems) {
              ddlTable.Items.Add(listItem.ListItem);
            }
            cellData = new TableCell();
            cellData.Controls.Add(ddlTable);
            break;
          }
          case 97: // Tenant drop down list
          {
            var ddlMethod = new DropDownList();
            ddlMethod.ID = "ddl-" + parm.ID;
            sb.Append(ddlMethod.ID);
            sb.Append("~");

            var item = new ListItem {Text = "Select a tenant", Value = string.Empty};
            ddlMethod.Items.Add(item);

            var tenants = from r in dbx.ClientCMs
                          orderby r.TenantName
                          select r;
            foreach (var tenant in tenants) {
              var listItem = new ListItem();
              listItem.Text = tenant.TenantName + " - (" + tenant.NCCltID + ")";
              listItem.Value = tenant.TenantID.ToString();
              ddlMethod.Items.Add(listItem);
            }
            cellData = new TableCell();
            cellData.Controls.Add(ddlMethod);
            break;
          }
          case 98: // User ID
          {
            var textBox = new TextBox();
            textBox.ID = "txt-" + parm.ID;
            sb.Append(textBox.ID);
            sb.Append("~");
            textBox.Text = SecurityUser.SysUserId.ToString();
            textBox.Enabled = false;
            cellData = new TableCell();
            cellData.Controls.Add(textBox);
            break;
          }
          case 99: // Environment
          {
            var ddlTable = new DropDownList();
            ddlTable.ID = "ddl-" + parm.ID;
            sb.Append(ddlTable.ID);
            sb.Append("~");
            var listItems = from s in dbx.ScriptLists
                            where s.ListName == parm.ControlSource
                            select s;
            foreach (var listItem in listItems) {
              ddlTable.Items.Add(listItem.ListItem);
            }
            cellData = new TableCell();
            cellData.Controls.Add(ddlTable);
            break;
          }
        }

        if (cellData != null) {
          cellData.HorizontalAlign = HorizontalAlign.Right;
          cellData.VerticalAlign = VerticalAlign.Top;
          rowDetail.Cells.Add(cellData);
        }

        cellLabel = new TableCell();
        label = new Label();
        label.Text = parm.Description;
        cellLabel.Controls.Add(label);
        cellLabel.HorizontalAlign = HorizontalAlign.Left;
        cellLabel.VerticalAlign = VerticalAlign.Top;
        rowDetail.Cells.Add(cellLabel);
        tblDetail.Rows.Add(rowDetail);
        cellMain.Controls.Add(tblDetail);
        cellMain.HorizontalAlign = HorizontalAlign.Left;
        //rowMain.Cells.Add(cellMain);
        tblMain.Rows.Add(rowDetail);
      }

      PlaceHolder1.Controls.Add(tblMain);
      hControls.Value = sb.ToString().Substring(0, sb.Length - 1);

      Session["ScriptParms"] = theParms;

      dbx.Dispose();
    }

    protected void PageCommand(object sender, CommandEventArgs e) {
      try {
        switch (e.CommandName) {
          case "ExecuteScript": {
            var message = PrepExecution().Split('*');
            DisplayMessage(message[0] != "1", message[1]);
            break;
          }
          case "Cancel": {
            var url = String.Format(@"../Scripts/default.aspx");
            Response.Redirect(url);
            break;
          }
        }
      }
      catch (Exception ex) {
        ClientProfileError.SystemError(new StackTrace(true).GetFrame(0), ex);
      }
    }

    private void Page_Load(object sender, EventArgs e) {
      gID = CpConv.ToInteger(Request["id"]);

      gScript = Request["sn"];
      hscript.Value = gScript;

      Session["secname"] = "utilsscripts";

      LoadParameterFields();

      SetExecuteVisibility();
    }

    private string PrepExecution() {
      var message = string.Empty;

      try {
        var connString = string.Empty;
        var tenantId = Guid.Empty;
        var sb = new StringBuilder();
        var db = new CPDataContext(DbProviderFactories.GetFactory().m_sConnectionString);

        if (txtWRID.Text == string.Empty) {
          throw new Exception("Work Request is required.");
        }

        var wrFound = false;
        int? wrClient = null;
        var wrCpClient = Guid.Empty;
        var wrID = Guid.Empty;
        var workRequests = from s in db.vwWorkRequests
                           where s.ShortID == txtWRID.Text.Trim() && s.StatusID != 8 && s.StatusID != 18
                           select s;
        foreach (var workRequest in workRequests) {
          wrFound = true;
          wrID = workRequest.ID;
          wrCpClient = workRequest.CPCltID;
          wrClient = workRequest.NCCltID;
        }
        if (!wrFound) {
          throw new Exception("Work Request not valid.");
        }
        if (wrClient == null || wrClient == 0) {
          throw new Exception("Work Request does not contain a valid client.");
        }

        sb.AppendLine("DECLARE @SysCltID int");
        sb.AppendLine("SET @SysCltID = " + wrClient);
        sb.AppendLine(string.Empty);

        sb.AppendLine("DECLARE @CPCltID uniqueidentifier");
        sb.AppendLine("SET @CPCltID = '" + wrCpClient + "'");
        sb.AppendLine(string.Empty);

        Script theScript = null;
        var scripts = from s in db.Scripts
                      where s.ID == gID
                      select s;
        foreach (var script in scripts) {
          theScript = script;
        }
        db.Dispose();

        var theParms = (List<ScriptParam>) Session["ScriptParms"];

        var fieldIds = hControls.Value.Split('~');

        foreach (var fieldId in fieldIds) {
          var cntrlId = fieldId.Split('-');

          ScriptParam theParm = null;
          foreach (var parm in theParms) {
            if (parm.ID.ToString() == cntrlId[1]) {
              theParm = parm;
              break;
            }
          }

          var text = PlaceHolder1.FindControl(fieldId) as TextBox;
          if (text != null) {
            if (theParm.Required && text.Text == string.Empty) {
              throw new Exception(theParm.ParamName + " is required.");
            }
            sb.AppendLine("DECLARE @" + theParm.ScriptParamName + " " + theParm.ScriptParamType);
            if (theParm.ScriptParamType.ToLower().Substring(0, 3) == "var" ||
                theParm.ScriptParamType.ToLower().Substring(0, 3) == "nva" ||
                theParm.ScriptParamType.ToLower().Substring(0, 3) == "uni") {
              sb.AppendLine("SET @" + theParm.ScriptParamName + " = '" + text.Text + "'");
            } else {
              sb.AppendLine("SET @" + theParm.ScriptParamName + " = " + text.Text);
            }
          }

          var cb = PlaceHolder1.FindControl(fieldId) as DropDownList;
          if (cb != null) {
            if (theParm.Required && cb.SelectedValue == string.Empty) {
              throw new Exception(theParm.ParamName + " is required.");
            }
            if (theParm.Seq == 97 && theParm.ParamName == "Tenant") {
              tenantId = new Guid(cb.SelectedValue);
            }
            if (theParm.Seq == 99 && theParm.ParamName == "Environment") {
              if (theScript.Type.ToLower() == "netclaim") {
                connString = GetConnectionString(cb.SelectedValue, theScript.Type, Guid.Empty);
              } else if (theScript.Type.ToLower() == "enterprise") {
                connString = GetConnectionString(cb.SelectedValue, theScript.Type, Guid.Empty);
              } else if (theScript.Type.ToLower() == "cm tenant") {
                connString = GetConnectionString(cb.SelectedValue, theScript.Type, tenantId);
                var start = connString.IndexOf("Initial Catalog=") + 16;
                var end = connString.IndexOf(";", start);
                var database = connString.Substring(start, end - start);
                sb.Insert(0, " USE [" + database + "] ");
              } else {
                connString = DbProviderFactories.GetFactory().m_sConnectionString;
              }
            }
            sb.AppendLine("DECLARE @" + theParm.ScriptParamName + " " + theParm.ScriptParamType);
            if (theParm.ScriptParamType.ToLower().Substring(0, 3) == "var" ||
                theParm.ScriptParamType.ToLower().Substring(0, 3) == "nva" ||
                theParm.ScriptParamType.ToLower().Substring(0, 3) == "uni") {
              sb.AppendLine("SET @" + theParm.ScriptParamName + " = '" + cb.SelectedValue + "'");
            } else {
              sb.AppendLine("SET @" + theParm.ScriptParamName + " = " + cb.SelectedValue);
            }
          }
          sb.AppendLine(string.Empty);
        }

        sb.Append(theScript.SQLCode);

        DataTable[] tables = null;

        try {
          var scope = new TransactionScope();
          using (scope) {
            var theSql = sb.ToString();

            SqlConnection myConnection = null;
            myConnection = new SqlConnection(connString);

            var result = new ArrayList();

            // add whitespace so the RegEx works
            theSql += "\r\n";

            // split query
            var regex = new Regex("[\r\n][gG][oO][\r\n]");

            var matches = regex.Matches(theSql);
            var prevIndex = 0;
            string tquery;

            for (var i = 0; i < matches.Count; i++) {
              var m = matches[i];

              tquery = theSql.Substring(prevIndex, m.Index - prevIndex);

              if (tquery.Trim().Length > 0) {
                var myCommand = new SqlDataAdapter(tquery.Trim(), myConnection);

                var singleresult = new DataSet();
                myCommand.Fill(singleresult);

                for (var j = 0; j < singleresult.Tables.Count; j++) {
                  result.Add(singleresult.Tables[j]);
                }
              }

              prevIndex = m.Index + 3;
            }

            tquery = theSql.Substring(prevIndex, theSql.Length - prevIndex);

            if (tquery.Trim().Length > 0) {
              var myCommand = new SqlDataAdapter(tquery.Trim(), myConnection);

              var singleresult = new DataSet();
              myCommand.Fill(singleresult);

              for (var j = 0; j < singleresult.Tables.Count; j++) {
                result.Add(singleresult.Tables[j]);
              }
            }

            tables = (DataTable[]) result.ToArray(typeof (DataTable));

            // update the WR history
            Utils.AddWorkRequestHistory(wrID, DateTime.Now, "Script Execution", gScript, "script successfully executed");

            scope.Complete();
          }
        }
        catch (SqlException ex) {
          pnlResults.Visible = false;
          message =
            "The following error occured while executing the query:<br>\n" +
            String.Format("Server: Msg {0}, Level {1}, State {2}, Line {3}<br>\n", new object[] {ex.Number, ex.Class, ex.State, ex.LineNumber}) +
            Server.HtmlEncode(ex.Message).Replace("\n", "<br>") + "<br>\n";
          Utils.AddWorkRequestHistory(wrID, DateTime.Now, "Script Execution", gScript, "script execution failed.  " + message);
          throw new Exception(message);
        }

        // Print output tables, if they exist
        if (tables != null) {
          // Add header text "Results:"
          var label = new Label {Text = "<br><br>"};
          pnlResults.Controls.Add(label);

          // Loop through all the tables in the DataSet
          for (var i = 0; i < tables.Length; i++) {
            // Only print divider after first table
            if (i > 0) {
              // Create new label for grid divider
              label = new Label {Text = "<br><br><hr><br><br>"};
              pnlResults.Controls.Add(label);
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

            //pnlResults.Controls.Add(dataGrid);


            var radGrid1 = new RadGrid {
              ID = "RadGrid" + i,
              Width = Unit.Percentage(100),
              GridLines = GridLines.Both,
              AllowPaging = false,
              AutoGenerateColumns = true,
              ShowStatusBar = false,
              AllowMultiRowSelection = true,
              AllowAutomaticDeletes = false,
              AllowAutomaticInserts = false,
              AllowAutomaticUpdates = false
            };

            radGrid1.MasterTableView.Width = Unit.Percentage(100);

            radGrid1.DataSource = tables[i];
            radGrid1.DataBind();

            pnlResults.Controls.Add(radGrid1);
          }

          pnlResults.Visible = true;
        }

        message = "1*" + "Script successful";
      }
      catch (Exception ex) {
        message = "0*" + "Script failed. Reason: " + ex.Message;
        ClientProfileError.SystemError(new StackTrace(true).GetFrame(0), ex);
      }
      return message;
    }

    private void SetExecuteVisibility() {
      var canExecute = false;
      var db = new CPDataContext(DbProviderFactories.GetFactory().m_sConnectionString);
      var rollAccesses = from s in db.ScriptRoleAccesses
                         where s.ScriptID == gID
                         select s;
      foreach (var rollAccess in rollAccesses) {
        if (SecurityUser.RoleIDs.Contains(rollAccess.RoleID.ToString())) {
          canExecute = true;
        }
      }
      db.Dispose();

      if (SecurityUser.IsAdmin || canExecute) {
        lnkExecute.Visible = true;
      } else {
        lnkExecute.Visible = false;
      }
    }
  }
}