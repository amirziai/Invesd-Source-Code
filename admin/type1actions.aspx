
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="type1actions.aspx.cs" Inherits="admin_type1actions" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="<%=Page.ResolveUrl("~/") %>css/bootstrap.css" rel="stylesheet" media="screen" />
    <link href="<%= Page.ResolveUrl("~/")%>css/investor.css" rel="stylesheet" media="screen" />
    <link href="<%=Page.ResolveUrl("~/") %>css/font-awesome.min.css" rel="stylesheet" media="screen" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:GridView runat="server" DataSourceID="LinqDataSource1" AutoGenerateColumns="false" PageSize="10" AllowPaging="true">
            <Columns>
                <asp:BoundField DataField="date" />
                <asp:HyperLinkField DataNavigateUrlFields="url" DataTextField="title" Target="_blank" />
                <asp:BoundField DataField="author" />
                <asp:BoundField DataField="origin" />
                <asp:TemplateField>
                    <ItemTemplate>
                        <%#get_tickers(Convert.ToInt32(Eval("idarticle")),Convert.ToDateTime(Eval("date"))) %>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
    <asp:LinqDataSource ID="LinqDataSource1" runat="server" 
        ContextTypeName="DataClassesDataContext" 
        EntityTypeName="" 
        Select="new (idarticle,url, title, date, type, action, deleted, is_ticket, ticketOrArticleNumber, not_actionable, Publish,user.display_name as author,origin)" 
        Where="type==1 and action==false" OrderBy="date desc"
        TableName="articles">
    </asp:LinqDataSource>
    </form>
</body>
</html>

