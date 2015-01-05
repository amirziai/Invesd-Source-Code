<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Challenge.aspx.cs" Inherits="Competition" MasterPageFile="~/MasterPage_Root.master" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
    <script type="text/javascript" src="<%=Page.ResolveUrl("~")%>js/url_cleaner.js"></script>

    <script type="text/javascript">
        var userid = <%=userid%>;

        if( /Android|webOS|iPhone|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) ) {
            //window.location.replace("Challenge_mob.aspx"+location.search);
        }

        $(document).ready(function () {
            $("[rel='tooltip']").tooltip();
            //invitation_details_list();
            leaderboard_season("current");
            $('#agree').change(function () {
                check_terms();
            });
        });

        function invitation_details_list() {

            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%=Page.ResolveUrl("~")%>Users.asmx/invitation_cash_earned",
                dataType: "json",
                data: "",
                success: function (data) {
                    $("#cash_earned").text("$" + accounting.formatNumber(data.d) );
                }
            });

            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%=Page.ResolveUrl("~")%>Users.asmx/invitation_details_list",
                dataType: "json",
                data: "",
                success: function (data) {
                    if (data.d.length == 0) {
                        $("#count_invited").text(0);
                        $("#count_joined").text(0);
                    }
                    else {
                        $("#count_invited").text(data.d.length);
                        var count_joined = 0;

                        for (var i = 0; i < data.d.length; i++) {
                            if (data.d[i].joined>0)
                                count_joined++;
                        }

                        $("#count_joined").text(count_joined);
                    }
                }
            });
        }

        function check_terms(){
            if ($("#agree").is(':checked')){
                $("#btn").removeAttr('disabled');
            }
            else{
                $("#btn").attr('disabled','disabled');
            }
        }

        function imgError_user(image) {
            image.onerror = "";
            image.src = "<%= Page.ResolveUrl("~/")%>images/user/nouserIcon.png";
            return true;
        }

        function compare(a, b) {
            if (a.profit_return > b.profit_return)
                return -1;
            if (a.profit_return < b.profit_return)
                return 1;
            return 0;
        }

        function get_season(){
            sort = $("#season_selector").val();

            if (sort=="alltime"){
                $('#TableLeaderboard > tbody:last').html('');
                $("#loader").show();
                leaderboard();
            }
            else{
                leaderboard_season(sort);
            }
        }

        function leaderboard_season(season){
            $('#TableLeaderboard > tbody:last').html('');
            $("#loader").show();

            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Portfolio.asmx/leaderboard_season",
                dataType: "json",
                data: "{season:'" + season + "'}",
                success: function (data) {
                    $("#loader").hide();
                    data.d.sort(compare);

                    for (var i = 0; i < data.d.length; i++) {
                        $('#TableLeaderboard > tbody:last').append(row_builder(i+1, data.d[i].userid, data.d[i].name, data.d[i].deployed, data.d[i].profit_return,data.d[i].membership_duration));
                        if (userid == data.d[i].userid){
                            $("#l_rank").text(i+1);
                        }
                    }

                    $("[rel='tooltip']").tooltip();
                    $('#TableLeaderboard tr').click(function () {
                        var href = $(this).find("a").attr("href");
                        if (href) {
                            window.location = href;
                        }
                    });
                }
            });
        }

        function leaderboard() {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Portfolio.asmx/leaderboard",
                dataType: "json",
                data: "",
                success: function (data) {
                    $("#loader").hide();
                    data.d.sort(compare);

                    for (var i = 0; i < data.d.length; i++) {
                        $('#TableLeaderboard > tbody:last').append(row_builder(i+1, data.d[i].userid, data.d[i].name, data.d[i].deployed, data.d[i].profit_return,data.d[i].membership_duration));
                        if (userid == data.d[i].userid){
                            $("#l_rank").text(i+1);
                        }
                    }

                    $("[rel='tooltip']").tooltip();
                    $('#TableLeaderboard tr').click(function () {
                        var href = $(this).find("a").attr("href");
                        if (href) {
                            window.location = href;
                        }
                    });
                }
            });
        }

        function row_builder(rank,user_id,name,deployed,ret,membership_duration) {
            return '<tr style="cursor:pointer;' + ((user_id==userid)?'background-color:rgba(0,136,204,0.1)':'') + '"> \
                        <td width="10%" style="text-align:center;vertical-align:middle;font-size:11pt;">' + rank + '</td> \
                        <td width="50%" style="vertical-align:middle;"> \
                            <table border="0" width="100%"> \
                                <tr> \
                                    <td width="20%" style="margin:0;padding:0;border:0;background-color:transparent;position:relative"> \
                                        <a href="<%=Page.ResolveUrl("~")%>investor/' + user_id + '/' + url_cleaner(name) + '"> \
                                            <img src="<%=Page.ResolveUrl("~")%>images/user/' + user_id + '.png" style="height:40px;width:40px;" class="img-circle" alt="" onerror="imgError_user(this)" /> \
                                        </a>'
                                       + ((user_id==1 || user_id==2)?'<i title="Linked" rel="tooltip" data-toggle="tooltip" class="icon-ok-sign" style="color:#62c462;position:absolute;top:0;right:2px"></i>':'') +
                                    '</td> \
                                    <td width="80%" style="margin:0;padding:0;border:0;vertical-align:middle;background-color:transparent"> \
                                        <a href="<%=Page.ResolveUrl("~")%>investor/' + user_id + '/' + url_cleaner(name) + '" class="urls"><strong>' + name + '</strong></a><br><span style="color:gray;font-size:x-small">Joined ' + duration_builder(membership_duration) + '</span> \
                                    </td> \
                                </tr> \
                            </table> \
                        </td> \
                        <td width="20%" style="vertical-align:middle;">\
                            <div class="progress" style="height:10px;margin:5px" rel="tooltip" data-toggle="tooltip" title="' + (100*deployed).toFixed(0)  + '%"> \
                                <div class="bar bar-success" style="width:' + (100*deployed).toFixed(0) + '%"></div> \
                            </div> \
                        </td>\
                        <td width="20%" style="text-align:center;vertical-align:middle;">' 
                            + profit_builder(ret) +
                        '</td>\
                    </tr>';
        }

        function duration_builder(x){
            if (x<=1)
                return 'today';
            else if (x>1 && x<30)
                return x.toFixed(0) + ' days ago';
            else if (x>=30 && x<365)
                return (x/30).toFixed(0) + ' months ago';
            else if (x>=365 && x<2*365)
                return 'over a year ago';
            else
                return (x/365).toFixed(0) + ' years ago';
        }

        function profit_builder(x){
            if (x>.5){
                return '<span style="color:#62c462">$' + accounting.formatNumber(x) + '</span>';
            }
            else if (x<-0.5){
                return '<span style="color:#ee5f5b">-$' + accounting.formatNumber(-x) + '</span>';
            }
            else{
                return "$0";
            }
        }

        function target_builder(x) {
            
            if (x > 0) {
                return "<span style=\"color:#62c462\">" + accounting.formatNumber(x) + "%</span>";
            }
            else if (x < 0) {
                return "<span style=\"color:#ee5f5b\">" + accounting.formatNumber(Math.abs(x)) + "%</span>";
            }
            else {
                return "0%";
            }
        }

        function enter() {

            function onSucess(result) {
                switch (result) {
                    case "success":
                        document.getElementById("btn").innerHTML = txt + ' <i class="icon-ok"></i>';
                        location.reload(true);
                        break;
                    default:
                        document.getElementById("signup_btn").disabled = false;
                        document.getElementById("btn").innerHTML = txt;
                }
            }

            function onError(result) {
                document.getElementById("signup_btn").disabled = false;
                document.getElementById("btn").innerHTML = txt;
            }

            var txt = document.getElementById("btn").innerHTML;
            document.getElementById("btn").disabled = true;
            document.getElementById("btn").innerHTML = txt + ' <i class="icon-spinner icon-spin"></i>';
            PageMethods.enter(onSucess, onError);
            return false;
        }

        $("[rel='tooltip']").tooltip();
    </script>
