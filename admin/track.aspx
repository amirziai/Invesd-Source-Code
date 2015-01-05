<%@ Page Language="C#" AutoEventWireup="true" CodeFile="track.aspx.cs" Inherits="admin_track" MasterPageFile="~/admin/MasterPage_Admin.master" %>

<asp:Content runat="server" ContentPlaceHolderID="head">

</asp:Content>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="body">

    <div class="thumbnail">

        <asp:Label runat="server" ID="txt_user_details" stlye="text-align:center"></asp:Label>

        <table class="table table-striped table-hover table-condensed" runat="server" id="tbl" style="margin:0">
            <thead>
                <tr>
                    <td style="width:20%;font-weight:bold;border-top:0">Investor</td>
                    <td style="width:60%;font-weight:bold;border-top:0">Link</td>
                    <td style="width:20%;font-weight:bold;border-top:0">Timestamp</td>
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

    <%--<div class="container">
        <div class="row">
            <div class="span12">
                <div class="thumbnail">
                    <asp:GridView ID="GridView1" runat="server" AllowSorting="True" 
                        DataSourceID="LinqDataSource2" AutoGenerateColumns="False" CssClass="table table-condensed table-striped table-hover" GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="analyst" HeaderText="Investor" ReadOnly="True" 
                                SortExpression="analyst" />
                            <asp:BoundField DataField="hyperlink" HeaderText="Link" ReadOnly="True" 
                                SortExpression="hyperlink" />
                            <asp:BoundField DataField="timestamp" HeaderText="Timestamp" ReadOnly="True" 
                                SortExpression="timestamp" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <!-- DataSource -->
    <asp:LinqDataSource ID="LinqDataSource2" runat="server" 
        ContextTypeName="DataClassesDataContext" EntityTypeName="" 
        Select="new (user.display_name as analyst, timestamp, hyperlink)" 
        TableName="trackings" Where='!hyperlink.Contains("localhost") && analyst!=2 && analyst!=1 && analyst!=2656147' 
        OrderBy="timestamp desc">
    </asp:LinqDataSource>--%>
</asp:Content>