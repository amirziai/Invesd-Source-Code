<%@ Page Language="VB" AutoEventWireup="false" CodeFile="sa.aspx.vb" Inherits="admin_sa" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>NLP</title>
    <link href="<%= Page.ResolveUrl("~/")%>css/bootstrap.css" rel="stylesheet" media="screen" />
    <link href="<%= Page.ResolveUrl("~/")%>css/font-awesome.min.css" rel="stylesheet" media="screen" />
    <script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js"></script>
    <script type="text/javascript" src="<%= Page.ResolveUrl("~/")%>js/bootstrap.js"></script>
    <script type="text/javascript" src="<%= Page.ResolveUrl("~/")%>js/jquery.tokeninput.js"></script>
    <link rel="stylesheet" type="text/css" href="<%= Page.ResolveUrl("~/")%>css/token-input-investor.css" />
    <script type="text/javascript">
        var index = 0;

        $(document).ready(function () {
            skip(0);
        });

        function next() {
            index++;
            skip();
            return false;
        }

        function skip() {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>admin/Seeking_Alpha.asmx/seeking_alpha_type1",
                dataType: "json",
                data: "{index:" + index + "}",
                success: function (data) {
                    $("#iframe").attr("src", data.d.url);
                    $("#title").text(data.d.title);
                    $("#author").text(data.d.author);
                    $("#date").text(data.d.date);
                }
            });
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <table border="0" width="100%">
            <tr>
                <td width="70%">
                    <iframe id="iframe" style="width:100%;height:800px"></iframe>
                </td>
                <td width="30%">
                    <strong><span id="title"></span></strong>
                    <br />
                    <span id="author"></span>
                    <br />
                    <span id="date" style="color:gray"></span>
                    <hr />

                    <button class="btn btn-block btn-success">Add</button>
                    <br />
                    <button class="btn btn-block btn-info" onclick="return next();">Skip</button>
                    <br />
                    <button class="btn btn-block btn-danger">Remove</button>
                </td>
            </tr>
        </table>
    </form>
</body>
</html>
