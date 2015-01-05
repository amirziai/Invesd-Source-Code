<%@ Page Language="C#" AutoEventWireup="true" CodeFile="sendEmail_investor.aspx.cs" Inherits="sendEmail" MasterPageFile="~/admin/MasterPage_Admin.master" %>

<asp:Content runat="server" ContentPlaceHolderID="head">

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="body">
    <asp:ScriptManager runat="server"></asp:ScriptManager>
    <div class="row">
        <div class="span6">
            <div class="thumbnail">
                <asp:Table runat="server" ID="tbl" CssClass="table table-condensed table-hover table-striped" style="margin-bottom:0">
                    <asp:TableHeaderRow>
                        <asp:TableHeaderCell Width="12%">
                            <span style="display:block;text-align:center">#</span>
                        </asp:TableHeaderCell>
                        <asp:TableHeaderCell Width="40%">
                            Investor
                        </asp:TableHeaderCell>
                        <asp:TableHeaderCell Width="12%">
                            Views
                        </asp:TableHeaderCell>
                        <asp:TableHeaderCell Width="12%">
                            Invited
                        </asp:TableHeaderCell>
                        <asp:TableHeaderCell Width="12%">
                            Signup
                        </asp:TableHeaderCell>
                        <asp:TableHeaderCell Width="12%">
                            Challenge
                        </asp:TableHeaderCell>
                    </asp:TableHeaderRow>
                </asp:Table>
            </div>
        </div>
        <div class="span6 text-left">
            <asp:UpdatePanel ID="upanel" runat="server">
                <ContentTemplate>
                    Existing user
                    <br />
                    <asp:DropDownList runat="server" ID="pickuser" CssClass="span6" OnSelectedIndexChanged="get_user_info" AutoPostBack="true"></asp:DropDownList>
                    Name
                    <asp:TextBox ID="displayNameTb" runat="server" CssClass="span6"></asp:TextBox>
                    E-mail
                    <asp:TextBox ID="TheEmail" runat="server" CssClass="span6"></asp:TextBox>
                    Subject
                    <asp:TextBox runat="server" ID="txt_subject" CssClass="span6" Text="You are invited to join"></asp:TextBox>
                    Message
                    <asp:TextBox ID="txt_message" runat="server" Rows="5" TextMode="MultiLine" CssClass="span6"></asp:TextBox>
                    <asp:Button ID="Send" runat="server" OnClick="Send_Click" Text="Send Invitation" CssClass="btn btn-success btn-large btn-block" />
                    <asp:Label ID="status" runat="server" Text=""></asp:Label>
                </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</asp:Content>

                