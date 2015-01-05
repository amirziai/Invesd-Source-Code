<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Action.aspx.cs" Inherits="Update_Actions" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Table runat="server">
            <asp:TableRow runat="server">
                <asp:TableCell ID="TableCell2" runat="server">
                    <asp:Button runat="server" ID="bu" Text="Update" OnClick="uv"  />&nbsp;<asp:Button runat="server" ID="bdiv" Text="Dividend Yield" OnClick="dividend_yield" />
                    &nbsp;<asp:Button runat="server" ID="bdyhoo" Text="Dividend Y!" OnClick="dividend_yahoo" />
                </asp:TableCell>
                <asp:TableCell ID="TableCell3" runat="server">
                    <asp:Label runat="server" ID="l_update"></asp:Label>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow runat="server">
                <asp:TableCell runat="server">
                    <asp:Button runat="server" ID="br" Text="Reset" OnClick="ra"  />
                </asp:TableCell>
                <asp:TableCell ID="TableCell1" runat="server">
                    <asp:Label runat="server" ID="l_reset"></asp:Label>
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
        
        
    </div>
    <asp:GridView ID="GridView1" runat="server" CellPadding="4" ForeColor="#333333" 
        GridLines="None">
        <AlternatingRowStyle BackColor="White" />
        <EditRowStyle BackColor="#2461BF" />
        <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="#EFF3FB" />
        <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
        <SortedAscendingCellStyle BackColor="#F5F7FB" />
        <SortedAscendingHeaderStyle BackColor="#6D95E1" />
        <SortedDescendingCellStyle BackColor="#E9EBEF" />
        <SortedDescendingHeaderStyle BackColor="#4870BE" />
    </asp:GridView>

    <asp:DropDownList runat="server" ID="filter_pages" AutoPostBack="true">
        <asp:ListItem Value="2">2</asp:ListItem>
        <asp:ListItem Value="5">5</asp:ListItem>
        <asp:ListItem Value="10">10</asp:ListItem>
        <asp:ListItem Value="20" Selected="True">20</asp:ListItem>
        <asp:ListItem Value="50">50</asp:ListItem>
        <asp:ListItem Value="100">100</asp:ListItem>
        <asp:ListItem Value="200">200</asp:ListItem>
        <asp:ListItem Value="300">300</asp:ListItem>
        <asp:ListItem Value="500">500</asp:ListItem>
        <asp:ListItem Value="1000">1000</asp:ListItem>
    </asp:DropDownList>&nbsp;
    <asp:DropDownList runat="server" ID="filter" AutoPostBack="true">
        <asp:ListItem>All</asp:ListItem>
        <asp:ListItem>Breached</asp:ListItem>
        <asp:ListItem>Expired</asp:ListItem>
        <asp:ListItem>Matured</asp:ListItem>
        <asp:ListItem>Active</asp:ListItem>
        <asp:ListItem>Inactive</asp:ListItem>
    </asp:DropDownList>&nbsp;Dividend><asp:TextBox runat="server" ID="tb_div" AutoPostBack="true" Width="50"></asp:TextBox>
    &nbsp;Ticker: <asp:TextBox runat="server" ID="tb_ticker" AutoPostBack="true" Width="50"></asp:TextBox>
    &nbsp;Author: <asp:TextBox runat="server" ID="tb_author" AutoPostBack="true" Width="100"></asp:TextBox>
    &nbsp;<asp:TextBox runat="server" ID="tb_irr_r_lower" AutoPostBack="true" Width="50"></asp:TextBox> < IRRr < <asp:TextBox runat="server" ID="tb_irr_r_upper" AutoPostBack="true" Width="50"></asp:TextBox>
    &nbsp;<asp:TextBox runat="server" ID="tb_irr_t_lower" AutoPostBack="true" Width="50"></asp:TextBox> < IRRt < <asp:TextBox runat="server" ID="tb_irr_t_upper" AutoPostBack="true" Width="50"></asp:TextBox>
    &nbsp;<asp:TextBox runat="server" ID="tb_ret_r_lower" AutoPostBack="true" Width="50"></asp:TextBox> < RETr < <asp:TextBox runat="server" ID="tb_ret_r_upper" AutoPostBack="true" Width="50"></asp:TextBox>
    &nbsp;<asp:TextBox runat="server" ID="tb_ret_t_lower" AutoPostBack="true" Width="50"></asp:TextBox> < RETt < <asp:TextBox runat="server" ID="tb_ret_t_upper" AutoPostBack="true" Width="50"></asp:TextBox>
    &nbsp;ID: <asp:TextBox runat="server" ID="tb_id" AutoPostBack="true" Width="50"></asp:TextBox>

    <asp:Label runat="server" ID="rowshere"></asp:Label> actions<br />
    <br />
    <asp:Table ID="Table1" runat="server" GridLines="Both">
        <asp:TableHeaderRow>
            <asp:TableHeaderCell runat="server"></asp:TableHeaderCell>
            <asp:TableHeaderCell ID="TableHeaderCell1" runat="server">Total</asp:TableHeaderCell>
            <asp:TableHeaderCell ID="TableHeaderCell2" runat="server">Positive</asp:TableHeaderCell>
            <asp:TableHeaderCell ID="TableHeaderCell3" runat="server">Negative</asp:TableHeaderCell>
        </asp:TableHeaderRow>
        <asp:TableRow ID="TableRow1" runat="server">
            <asp:TableCell ID="TableCell4" runat="server">
                Count:
            </asp:TableCell>
            <asp:TableCell ID="TableCell5" runat="server">
                <asp:Label runat="server" ID="s_count"></asp:Label>
            </asp:TableCell>
            <asp:TableCell ID="TableCell14" runat="server">
                <asp:Label runat="server" ID="s_winners"></asp:Label>
            </asp:TableCell>
            <asp:TableCell ID="TableCell15" runat="server">
                <asp:Label runat="server" ID="s_losers"></asp:Label>
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow ID="TableRow2" runat="server">
            <asp:TableCell ID="TableCell6" runat="server">
                Average Realized:
            </asp:TableCell>
            <asp:TableCell ID="TableCell7" runat="server">
                <asp:Label runat="server" ID="s_realized"></asp:Label>
            </asp:TableCell>
            <asp:TableCell ID="TableCell12" runat="server">
                <asp:Label runat="server" ID="s_avg_winners"></asp:Label>
            </asp:TableCell>
            <asp:TableCell ID="TableCell13" runat="server">
                <asp:Label runat="server" ID="s_avg_losers"></asp:Label>
            </asp:TableCell>
        </asp:TableRow>
        <asp:tableRow ID="TableRow3" runat="server">
            <asp:TableCell ID="TableCell8" runat="server">
                Average Target:
            </asp:TableCell>
            <asp:TableCell ID="TableCell9" runat="server">
                <asp:Label runat="server" ID="s_target"></asp:Label>
            </asp:TableCell>
            <asp:TableCell ID="TableCell16" runat="server">
                <asp:Label runat="server" ID="s_target_pos"></asp:Label>
            </asp:TableCell>
            <asp:TableCell ID="TableCell17" runat="server">
                <asp:Label runat="server" ID="s_target_neg"></asp:Label>
            </asp:TableCell>
        </asp:tableRow>
        <asp:tableRow ID="TableRow4" runat="server">
            <asp:TableCell ID="TableCell10" runat="server">
                Realized/Target:
            </asp:TableCell>
            <asp:TableCell ID="TableCell11" runat="server">
                <asp:Label runat="server" ID="s_ratio"></asp:Label>
            </asp:TableCell>
            <asp:TableCell ID="TableCell18" runat="server">
                <asp:Label runat="server" ID="s_ratio_p"></asp:Label>
            </asp:TableCell>
            <asp:TableCell ID="TableCell19" runat="server">
                <asp:Label runat="server" ID="s_ratio_n"></asp:Label>
            </asp:TableCell>
        </asp:tableRow>
        <asp:tableRow ID="TableRow5" runat="server">
            <asp:TableCell ID="TableCell20" runat="server">
                Return realized:
            </asp:TableCell>
            <asp:TableCell ID="TableCell21" runat="server">
                <asp:Label runat="server" ID="s_return_realized_total"></asp:Label>
            </asp:TableCell>
            <asp:TableCell ID="TableCell22" runat="server">
                <asp:Label runat="server" ID="s_return_realized_positive"></asp:Label>
            </asp:TableCell>
            <asp:TableCell ID="TableCell23" runat="server">
                <asp:Label runat="server" ID="s_return_realized_negative"></asp:Label>
            </asp:TableCell>
        </asp:tableRow>
        <asp:tableRow ID="TableRow6" runat="server">
            <asp:TableCell ID="TableCell24" runat="server">
                Return target:
            </asp:TableCell>
            <asp:TableCell ID="TableCell25" runat="server">
                <asp:Label runat="server" ID="s_return_target_total"></asp:Label>
            </asp:TableCell>
            <asp:TableCell ID="TableCell26" runat="server">
                <asp:Label runat="server" ID="s_return_target_positive"></asp:Label>
            </asp:TableCell>
            <asp:TableCell ID="TableCell27" runat="server">
                <asp:Label runat="server" ID="s_return_target_negative"></asp:Label>
            </asp:TableCell>
        </asp:tableRow>
        <asp:tableRow ID="TableRow7" runat="server">
            <asp:TableCell ID="TableCell28" runat="server">
                Return realized/target:
            </asp:TableCell>
            <asp:TableCell ID="TableCell29" runat="server">
                <asp:Label runat="server" ID="s_return_ratio_total"></asp:Label>
            </asp:TableCell>
            <asp:TableCell ID="TableCell30" runat="server">
                <asp:Label runat="server" ID="s_return_ratio_positive"></asp:Label>
            </asp:TableCell>
            <asp:TableCell ID="TableCell31" runat="server">
                <asp:Label runat="server" ID="s_return_ratio_negative"></asp:Label>
            </asp:TableCell>
        </asp:tableRow>

    </asp:Table>
    <br />
    <asp:GridView ID="GridView2" runat="server" AllowPaging="True" OnDataBound="gvdb"
        AllowSorting="True" AutoGenerateColumns="False" CellPadding="4" 
        DataSourceID="LinqDataSource1" ForeColor="#333333" GridLines="None">
        <AlternatingRowStyle BackColor="White" />
        <Columns>
            <asp:BoundField DataField="actionID" HeaderText="ID" ReadOnly="True" 
                SortExpression="actionID" />
            <asp:CheckBoxField DataField="TotalReturn" HeaderText="Total" ReadOnly="True" 
                SortExpression="TotalReturn" />
            <asp:BoundField DataField="creationTime" DataFormatString="{0:d}" 
                HeaderText="Created" ReadOnly="True" SortExpression="creationTime" />
            <asp:BoundField DataField="actualStartDate" DataFormatString="{0:d}" 
                HeaderText="Actual Start" ReadOnly="True" SortExpression="actualStartDate" />
            <asp:BoundField DataField="author" HeaderText="Author" 
                SortExpression="author" />
            <asp:BoundField DataField="ticker" HeaderText="Ticker" 
                SortExpression="ticker" />
            <asp:BoundField DataField="title" HeaderText="Title" SortExpression="title" />
            <asp:BoundField DataField="minValue" DataFormatString="{0:c}" HeaderText="Min" 
                ReadOnly="True" SortExpression="minValue" />
            <asp:BoundField DataField="maxValue" DataFormatString="{0:c}" HeaderText="Max" 
                ReadOnly="True" SortExpression="maxValue" />
            <asp:BoundField DataField="lowerValue" DataFormatString="{0:c}" 
                HeaderText="Low" ReadOnly="True" SortExpression="lowerValue" />
            <asp:BoundField DataField="startValue" DataFormatString="{0:c}" 
                HeaderText="Start" ReadOnly="True" SortExpression="startValue">
            <ItemStyle BackColor="#99FF66" />
            </asp:BoundField>
            <asp:BoundField DataField="currentValue" DataFormatString="{0:c}" 
                HeaderText="Current" ReadOnly="True" SortExpression="currentValue">
            <ItemStyle BackColor="#99FF66" />
            </asp:BoundField>
            <asp:BoundField DataField="targetValue" DataFormatString="{0:c}" 
                HeaderText="Target" ReadOnly="True" SortExpression="targetValue">
            <ControlStyle BackColor="#99FF66" />
            <ItemStyle BackColor="#99FF66" />
            </asp:BoundField>
            <asp:BoundField DataField="dividend" DataFormatString="{0:c}" 
                HeaderText="Dividend" ReadOnly="True" SortExpression="dividend" />
            <asp:BoundField DataField="startDate" DataFormatString="{0:d}" 
                HeaderText="Start" ReadOnly="True" SortExpression="startDate">
            <ItemStyle BackColor="#66FFFF" />
            </asp:BoundField>
            <asp:BoundField DataField="lastUpdated" DataFormatString="{0:d}" 
                HeaderText="Updated" ReadOnly="True" SortExpression="lastUpdated">
            <ItemStyle BackColor="#66FFFF" />
            </asp:BoundField>
            <asp:BoundField DataField="targetDate" DataFormatString="{0:d}" 
                HeaderText="Target" ReadOnly="True" SortExpression="targetDate">
            <ItemStyle BackColor="#66FFFF" />
            </asp:BoundField>
            <asp:CheckBoxField DataField="breached" HeaderText="Breached" ReadOnly="True" 
                SortExpression="breached" />
            <asp:CheckBoxField DataField="expired" HeaderText="Expired" ReadOnly="True" 
                SortExpression="expired" />
            <asp:CheckBoxField DataField="matured" HeaderText="Matured" ReadOnly="True" 
                SortExpression="matured" />
            
        </Columns>
        <FooterStyle BackColor="#990000" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#990000" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#FFCC66" ForeColor="#333333" HorizontalAlign="Center" />
        <RowStyle BackColor="#FFFBD6" ForeColor="#333333" />
        <SelectedRowStyle BackColor="#FFCC66" Font-Bold="True" ForeColor="Navy" />
        <SortedAscendingCellStyle BackColor="#FDF5AC" />
        <SortedAscendingHeaderStyle BackColor="#4D0000" />
        <SortedDescendingCellStyle BackColor="#FCF6C0" />
        <SortedDescendingHeaderStyle BackColor="#820000" />
    </asp:GridView>

    <asp:LinqDataSource ID="LinqDataSource1" runat="server" OnSelected="linq_selected"
        ContextTypeName="DataClassesDataContext" EntityTypeName="" 
        Select="new (actionID, creationTime, article1.title as title, (article1.user.firstname + ' ' + article1.user.lastname) as author, ticker, targetDate, targetValue, currentValue, startDate, lastUpdated, startValue, matured, expired, breached, lowerValue, maxValue, minValue, actualStartDate, dividend, TotalReturn)" 
        TableName="Actions">
    </asp:LinqDataSource>



    </form>
</body>
</html>