</asp:Content>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="body">
    <asp:ScriptManager runat="server" EnablePageMethods="true"></asp:ScriptManager>
    <div class="row">
        <div class="span12">
            <table border="0" width="100%">
                <tr>
                    <td width="35%" style="vertical-align:top">
                        <h4>Your stats</h4>
                        <div class="thumbnail text-center" style="background-color:white">
                            <table border="0" width="100%" id="logged_in" runat="server">
                                <tr style="border-bottom:1px solid #eeeeee">
                                    <td colspan="2">
                                        <table border="0" width="100%">
                                            <tr>
                                                <td width="25%">
                                                    <asp:Image runat="server" ID="image" style="width:50px;height:50px" CssClass="img-circle" />
                                                </td>
                                                <td width="50%">
                                                    <asp:Label runat="server" ID="l_investor"></asp:Label>
                                                </td>
                                                <td width="25%">
                                                    <span style="font-size:18pt;font-family:Sans-Serif;font-weight:normal" id="l_rank">?</span>
                                                    <br />
                                                    <span style="font-size:x-small;color:gray">RANK</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2">
                                        <div style="height:10px"></div>
                                    </td>
                                </tr>
                                <tr style="border-bottom:1px solid #eeeeee" runat="server" id="competing1">
                                    <td width="50%" style="border-right:1px solid #eeeeee">
                                        <span style="font-size:14pt;font-family:Sans-Serif;font-weight:normal">
                                            <asp:Label runat="server" ID="profit"></asp:Label>
                                        </span>
                                        <br />
                                        <span style="color:gray;font-size:x-small">ALL-TIME PROFIT</span>
                                    </td>
                                    <td width="50%">
                                        <span style="font-size:14pt;font-family:Sans-Serif;font-weight:normal">
                                            <asp:Label runat="server" ID="deployed"></asp:Label>
                                        </span>
                                        <br />
                                        <span style="color:gray;font-size:x-small">DEPLOYED <i class="icon-info-sign" data-toggle="tooltip" rel="tooltip" title="Portion of assets invested in stocks" data-placement="bottom"></i></span>
                                    </td>
                                </tr>
                                <tr runat="server" id="competing2">
                                    <td width="50%" style="border-right:1px solid #eeeeee">
                                        <span style="font-size:14pt;font-family:Sans-Serif;font-weight:normal">
                                            <asp:Label runat="server" ID="stocks"></asp:Label>
                                        </span>
                                        <br />
                                        <span style="color:gray;font-size:x-small">STOCKS <i class="icon-info-sign" data-toggle="tooltip" rel="tooltip" title="Market value of stocks" data-placement="bottom"></i></span>
                                    </td>
                                    <td width="50%">
                                        <span style="font-size:14pt;font-family:Sans-Serif;font-weight:normal">
                                            <asp:Label runat="server" ID="funds"></asp:Label>
                                        </span>
                                        <br />
                                        <span style="color:gray;font-size:x-small">CASH</span>
                                    </td>
                                </tr>
                            </table>
                            <table border="0" width="100%" id="teaser" runat="server">
                                <tr>
                                    <td width="100%">
                                        <h4>$100k investment challenge</h4>
                                        <br />
                                        <ul style="text-align:left">
                                            <li>Practice & gain confidence with virtual cash</li>
                                            <li>Leverage insights from 4,500+ analysts</li>
                                            <li>Build your track record</li>
                                        </ul>
                                    </td>
                                </tr>
                                <tr id="optin" runat="server">
                                    <td>
                                        <br />
                                        <label class="checkbox" style="color:gray">
                                            <input type="checkbox" id="agree" /> I have read and agree to the challenge <abbr rel="tooltip" data-toggle="tooltip" title="Your name, picture and performance will be shown on the leaderboard">terms</abbr>
                                        </label>
                                        <button class="btn btn-block btn-success" disabled="disabled" id="btn" onclick="return enter();">Take the challenge</button>
                                    </td>
                                </tr>
                                <tr runat="server" id="loginorsignup">
                                    <td>
                                        <a class="btn btn-success btn-block" href="<%=Page.ResolveUrl("~") %>Signup.aspx?ReturnUrl=<%=Request.Url %>">Signup</a>
                                        <a class="btn btn-info btn-block" href="<%=Page.ResolveUrl("~") %>Login.aspx?ReturnUrl=<%=Request.Url %>">Login</a>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </td>
                    <td width="5%"></td>
                    <td width="60%" style="vertical-align:top">

                        <table border="0" width="100%">
                            <tr>
                                <td width="50%">
                                    <h4>Leaderboard</h4>
                                </td>
                                <td width="50%" class="text-right">
                                    <select style="margin:0;width:220px" onchange="get_season();" id="season_selector">
                                        <option value="current">Current season (Q3 2014)</option>
                                        <option value="Q2 2014">Previous season (Q2 2014)</option>
                                        <option value="Q1 2014">Q1 2014</option>
                                        <option value="Q4 2013">Q4 2013</option>
                                        <option value="alltime">All-Time</option>
                                    </select>
                                </td>
                            </tr>
                        </table>
                        
                        <div class="thumbnail" style="background-color:white">
                            <table border="0" class="table table-hover" width="100%" style="margin:0" id="TableLeaderboard">
                                <thead>
                                    <tr>
                                        <td style="text-align:center;width:10%"><strong>Rank</strong></td>
                                        <td style="text-align:center;width:50%"><strong>Investor</strong></td>
                                        <td style="text-align:center;width:20%"><strong>Deployed</strong> <i class="icon-info-sign" rel="tooltip" title="Portion of assets invested in stocks"></i></td>
                                        <td style="text-align:center;width:20%"><strong>Profit</strong> <i class="icon-info-sign" rel="tooltip" title="Cumulative profit"></i></td>
                                    </tr>
                                </thead>
                                <tbody></tbody>
                            </table>
                        </div>
                        <table border="0" id="loader" width="100%" cellpadding="5" cellspacing="5">
                            <tr>
                                <td width="100%">
                                    <div class="text-center">
                                        <img src="<%=Page.ResolveUrl("~") %>images/ajax_loader.gif" alt="" style="width:50px" />
                                    </div>                        
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </div>
    </div>
</asp:Content>