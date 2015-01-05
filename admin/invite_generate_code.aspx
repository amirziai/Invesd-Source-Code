<%@ Page Language="C#" AutoEventWireup="true" CodeFile="invite_generate_code.aspx.cs" Inherits="admin_invite_generate_code" MasterPageFile="~/admin/MasterPage_Admin.master" %>

<asp:Content runat="server" ContentPlaceHolderID="head">

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="body">
    Name: <asp:TextBox runat="server" ID="txt_name"></asp:TextBox>
    <br />
    Name: <asp:TextBox runat="server" ID="txt_email"></asp:TextBox>
    <br />
    <asp:Button runat="server" ID="btn" CssClass="btn btn-success" />
    <br />
    <asp:Label runat="server" ID="link"></asp:Label>
</asp:Content>