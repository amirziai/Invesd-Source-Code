<%@ Page Language="C#" AutoEventWireup="true" CodeFile="sendEmail.aspx.cs" Inherits="sendEmail" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server"></asp:ScriptManager>
        <asp:UpdatePanel runat="server" ID="up1">
            <ContentTemplate>
                <asp:LinqDataSource ID="LinqDataSource1" runat="server" ContextTypeName="DataClassesDataContext" EntityTypeName="" OnSelecting="LinqDataSource1_Selecting" Select="new ( (firstname.Trim() + ' ' + lastname.Trim()) as Name, userID, email)" TableName="users">
                </asp:LinqDataSource>
                <asp:DropDownList ID="SelectedUser" runat="server" DataSourceID="LinqDataSource1" DataTextField="Name" DataValueField="userID" Height="25px"  Width="255px" AutoPostBack="true" OnSelectedIndexChanged="update_metrics">
                </asp:DropDownList>
                <br />
                <br />
                Enter User Email:&nbsp;
                <asp:TextBox ID="TheEmail" runat="server" Width="261px"></asp:TextBox>
                <br />
                <br />
                <asp:Button ID="Send" runat="server" OnClick="Send_Click" Text="Send Invitation" Width="213px" />
                <br />
                <asp:DropDownList runat="server" ID="dd_type" OnSelectedIndexChanged="change_subject" AutoPostBack="true">
                    <asp:ListItem Value="empty">Clear</asp:ListItem>
                    <asp:ListItem Value="sector">Sector</asp:ListItem>
                    <asp:ListItem Value="stock">Stock</asp:ListItem>
                </asp:DropDownList>
                <br />
                Subject: <asp:TextBox runat="server" ID="txt_subject" Width="400">,</asp:TextBox>
                

                <asp:Label ID="status" runat="server" Text=""></asp:Label>


                <table style="margin-right:auto;margin-left:auto;">
                    <tr>
                        <td style="width:550px;font-size:xx-large;font-weight:bold;font-family:helvetica;letter-spacing:1px;">
                            <img src="http://invesd.com/images/invesd_logo.png" width="130" style="width:130px" alt=""Invesd"">
                        </td>
                    </tr>
                    <tr>
                        <td style="width:550px;font-size:15px;font-weight:100;letter-spacing:1px;font-family:helvetica;background-color:#1b1b1b;color:#cccccc;padding: 6px 0 6px 0;display:block">
                            &nbsp;Don't just invest!
                        </td>
                    </tr>
                    <tr>
                        <td style="width:550px">
                            <p style="font-family:helvetica;margin:10px 0 0px 0">Hello <asp:Label runat="server" ID="l_analyst"></asp:Label>,</p>
                            <p style="font-family:helvetica;margin:10px 0 0px 0">Congratulations! You have been selected as one of the leading analysts to open a pre-launch Invesd account.</p>
                            <p style="font-family:helvetica;margin:10px 0 0px 0">Invesd tracks, quantifies, and ranks the performance of analytical and forward-looking investment content provided by individual analysts and research firms.</p>
                            <div style="width:550px">
                                <p style="text-align:center;margin:10px 0 10px 0">
                                    <a href="https://invesd.com/analyst.aspx?analyst=11248&rid=GQGQISFLCIXPWYLACVCH" style="background: #62c462;
