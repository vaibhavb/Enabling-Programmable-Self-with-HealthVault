<%@ Page Language="C#" AutoEventWireup="true"  MasterPageFile="~/HelloWorld.master" CodeFile="Default.aspx.cs" Inherits="HelloWorldPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageBody" runat="server">
  Weight Values<br />
    <asp:GridView ID="WeightView" runat="server">
    </asp:GridView>

</asp:Content>
