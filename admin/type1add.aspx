<%@ Page Language="C#" AutoEventWireup="true" CodeFile="type1add.aspx.cs" Inherits="admin_type1add" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">

    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>

    <div>
        <asp:Label runat="server" ID="ltitle" Font-Bold="true"></asp:Label>
        <br />
        <asp:Label runat="server" ID="lactioncount" Font-Bold="true"></asp:Label> actions found for this article
        <asp:Table ID="Table1" runat="server">
            <asp:TableRow runat="server">
                <asp:TableCell runat="server">
                    Stock
                </asp:TableCell>
                <asp:TableCell ID="TableCell11" runat="server">
                    <asp:DropDownList runat="server" ID="stock" OnSelectedIndexChanged="stockchanged" AutoPostBack="true"></asp:DropDownList>
                    or <asp:TextBox runat="server" ID="mticker" size="5" OnTextChanged="mtickerchanged" AutoPostBack="true"></asp:TextBox>&nbsp;<asp:Button runat="server" Text="Add" OnClick="addstock" ID="buttona" />
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow ID="TableRow1" runat="server">
                <asp:TableCell ID="TableCell1" runat="server">
                    Target date
                </asp:TableCell>
                <asp:TableCell ID="TableCell2" runat="server">
                    <asp:TextBox runat="server" ID="targetdate"></asp:TextBox> YYYY-MM-DD
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow ID="TableRow2" runat="server">
                <asp:TableCell ID="TableCell3" runat="server">
                    Target %
                </asp:TableCell>
                <asp:TableCell ID="TableCell4" runat="server">
                    <asp:TextBox runat="server" ID="targetpercent" OnTextChanged="percentchanged" AutoPostBack="true"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow ID="TableRow3" runat="server">
                <asp:TableCell ID="TableCell5" runat="server">
                    Target price
                </asp:TableCell>
                <asp:TableCell ID="TableCell6" runat="server">
                    <asp:TextBox runat="server" ID="targetprice" OnTextChanged="targetchanged" AutoPostBack="true"></asp:TextBox>&nbsp;<asp:CheckBox ID="CheckBox1" runat="server" Checked="true" /> Total return
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow ID="TableRow4" runat="server">
                <asp:TableCell ID="TableCell7" runat="server">
                    Start date
                </asp:TableCell>
                <asp:TableCell ID="TableCell8" runat="server">
                    <asp:TextBox runat="server" ID="startdate"></asp:TextBox> YYYY-MM-DD
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow runat="server">
                <asp:TableCell ID="TableCell9" runat="server">
                    Start price
                </asp:TableCell>
                <asp:TableCell runat="server">
                    <asp:TextBox runat="server" ID="startprice"></asp:TextBox>&nbsp;<asp:Button runat="server" id="b1" OnClick="b1click" Text="Submit" Enabled="false" />
                </asp:TableCell>
            </asp:TableRow>

        </asp:Table>

        <asp:Label runat="server" ID="urlholder" Visible="false"></asp:Label>

        </div>
        </ContentTemplate>

        
        
	</asp:UpdatePanel>

    <iframe id="frame1" runat="server" width="800" height="400"></iframe>

    </form>
</body>
</html>