padding-top: 6px;
padding-right: 10px;
padding-bottom: 6px;
padding-left: 10px;
-webkit-border-radius: 4px;
-moz-border-radius: 4px;
border-radius: 4px;
color: #fff;
font-size:14px;
font-weight: 100;
text-decoration: none;
font-family: Helvetica, Arial, sans-serif;">See your detailed performance</a>
                                </p>
                            </div>
                            <table width="550" cellpadding="1" cellspacing="1">
                                <tr>
                                    <td style="text-align:center;width:250px;font-family:helvetica;font-weight:bold;"><strong>Actions</strong></td>
                                    <td width="50">&nbsp;</td>
                                    <td style="text-align:center;width:250px;font-family:helvetica;font-weight:bold;"><strong>Average Return</strong></td>
                                </tr>
                                <tr>
                                    <td style="text-align:center;width:250px;border:1px solid Silver;height:100px;font-size:x-large;font-family:helvetica">
                                        <table border="0" width="100%">
                                            <tr>
                                                <td style="width:50%;font-family:helvetica">
                                                    <asp:Label runat="server" ID="l_actual_actions"></asp:Label>
                                                </td>
                                                <td style="width:50%;font-family:helvetica">
                                                    <asp:Label runat="server" ID="l_actual_articles"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width:50%;font-family:helvetica;font-size:medium">
                                                    Actions
                                                </td>
                                                <td style="width:50%;font-family:helvetica;font-size:medium">
                                                    Articles
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                    <td width="50">&nbsp;</td>
                                    <td style="text-align:center;width:250px;border:1px solid Silver;height:100px;font-size:x-large;font-family:helvetica"> 
                                        <table border="0" width="100%">
                                            <tr>
                                                <td style="width:50%;font-family:helvetica">
                                                    <asp:Label runat="server" ID="l_actual_return"></asp:Label>
                                                </td>
                                                <td style="width:50%;font-family:helvetica">
                                                    <asp:Label runat="server" ID="l_actual_alpha"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width:50%;font-family:helvetica;font-size:medium">
                                                    Total return
                                                </td>
                                                <td style="width:50%;font-family:helvetica;font-size:medium">
                                                    Alpha
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="text-align:center;width:250px;font-family:helvetica;font-weight:bold;">Top Sector</td>
                                    <td width="50">&nbsp;</td>
                                    <td style="text-align:center;width:250px;font-family:helvetica;font-weight:bold;">Top Stock</td>
                                </tr>
                                <tr>
                                    <td style="text-align:center;width:250px;border:1px solid Silver;height:100px;font-size:x-large;font-family:helvetica">
                                        <table border="0" width="100%">
                                            <tr>
                                                <td width="100%" style="text-align:center;width:100%;font-size:large;font-weight:bold" colspan="2"><asp:Label runat="server" ID="l_actual_sector"></asp:Label></td>
                                            </tr>
                                            <tr>
                                                <td colspan="2">&nbsp;</td>
                                            </tr>
                                            <tr>
                                                <td style="width:50%;font-size:x-large"><asp:Label runat="server" ID="l_actual_sector_rank"></asp:Label></td>
                                                <td style="width:50%;font-size:x-large"><asp:Label runat="server" ID="l_actual_sector_return"></asp:Label></td>
                                            </tr>
                                            <tr>
                                                <td style="width:50%;font-size:small">Rank</td>
                                                <td style="width:50%;font-size:small">Average Return</td>
                                            </tr>
                                        </table>
                                    </td>
                                    <td width="50">&nbsp;</td>
                                    <td style="text-align:center;width:250px;border:1px solid Silver;height:100px;font-size:x-large;font-family:helvetica">
                                        <table border="0" width="100%">
                                            <tr>
                                                <td width="100%" style="text-align:center;width:100%;font-size:large;font-weight:bold;" colspan="2"><asp:Label runat="server" ID="l_actual_ticker"></asp:Label></td>
                                            </tr>
                                            <tr>
                                                <td colspan="2">&nbsp;</td>
                                            </tr>
                                            <tr>
                                                <td style="width:50%;font-size:x-large"><asp:Label runat="server" ID="l_actual_ticker_rank"></asp:Label></td>
                                                <td style="width:50%;font-size:x-large"><asp:Label runat="server" ID="l_actual_ticker_return"></asp:Label></td>
                                            </tr>
                                            <tr>
                                                <td style="width:50%;font-size:small">Rank</td>
                                                <td style="width:50%;font-size:small">Average Return</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                            <span>&nbsp;</span>
                            <table style="width:550px;border:1px solid Silver">
                                <tr>
                                    <td style="width:55px;text-align:center;font-weight:bold;font-family:helvetica;font-size:medium;border-bottom:1px solid Silver">Rank</td>
                                    <td style="width:165px;text-align:center;font-weight:bold;font-family:helvetica;font-size:medium;border-bottom:1px solid Silver">Analyst</td>
                                    <td style="width:165px;text-align:center;font-weight:bold;font-family:helvetica;font-size:medium;border-bottom:1px solid Silver">Average Return</td>
                                    <td style="width:165px;text-align:center;font-weight:bold;font-family:helvetica;font-size:medium;border-bottom:1px solid Silver">Actions</td>
                                </tr>
                                <tr>
                                    <td style="text-align:center;font-weight:normal;font-family:helvetica;font-size:medium">1</td>
                                    <td style="text-align:center;font-weight:normal;font-family:helvetica;font-size:medium">?</td>
                                    <td style="text-align:center;font-weight:normal;font-family:helvetica;font-size:medium">?</td>
                                    <td style="text-align:center;font-weight:normal;font-family:helvetica;font-size:medium"><asp:Label runat="server" ID="l_actual_first_actions"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td style="text-align:center;font-weight:normal;font-family:helvetica;font-size:medium">...</td>
                                    <td style="text-align:center;font-weight:normal;font-family:helvetica;font-size:medium">...</td>
                                    <td style="text-align:center;font-weight:normal;font-family:helvetica;font-size:medium">...</td>
                                    <td style="text-align:center;font-weight:normal;font-family:helvetica;font-size:medium">...</td>
                                </tr>
                                <tr>
                                    <td style="text-align:center;font-weight:bold;font-family:helvetica;font-size:medium;background-color:#5fbd5f;color:#ffffff">?</td>
                                    <td style="text-align:center;font-weight:bold;font-family:helvetica;font-size:medium;background-color:#5fbd5f;color:#ffffff"><asp:Label runat="server" ID="l_analyst_name"></asp:Label></td>
                                    <td style="text-align:center;font-weight:bold;font-family:helvetica;font-size:medium;background-color:#5fbd5f;color:#ffffff"><asp:Label runat="server" ID="l_actual_analyst_return"></asp:Label></td>
                                    <td style="text-align:center;font-weight:bold;font-family:helvetica;font-size:medium;background-color:#5fbd5f;color:#ffffff"><asp:Label runat="server" ID="l_actual_analyst_actions"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td style="text-align:center;font-weight:normal;font-family:helvetica;font-size:medium">...</td>
                                    <td style="text-align:center;font-weight:normal;font-family:helvetica;font-size:medium">...</td>
                                    <td style="text-align:center;font-weight:normal;font-family:helvetica;font-size:medium">...</td>
                                    <td style="text-align:center;font-weight:normal;font-family:helvetica;font-size:medium">...</td>
                                </tr>
                                <tr>
                                    <td style="text-align:center;font-weight:normal;font-family:helvetica;font-size:medium"><asp:Label runat="server" ID="l_actual_last"></asp:Label></td>
                                    <td style="text-align:center;font-weight:normal;font-family:helvetica;font-size:medium">?</td>
                                    <td style="text-align:center;font-weight:normal;font-family:helvetica;font-size:medium">?</td>
                                    <td style="text-align:center;font-weight:normal;font-family:helvetica;font-size:medium"><asp:Label runat="server" ID="l_actual_last_actions"></asp:Label></td>
                                </tr>
                            </table>
                            <br />
                            <p style="font-family:helvetica">
                                Your performance is based on your publicly available articles. This is an opportunity to load more free and premium content into your account in order to increase your rankings prior to our public launch. <a href="#">Click here for more information.</a>
                            </p>
                            <p style="font-family:helvetica">
                            Thank you,<br />
                            Invesd
                            </p>
                            <p style="font-size:8pt;color:Gray">
                                This email was sent to you because you publish investment articles. Cick <a href="#" style="font-family:helvetica;color:Gray;text-decoration:underline">here</a> if you are not interested in receiving such emails in the future.
                            </p>
                        </td>
                    </tr>
                </table>
                
                <br />
                <br />
                
                <table>
                    <tr>
                        <td colspan="2"><strong>Invesd</strong></td>
                    </tr>
                    <tr>
                        <td colspan="2" style="background-color:#464646"><strong style="color:White">Don't just invest!</strong></td>
                    </tr>
                    <tr>
                        <td>Performance</td>
                        <td><asp:Label runat="server" ID="l_performance"></asp:Label></td>
                    </tr>
                    <tr>
                        <td>Alpha</td>
                        <td><asp:Label runat="server" ID="l_alpha"></asp:Label></td>
                    </tr>
                    <tr>
                        <td>Beta</td>
                        <td><asp:Label runat="server" ID="l_beta"></asp:Label></td>
                    </tr>
                    <tr>
                        <td>Overal Rank</td>
                        <td><asp:Label runat="server" ID="l_rank"></asp:Label></td>
                    </tr>
                    <tr>
                        <td>Actions</td>
                        <td><asp:Label runat="server" ID="l_actions"></asp:Label></td>
                    </tr>
                    <tr>
                        <td>Articles</td>
                        <td><asp:Label runat="server" ID="l_articles"></asp:Label></td>
                    </tr>
                    <tr>
                        <td>Actions/Articles</td>
                        <td><asp:Label runat="server" ID="l_ratio"></asp:Label></td>
                    </tr>
                    <tr>
                        <td>Best Sector</td>
                        <td><asp:Label runat="server" ID="l_sector"></asp:Label></td>
                    </tr>
                    <tr>
                        <td>Sector Rank</td>
                        <td><asp:Label runat="server" ID="l_sector_rank"></asp:Label></td>
                    </tr>
                    <tr>
                        <td>Sector Performance</td>
                        <td><asp:Label runat="server" ID="l_sector_performance"></asp:Label></td>
                    </tr>
                    <tr>
                        <td>Best Stock</td>
                        <td><asp:Label runat="server" ID="l_stock"></asp:Label></td>
                    </tr>
                    <tr>
                        <td>Stock Rank</td>
                        <td><asp:Label runat="server" ID="l_stock_rank"></asp:Label></td>
                    </tr>
                    <tr>
                        <td>Stock Performance</td>
                        <td><asp:Label runat="server" ID="l_stock_performance"></asp:Label></td>
                    </tr>

                </table>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
