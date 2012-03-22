<%@ Page Language="C#" AutoEventWireup="true"  MasterPageFile="~/HelloWorld.master" CodeFile="Default.aspx.cs" Inherits="HelloWorldPage" %>
<%@ Register TagPrefix="uc" TagName="Timeplot" 
    Src="~/Timeplot.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageBody" runat="server">
    

<div id="header">
<h1>Weight Tracker</h1>
<div id="site-nav">
    Welcome <asp:Label ID="Lbl_UserName" runat="server"></asp:Label> | 
    <asp:LinkButton ID="Lnk_SwitchAccount" 
                    runat="server" onclick="Lnk_SwitchAccount_Click">Switch Account</asp:LinkButton>
</div>
</div>

<div>
  Weight Values<br />
    <asp:GridView ID="WeightView" runat="server">
    </asp:GridView>

    <asp:Button ID="Btn_ShowWeeklyWeightReadings" runat="Server" 
        Text="Weekly Readings" onclick="Btn_ShowWeeklyWeightReadings_Click"/>
    <uc:Timeplot runat="server" ID="TimeplotView" Visible="false"/>
</div>

<div>
Record today's weight (in lbs):
    <asp:TextBox ID=Txt_Weight runat="server"></asp:TextBox>
    <asp:Button ID="Btn_SubmitWeight" runat="server" Text="Submit" 
        onclick="Btn_SubmitWeight_Click"/>
</div>

</asp:Content>
