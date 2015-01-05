<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Gripe.aspx.cs" Inherits="admin_Gripe" MasterPageFile="~/admin/MasterPage_Admin.master" %>

<asp:Content runat="server" ContentPlaceHolderID="head">

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="body">
    <div class="text-right">
        <asp:HyperLink runat="server" NavigateUrl="Gripe.aspx">All</asp:HyperLink>&nbsp;<asp:HyperLink runat="server" NavigateUrl="Gripe.aspx?users=true">Users</asp:HyperLink>
    </div>
    <div class="thumbnail">
        <table class="table table-striped table-hover table-condensed" runat="server" id="tbl" style="margin:0">
            <thead>
                <tr>
                    <td style="width:20%;font-weight:bold">Name</td>
                    <td style="width:60%;font-weight:bold">Message</td>
                    <td style="width:15%;font-weight:bold">Date</td>
                    <td style="width:5%"></td>
                </tr>
            </thead>
            <tbody>

            </tbody>
        </table>
    </div>

    <ul class="pager">
        <li>
            <asp:HyperLink runat="server" ID="hyp_previous">Previous</asp:HyperLink>
        </li>
        <li>
            <asp:HyperLink runat="server" ID="hyp_next">Next</asp:HyperLink>
        </li>
    </ul>

</asp:Content>