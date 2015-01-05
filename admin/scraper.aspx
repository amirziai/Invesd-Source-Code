<%@ Page Language="C#" AutoEventWireup="true" CodeFile="scraper.aspx.cs" Inherits="scraper" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        
        <asp:Button runat="server" ID="b3" Text="Quick Scrape" ToolTip="1st two pages only" OnClick="quickie" />
        
        <asp:DropDownList runat="server" ID="urlx">
            <asp:ListItem Selected="True" Value="long-ideas" Text="Long Ideas"></asp:ListItem>
            <asp:ListItem Value="quick-picks-lists" Text="Quick Picks Lists"></asp:ListItem>
            <asp:ListItem Value="fund-holdings" Text="Fund Holdings"></asp:ListItem>
            <asp:ListItem Value="cramers-picks" Text="Cramer's Picks"></asp:ListItem>
            <asp:ListItem Value="short-ideas" Text="Short Ideas"></asp:ListItem>
            <asp:ListItem Value="insider-ownership" Text="Insider Ownership"></asp:ListItem>
            <asp:ListItem Value="ipo-analysis" Text="IPO Analysis"></asp:ListItem>
            <asp:ListItem Value="options" Text="Options"></asp:ListItem>
        </asp:DropDownList>

        <asp:Table ID="Table1" runat="server">
            <asp:TableRow ID="TableRow1" runat="server">
                <asp:TableCell ID="TableCell1" runat="server">Start page:</asp:TableCell>
                <asp:TableCell ID="TableCell2" runat="server"><asp:TextBox runat="server" ID="startpage"></asp:TextBox></asp:TableCell>
            </asp:TableRow>
            <asp:TableRow ID="TableRow2" runat="server">
                <asp:TableCell ID="TableCell3" runat="server">End page:</asp:TableCell>
                <asp:TableCell ID="TableCell4" runat="server"><asp:TextBox runat="server" ID="endpage"></asp:TextBox></asp:TableCell>
            </asp:TableRow>
        </asp:Table>
        <asp:Button runat="server" ID="b1" Text="Start" OnClick="b1click" />
        <br />
        <br />
        <asp:Button runat="server" ID="b2" OnClick="toadd" Text="Temp Adder" />&nbsp;<asp:Button runat="server" onclick="gotofilter" Text="Temp Filter" id="bfilter" />
    </div>
    </form>
</body>
</html>