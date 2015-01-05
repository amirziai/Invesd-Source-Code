<%@ Page Language="C#" AutoEventWireup="true" CodeFile="email_user.aspx.cs" Inherits="admin_email_user" MasterPageFile="~/admin/MasterPage_Admin.master" ValidateRequest="false" %>

<asp:Content runat="server" ContentPlaceHolderID="head">

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="body">
    <div class="span12" class="text-center">
        From:
        <asp:TextBox runat="server" ID="txt_from"></asp:TextBox>

        E-mail:
        <asp:TextBox runat="server" ID="txt_from_email"></asp:TextBox>
        
        <br />
        Name:
        <asp:TextBox runat="server" ID="txt_name"></asp:TextBox>

        <br />
        E-mail:
        <asp:TextBox runat="server" ID="txt_email"></asp:TextBox>

        <br />
        Subject:
        <asp:TextBox runat="server" ID="txt_subject"></asp:TextBox>

        <br />
        Message:
        <asp:TextBox TextMode="MultiLine" Rows="5" ID="txt_message" runat="server"></asp:TextBox>

        <br />
        Button:
        <asp:TextBox runat="server" ID="txt_button"></asp:TextBox>

        <br />
        Link:
        <asp:TextBox runat="server" ID="txt_link"></asp:TextBox>

        <br />
        <asp:Button runat="server" ID="btn" Text="Send" CssClass="btn btn-success" OnClick="btn_click" />

        <br />
        <asp:Label runat="server" ID="status"></asp:Label>
    </div>
</asp:Content>