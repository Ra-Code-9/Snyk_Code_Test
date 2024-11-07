<%@ Page language="c#" MasterPageFile="~/DefaultView.Master" Codebehind="view.aspx.cs" AutoEventWireup="true" Inherits="ClientProfile.Scripts.View" %>
<script runat="server">
</script>

<asp:Content ID="cntBody" ContentPlaceHolderID="cntBody" runat="server">

<telerik:RadScriptBlock ID="RadScriptBlock1" runat="server">
	<script type="text/javascript">

	    function OpenScriptPopupWR() {
	        var scriptName = document.getElementById('<%=hscript.ClientID%>').value;
	        var oWnd = $find("<%=RadWindowWRSelect.ClientID%>");
	        oWnd.setUrl("../WorkRequests/BasicSelection.aspx?clientid=00000000-0000-0000-0000-000000000000&secname=uscript&act=" + scriptName);
	        oWnd.show();
	    }
	    function wrSelectCloseScript(sender, args) {
	        if (args.get_argument() != null && args.get_argument() != '') {
	            wrid = args.get_argument();
	            PageMethods.GetWRShortID(wrid, OnSucceeded, OnFailed);
	        }
	        else {
	            radalert('Work Request selection cancelled.', 300, 100, 'Work Request Selection');
	        }
	    }
	    function OnSucceeded(result, context, method) {
	        var wrtextbox = document.getElementById("<%=txtWRID.ClientID%>");
	        wrtextbox.value = result;
	    }
	    function OnFailed(error) {
	        radalert('Please contact IT.  ' + error.get_message(), 300, 100, 'Work Request Lookup');
	    }
	    
	</script>
</telerik:RadScriptBlock>
    
<asp:HiddenField ID="hscript" runat="server"/>

<telerik:RadWindow ID="RadWindowWRSelect" ReloadOnShow="true" DestroyOnClose="false" ShowContentDuringLoad="false" VisibleOnPageLoad="false" runat="server" Modal="true" Behaviors="Close,Move"
     Width="975px" Height="550px" VisibleStatusbar="false" OnClientClose="wrSelectCloseScript">
</telerik:RadWindow>

<telerik:RadAjaxManagerProxy ID="RadAjaxManagerProxy1" runat="server">
    <AjaxSettings>
        <telerik:AjaxSetting AjaxControlID="lnkExecute">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="lnkExecute" />
                <telerik:AjaxUpdatedControl ControlID="hControls" />
                <telerik:AjaxUpdatedControl ControlID="lblMessage1" />
                <telerik:AjaxUpdatedControl ControlID="lblMessage2" />
                <telerik:AjaxUpdatedControl ControlID="txtSQL" />
                <telerik:AjaxUpdatedControl ControlID="pnlResults" />
                <telerik:AjaxUpdatedControl ControlID="lnkWRLookup" />
                <telerik:AjaxUpdatedControl ControlID="txtWRID" />
                <telerik:AjaxUpdatedControl ControlID="Table2" LoadingPanelID="RadAjaxLoadingPanel1" />
            </UpdatedControls>
        </telerik:AjaxSetting>
    </AjaxSettings>
</telerik:RadAjaxManagerProxy>

<telerik:RadAjaxLoadingPanel ID="RadAjaxLoadingPanel1" runat="server">
</telerik:RadAjaxLoadingPanel>
    
  <%@ Register TagPrefix="ClientProfile" Tagname="ModuleHeader" Src="~/_controls/ModuleHeader.ascx" %>
	<ClientProfile:ModuleHeader ID="ctlModuleHeader" Module="" Title='<%# "Script  -  " + Request["sn"] %>' EnablePrint="false" HelpName="client" EnableHelp="true" Runat="Server" />

    <asp:HiddenField ID="hControls" runat="server" />

    <div>
    <table id="Table1" cellspacing="0" cellpadding="0" width="100%" border="0"
        style="BORDER-COLLAPSE: collapse">
        <tr>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td colspan="6">
                &nbsp;
                <asp:Label ID="Label1" runat="server" Text="Bold Items Are Required" Font-Bold="True"></asp:Label>
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
        </tr>
    </table>
    
    <asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder>
    
    <table id="Table3" cellspacing="2" cellpadding="1" width="100%" border="0" rules="none"
        style="BORDER-COLLAPSE: collapse">
        <tr>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td colspan="6">
                <asp:Label ID="Label2" runat="server" Text="Work Request:" Font-Bold="True"></asp:Label>
                <asp:TextBox ID="txtWRID" runat="server"></asp:TextBox>
                <asp:LinkButton ID="lnkWRLookup" Font-Underline="true" 
                    OnClientClick="OpenScriptPopupWR(); return false;" 
                    Text="WR Lookup" runat="server">
                </asp:LinkButton>  
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td colspan="6">
                <asp:LinkButton ID="lnkExecute" Text="Execute Script" OnClientClick="hideButtonNoValCheck(this);" CausesValidation="true"
                            tabIndex="9998" runat="server" CommandName="ExecuteScript" OnCommand="PageCommand">
                </asp:LinkButton>
            </td>
            <td>
                <asp:LinkButton ID="btnCancel" Text="Cancel" runat="server" CausesValidation="False"
                            tabIndex="9999" CommandName="Cancel" OnCommand="PageCommand">
                </asp:LinkButton>
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td colspan="6">
                <asp:Label ID="lblMessage1" runat="server" EnableViewState="False" Font-Bold="True" ForeColor="#FF8080"></asp:Label>
                <asp:Label ID="lblMessage2" runat="server" EnableViewState="False" Font-Bold="True" ForeColor="#00C000"></asp:Label>
            </td>
        </tr>
        <%--<tr>
            <td>&nbsp;</td>
        </tr>--%>
        <%--<tr>
            <td><b>SQL:</b></td>
        </tr>--%>
        <%--<tr>
            <td colspan="6">
                <asp:TextBox BorderStyle="None" BorderWidth="1px" runat="server" TextMode="MultiLine" 
                    Columns="180" Rows="10" ID="txtSQL">
                </asp:TextBox>
            </td>
        </tr>--%>
        <tr>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td><b>Results:</b></td>
        </tr>
        <tr>
            <td colspan="6"><asp:Panel runat="server" ID="pnlResults"></asp:Panel></td>
        </tr>
    </table>
    </div>

</asp:Content>
