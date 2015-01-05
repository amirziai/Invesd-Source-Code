<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Action_Manual.aspx.cs" Inherits="admin_Action_Manual" MasterPageFile="~/admin/MasterPage_Admin.master" %>

<asp:Content runat="server" ContentPlaceHolderID="head">

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="body">

    <div class="row">
        <div class="span6">
            <div class="text-center">
                <h4>Step 1</h4>
            </div>
            <div class="thumbnail">
                <table border="0" style="margin-right:auto;margin-left:auto">
                    <tr>
                        <td>
                            Date
                            <br />
                            <asp:TextBox runat="server" CssClass="span5" ID="date"></asp:TextBox>
                            <br />
                            Analyst
                            <br />
                            <asp:DropDownList runat="server" CssClass="span5" ID="analyst"></asp:DropDownList>
                            <br />
                            Tickers
                            <br />
                            <asp:TextBox runat="server" CssClass="span5" ID="ticker"></asp:TextBox>
                            <br />
                            Summary
                            <br />
                            <asp:TextBox TextMode="MultiLine" runat="server" ID="summary" Rows="5" CssClass="span5"></asp:TextBox>
                            <asp:Button runat="server" ID="btn_article" Text="Article" CssClass="btn btn-block btn-large btn-success" OnClick="add_article" />
                        </td>
                    </tr>
                </table>
            </div>
        </div>

        <div class="span6">
            <div class="text-center">
                <h4>Step 2</h4>
            </div>
            <div class="thumbnail">
                <table border="0" style="margin-right:auto;margin-left:auto">
                    <tr>
                        <td>
                            Ticker ID
                            <br />
                            <asp:TextBox runat="server" CssClass="span5" ID="tickerid"></asp:TextBox>
                            <br />
                            Article
                            <br />
                            <asp:TextBox runat="server" CssClass="span5" ID="articleid"></asp:TextBox>
                            <br />
                            Target
                            <br />
                            <asp:TextBox runat="server" CssClass="span5" ID="target"></asp:TextBox>
                            <br />
                            <asp:Button runat="server" ID="btn_action" Text="Action" CssClass="btn btn-block btn-large btn-success" OnClick="add_action" />
                            <br />
                            Message
                            <br />
                            <asp:TextBox TextMode="MultiLine" runat="server" ID="message" Rows="5" CssClass="span5" Enabled="false"></asp:TextBox>
                        </td>
                    </tr>
                </table>
            </div>
        </div>

    </div>

</asp:Content>