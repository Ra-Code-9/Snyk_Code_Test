<%@ Page language="c#" MasterPageFile="~/DefaultView.Master" Codebehind="CustomSQL.aspx.cs" AutoEventWireup="true" Inherits="ClientProfile.Scripts.CustomSQL" %>
<script runat="server">
</script>

<asp:Content ID="cntBody" ContentPlaceHolderID="cntBody" runat="server">

<telerik:RadScriptBlock ID="RadScriptBlock1" runat="server">
	<script type="text/javascript">

	    function OpenScriptPopupWR() {
	        var oWnd = $find("<%=RadWindowWRSelect.ClientID%>");
	        oWnd.setUrl("../WorkRequests/BasicSelection.aspx?clientid=00000000-0000-0000-0000-000000000000");
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

<telerik:RadWindow ID="RadWindowWRSelect" ReloadOnShow="true" DestroyOnClose="false" ShowContentDuringLoad="false" VisibleOnPageLoad="false" runat="server" Modal="true" Behaviors="Close,Move"
     Width="975px" Height="550px" VisibleStatusbar="false" OnClientClose="wrSelectCloseScript">
</telerik:RadWindow>

<telerik:RadAjaxManagerProxy ID="RadAjaxManagerProxy1" runat="server">
    <AjaxSettings>
        <telerik:AjaxSetting AjaxControlID="lnkExecute">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="ExecuteButton" />
                <telerik:AjaxUpdatedControl ControlID="ClearButton" />
                <telerik:AjaxUpdatedControl ControlID="LoadButton" />
                <telerik:AjaxUpdatedControl ControlID="lnkWRLookup" />
                <telerik:AjaxUpdatedControl ControlID="txtWRID" />
                <telerik:AjaxUpdatedControl ControlID="ResultsPanel" />
                <telerik:AjaxUpdatedControl ControlID="ErrorLabel" />                
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="ClearButton">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="QueryTextbox" />
                <telerik:AjaxUpdatedControl ControlID="ResultsPanel" />
                <telerik:AjaxUpdatedControl ControlID="ErrorLabel" /> 
                <telerik:AjaxUpdatedControl ControlID="lblSQL" />                 
            </UpdatedControls>
        </telerik:AjaxSetting>        
    </AjaxSettings>
</telerik:RadAjaxManagerProxy>

<telerik:RadAjaxLoadingPanel ID="RadAjaxLoadingPanel1" runat="server">
</telerik:RadAjaxLoadingPanel>

<asp:ValidationSummary ID="ValidationSummary1" runat="server" ShowSummary="false" ShowMessageBox="true"
    HeaderText="Please correct the following error(s):"  
    DisplayMode="BulletList"
    EnableClientScript="true" />
    
<%@ Register TagPrefix="ClientProfile" Tagname="ModuleHeader" Src="~/_controls/ModuleHeader.ascx" %>
<ClientProfile:ModuleHeader ID="ctlModuleHeader" Module="" Title="Custom SQL" EnablePrint="false" HelpName="client" EnableHelp="true" Runat="Server" />

<table cellspacing="0" cellpadding="0" border="0" width="100%">
    <tr>
        <td align="left" width="100%">
            <table cellspacing="0" cellpadding="0" width="100%" border="0">
                <tr>
                    <td colspan="6">
                        <b>Work Request:</b>
                        &nbsp;
                        <asp:LinkButton ID="lnkWRLookup" Font-Underline="true" Text="WR Lookup"
                            OnClientClick="OpenScriptPopupWR(); return false;" runat="server">
                        </asp:LinkButton>  
                        &nbsp;        
                        <asp:TextBox ID="txtWRID" runat="server"></asp:TextBox>
                    </td>
                </tr>
            </table>
        </td>
    </tr>
</table>
                    
<table cellspacing="0" cellpadding="0" border="0" width="100%">
    <tr>
        <td align="left" width="100%">
            <table cellspacing="0" cellpadding="0" width="100%" border="0">    
                <tr>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td><b>Custom SQL</b></td>
                </tr>
                <tr>
                    <td style="font-size:9px">@WRID, @CPCltID and @TheDateTime variables will be set automatically.</td>
                </tr>
                <tr>
                    <td colspan="6" bgcolor="white">
                        <table cellspacing="0" cellpadding="0" border="0">
                            <tr>
                                <td colspan="6">
                                    <asp:TextBox Font-Names="Courier New" runat="server" TextMode="MultiLine" 
                                        Width="800px" Rows="25" Wrap="false" ID="QueryTextbox">
                                    </asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="3">
                                    <asp:CheckBox runat="server" ID="WrapCheckBox" Checked="True" Text="Wrap cell contents in results">
                                    </asp:CheckBox>
                                </td>
                                <td colspan="4"align="right">
                                    <asp:Button runat="server" Text="Execute" onMouseOver="this.style.color='#808080';"
                                        onMouseOut="this.style.color='#000000';" ID="ExecuteButton" OnClick="ExecuteButton_Click">
                                    </asp:Button>
                                    <asp:Button runat="server" Text="Clear" onMouseOver="this.style.color='#808080';"
                                        onMouseOut="this.style.color='#000000';" ID="ClearButton" OnClick="ClearButton_Click">
                                    </asp:Button>                                        
                                    <asp:Button runat="server" Text="Save query..." onMouseOver="this.style.color='#808080';"
                                        onMouseOut="this.style.color='#000000';" ID="SaveButton" OnClick="SaveButton_Click">
                                    </asp:Button>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>
                        <b>Load SQL File</b>
                        &nbsp;
                        &nbsp;
                        <input id="FileUploadInput" type="file" runat="server" />
                        &nbsp;
                        &nbsp;
                        <asp:Button runat="server" Text="Load file..." onMouseOver="this.style.color='#808080';"
                            onMouseOut="this.style.color='#000000';" ID="LoadButton" OnClick="LoadButton_Click">
                        </asp:Button>
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td><b>Results:</b></td>
                </tr>
                <tr>
                    <td>
                        <asp:Panel runat="server" ID="ResultsPanel">
                        </asp:Panel>
                        <asp:Label ID="ErrorLabel" runat="server" Visible="False" ForeColor="red"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td><b>SQL:</b></td>
                </tr>
                <tr>
                    <td><asp:Label ID="lblSQL" runat="server"></asp:Label></td>
                </tr>                
            </table>
            <br/>
        </td>
    </tr>
</table>

<table id="Table2" cellspacing="2" cellpadding="1" width="100%" border="0" rules="none"
    style="border-collapse: collapse">
    <tr>
        <td align="right" colspan="2">
            <asp:LinkButton ID="btnCancel" Text="Cancel" runat="server" CausesValidation="False"
                TabIndex="9999" CommandName="Cancel" OnCommand="Page_Command">
            </asp:LinkButton>
        </td>
    </tr>
    <tr>
        <td>
            &nbsp;
        </td>
    </tr>
</table>

</asp:Content>
