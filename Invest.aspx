<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Invest.aspx.cs" Inherits="investor_Invest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Invesd</title>

    <!-- Autonumeric & accounting -->
    <script type="text/javascript" src="<%=Page.ResolveUrl("~/") %>js/autoNumeric.js"></script>
    <script src="<%= Page.ResolveUrl("~/")%>js/accounting.min.js" type="text/javascript"></script>

    <!-- Invest JS -->
    <script type="text/javascript" src="<%=Page.ResolveUrl("~/") %>js/invest_entry.js"></script>

    <!-- Standard Deviation -->
    <script type="text/javascript" src="<%=Page.ResolveUrl("~/") %>js/standard_deviation.js"></script>

    <script type="text/javascript">
        var portfolio_value = <%=portfolio_value%>;
        var profit = <%=profit%>;
        var loss = <%=loss%>;
        var ticker_id = <%=ticker_id%>;
        var txt = document.getElementById("btn").innerHTML;
        var current_value = <%=current_value%>;
        ///var txt_header = '<div id="header"><div class="alert alert-info" style="margin-bottom:10px"><table border="0" width="100%" cellpadding="0" cellspacing="0"><tr><td width="65%" style="text-align:left"><span id="hint">Pick the number of shares to buy</span></td><td width="35%" style="text-align:right;font-size:small">Current price: ' + accounting.formatMoney(current_value) + '</td></tr></table></div></div>';
        //var txt_header = $("#header").html();
        var target_undo = <%=target_undo%>;
        var consensus_target = <%=consensus_target%>;
        var cash = <%=cash%>;
        var diversification_old = <%=diversification%>;
        var actionid = <%=actionid%>;
        var pricechange = false;
        var invest_confirm = false;
        var last_hint = '';
        var selected_analyst_action = <%=selected_analyst_action%>;
        var selected_investor_action = <%=selected_investor_action%>;
        var diversification_new = 0; // 2/19/2014, amir
        //var target = 0;

        if (target_undo!=0){
            $("#txt_shares").focus();
        }

        $("#remaining_cash").text(accounting.formatMoney(cash));
        //$("#header").html('<div class="alert alert-danger" style="margin-bottom:10px"><table border="0" width="100%" cellpadding="0" cellspacing="0"><tr><td width="100%">Consensus is lower than the current price (' + accounting.formatMoney(current_value) + ')</td></tr></div>');
    </script>

    <script type="text/javascript">

        // diversification parameters
        var d1 = 0;
        var d3 = 0;
        var n = 0;
        var total = 0;
        var ws = new Array();
        var wn = new Array();
        var index_sector = 0;
        var index_position = 0;

        get_diversification();

        function imgError_user(image) {
            image.onerror = "";
            image.src = "<%= Page.ResolveUrl("~/")%>images/user/nouserIcon.png";
            return true;
        }

        function error(input){
            
            document.getElementById("btn").disabled = false;
            document.getElementById("btn").innerHTML = txt;
            if (input=="nocash"){
                $("#l_message").html("<span style=\"color:red\"><i class=\"icon-warning-sign\"></i> You don't have enough cash to invest</span>");
            }
            else if (input == "zero")
            {
                $("#l_message").html("<span style=\"color:red\"><i class=\"icon-warning-sign\"></i> Buy at least one share</span>");
            }
            else if(input == "value_change")
            {
                $("#hint").html("<span style=\"color:red\"><i class=\"icon-warning-sign\"></i> Price has changed by more than 0.5%. Try with udpated price.</span>");
                updatePrice();
            }
            else{
                $("#l_message").html("<span style=\"color:red\"><i class=\"icon-warning-sign\"></i> There is a technical problem. Try again later.</span>");
            }
        }

        function updatePrice(){
            var ticker = getParameterByName('ticker');
            $.getJSON('https://www.google.com/finance/info?infotype=infoquoteall&q='+ticker+'&callback=?', function (jd) {
                //alert();
                //alert("Got new price");
                if (hasOwnProperty(jd[0], 'el' )){
                    current_value = parseFloat((jd[0].el).replace(',',''));
                }
                else{
                    current_value = parseFloat((jd[0].l).replace(',',''));
                }
                $('#l_current').html(current_value);
                //alert(current_value);
                pricechange = true;
                changed_shares();
            });
        }

        function invest_now(){
            function onSucess(result) {
                //alert(result);
                switch (result) {
                    
                    case "success":
                        document.getElementById("btn").innerHTML = txt + " <i class=\"icon-ok\"></i>";
                        $("#header").html('<div class="alert alert-success" style="margin-bottom:10px"><table border="0" width="100%" cellpadding="0" cellspacing="0"><tr><td width="100%"><i class="icon-spinner icon-spin"></i> Your position was initiated. Loading your portfolio.</td></tr></div>');
                        setTimeout(function () { window.top.location.href = "investor/Portfolio.aspx"; }, 3000);
                        break;
                    case "error":
                        error("error");
                        break;
                    case "nocash":
                        error("nocash");
                        break;
                    case "zero":
                        error("zero");
                        break;
                    case "value_change":
                        error("value_change");
                        break;
                    default:
                        error();
                }
            }

            function onError(result) {
                error("error");
            }   

            if (invest_confirm){
                document.getElementById("btn").disabled = true;
                document.getElementById("btn").innerHTML = txt + " <i class=\"icon-spinner icon-spin\"></i>";
                PageMethods.invest($("#txt_shares").val().replace(",", ""), $("#txt_target").val().replace(",", "").replace("$",""), $("#months").val(), $("#txt_stoploss").val().replace(",", "").replace("$",""), "<%=Request.QueryString["ticker"] %>", $("#txt_rationale").val() , current_value,actionid,consensus_target,selected_investor_action, onSucess, onError);
            }
            else{
                $("#cell_cancel").attr('style','width:30%');
                $("#cell_space").attr('style','width:3%');
                $("#cell_invest").attr('style','width:67%');
                $("#btn_cancel").show();
                document.getElementById("btn").innerHTML = 'Confirm investment';
                last_hint = $("#hint").html();
                $("#hint").html('Are you sure you want to invest?');
                invest_confirm = true;
            }

            return false;
        }

        function dismiss(){
            $("#cell_cancel").attr('style','width:0%');
            $("#cell_space").attr('style','width:0%');
            $("#cell_invest").attr('style','width:100%');
            $("#btn_cancel").hide();
            invest_confirm = false;
            document.getElementById("btn").innerHTML = txt;
            $("#hint").html(last_hint);

            return false;
        }

        get_scatter(ticker_id);

        function get_scatter(ticker,top,update){
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Views.asmx/scatter_views",
                dataType: "json",
                data: "{ticker:" + ticker_id + ",history:false,selected:true,action:" + actionid + ",consensus:" + consensus_target + ",current:" + current_value + ",actionmonitor:0}",
                success: function (data) {
                    draw_scatter_invest(data.d);
                }
            });
        }

        function get_diversification(){

            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Portfolio.asmx/invest_diversification_new",
                dataType: "json",
                data: "{ticker:" + ticker_id + "}",
                success: function (data) {
                    if (data.d.length>0){
                        diversification_new = data.d[0];
                    }
                    // set before
                    //$("#div_before").attr("src",'<%=Page.ResolveUrl("~")%>images/signal' + (diversification_old>=0.8?'4':diversification_old>=0.6?'3':diversification_old>=0.4?'2':diversification_old>=0.2?'1':'0') + '.png');
                    //$("#div_after").attr("src",'<%=Page.ResolveUrl("~")%>images/signal' + (diversification_old>=0.8?'4':diversification_old>=0.6?'3':diversification_old>=0.4?'2':diversification_old>=0.2?'1':'0') + '.png');

                    $("#div_1").attr("style","width:" + (90 * diversification_old).toFixed(0)  + "%");
                    $("#div_2").attr("style","width:" + (100 - 90 * diversification_old).toFixed(0)  + "%");
                    $("#div_1_before").attr("style","width:" + (90 * diversification_old).toFixed(0)  + "%");
                    $("#div_2_before").attr("style","width:" + (100 - 90 * diversification_old).toFixed(0)  + "%");

                    //d1 = data.d.d1;
                    //d3 = data.d.d3;
                    //n = data.d.n;
                    //s = data.d.s;
                    //total = data.d.total;
                    //ws = data.d.sectors;
                    //wn = data.d.positions;
                    //index_position = data.d.index_position;
                    //index_sector = data.d.index_sector;
                }
            });
        }

        function update_diversification(shares){
            return shares>0?diversification_new:diversification_old;
        }

        //function update_diversification(shares){
        //    var new_investment = shares * current_value;
        //    var new_total = total + new_investment;
        //    var div=0;

        //    if (total>0){
        //        var new_ws = ws.slice(0);
        //        var new_wn = wn.slice(0);

        //        new_wn[index_position] += new_investment;
        //        new_ws[index_sector] += new_investment;

        //        for (var i=0;i<new_wn.length;i++){
        //            new_wn[i] /= new_total;
        //        }

        //        for (var i=0;i<new_ws.length;i++){
        //            new_ws[i] /= new_total;
        //        }

        //        var div = d1 * (1 - Math.sqrt(s) * getStandardDeviation(new_ws,10) ) + d3 * (1- Math.sqrt(n) * getStandardDeviation(new_wn,10));
        //    }
            
        //    if (typeof new_wn === "undefined")
        //    {
        //        return diversification_old;
        //    }
        //    else{
        //        if (shares>0)
        //            return new_wn.length>1?div:0;
        //        else
        //            return diversification_old;
        //    }
            
        //}

        function action_info(actionid,price){
            $("#analyst_img").attr("src","<%=Page.ResolveUrl("~")%>images/ajax_loader.gif");
            if (price>current_value)
                changed_price();

            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Views.asmx/analyst_view_by_action_id",
                dataType: "json",
                data: "{actionid:" + actionid + "}",
                success: function (data) {
                    $("#analyst_img").attr("src","<%=Page.ResolveUrl("~")%>images/user/" + data.d.analyst_id + ".png");
                    $("#analyst_img").attr("onerror", "imgError_user(this);");
                    $("#analyst_name").html(cutter(data.d.analyst_name,15));
                    $("#analyst_broker").html(cutter(data.d.analyst_broker,15));
                    set_consensus_variation(price);
                    $("#analyst_confidence").html('<img src="<%=Page.ResolveUrl("~")%>images/signal' + (data.d.confidence>=.8?'4':data.d.confidence>=.6?'3':data.d.confidence>=.4?'2':data.d.confidence>=.2?'1':data.d.confidence>=0?'0':'0') + '.png" alt="Confidence" title="Confidence" \>');
                }
            });
        }

        function set_consensus_variation(price){
            $("#average_variation").html(price!=consensus_target?(Math.abs((100*(price/consensus_target-1)).toFixed(1)) + "%"):'');
            $("#l_above_below").text(price>consensus_target?'% ABOVE':price<consensus_target?'% BELOW':'')
        }

        function set_to_consensus(){
            $("#tr_consensus_variation").hide();
            $("#tr_consensus_set").hide();
            $("#tr_selected_details").hide();
            $("#tr_info").show();

            $("#average_variation").html('');
            if (consensus_target>current_value){
                var chart = $('#scatter_plot').highcharts();
                var x = chart.series[1].data[0].x;
                chart.series[1].setData([{x:x,y:consensus_target}]);
                $("#txt_target").val(accounting.formatMoney(consensus_target));
                $("#l_selected").text(accounting.formatMoney(consensus_target));
                set_price_details(consensus_target);
                //$("#hint").html('');
                $("#div_alert").attr("class", "alert alert-info");
                actionid = 0;
                actionid_target = 0;
                selected_analyst_action = 0;
                selected_investor_action = 0;
            }
        }

        function draw_scatter_invest(data) {
            
            Highcharts.setOptions({ global: { useUTC: true } });
            var now = new Date();
            $('#scatter_plot').highcharts({
                rangeSelector:{selected:1,enabled:false},
                chart: {
                    type: 'scatter',
                    zoomType: 'xy',
                },
                title: {text: null},
                xAxis: { type: 'datetime' },
                credits: { enabled: false },
                legend:  { enabled: false },
                yAxis:{
                    title:
                        {
                            enabled: false
                        },
                },
                exporting: { enabled: false },
                scrollbar: {enabled:false},
                navigator: {enabled:false},
                plotOptions: {scatter: {marker: {radius: 5,symbol: 'circle',states: {hover: {enabled: true,}}},states: {hover: {marker: {enabled: false}}},tooltip: {headerFormat: '<b>{series.name}</b><br>',pointFormat: '${point.y} '},}},
                series: [
                    {
                        name: 'Target',
                        data: data.futuredata,
                        type: 'scatter',
                        point: {
                            events: {
                                click: function () {
                                    
                                    actionid = this.actionid;
                                    actionid_target = this.y;
                                    selected_analyst_action = actionid;
                                    selected_investor_action = 0;

                                    if (this.y>current_value){
                                        $("#tr_consensus_variation").show();
                                        $("#tr_consensus_set").show();
                                        $("#tr_selected_details").show();
                                        $("#tr_info").hide();
                                        
                                        $("#txt_target").val(accounting.formatMoney(this.y));
                                        $("#l_selected").text(accounting.formatMoney(this.y));
                                        action_info(this.actionid,this.y);
                                        var target_date = new Date(this.x);
                                        update_horizon(target_date);
                                        var chart = $('#scatter_plot').highcharts();
                                        chart.series[1].setData([{x:this.x,y:this.y}]);
                                    }
                                    else{
                                        $("#hint").text("Pick a target price higher than the current price");
                                        $("#div_alert").attr("class", "alert alert-danger");
                                    }

                                    
                                }
                            }
                        }   ,
                        marker: {
                            fillColor: '#0088cc',
                            radius: 10,
                        },
                        shadow: true,
                        tooltip: {
                            valueDecimals: 2
                        },
                        pointInterval: 24 * 3600 * 1000,
                        pointStart: Date.UTC(now.getYear() - 1, now.getMonth(), now.getDate()),
                       
                    },
                    {
                        name: 'Selected',
                        data: data.selected,
                        type: 'scatter',
                        marker: {
                            fillColor: '#000000',
                            symbol:'square',
                            radius: 5
                        },
                        shadow: false,
                        tooltip: {
                            valueDecimals: 2
                        },
                        pointInterval: 24 * 3600 * 1000,
                        pointStart: Date.UTC(now.getYear() - 1, now.getMonth(), now.getDate()),
                       
                    }
                ]
            });
        }

        function rank_builder(x){
            return '<i class="icon-star icon-2x" style="color:gold;position:absolute;top:0;left:0"></i>';
        }

        function cutter(x, n) {
            if (x.length>n)
                return "<span title=\"" + x + "\">" + x.substring(0, n) + "...</span>";
            else
                return x;
        }
    </script>
</head>
<body>
    <form id="form1" runat="server" style="margin:0;padding:0">
    <asp:ScriptManager runat="server" EnablePageMethods="true"></asp:ScriptManager>
        
        <div id="login" runat="server" class="text-center">
            <i class="icon-envelope icon-3x" style="color:silver"></i>
            <h4>Verify your email before investing</h4>
            <h5 style="color:gray">Please check your mailbox</h5>
        </div>

        <div id="loggedin" runat="server">

            <div id="header">
                <div class="alert alert-info" style="margin-bottom:10px" runat="server" id="div_alert">
                    <table border="0" width="100%" cellpadding="0" cellspacing="0">
                        <tr>
                            <td width="65%" style="text-align:left">
                                <span id="hint" runat="server"></span>
                            </td>
                            <td width="35%" style="text-align:right;font-size:small">
                                <strong>Current price: <span id="l_current" runat="server"></span></strong>
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
        
            <div class="thumbnail">
                <table border="0" width="100%">
                    <tr>
                        <td width="25%" class="text-center" style="border-right:1px solid #eeeeee">
                            <strong>Shares</strong>
                            <br />
                            <asp:TextBox runat="server" style="width:100px;margin-bottom:0;text-align:center;" ID="txt_shares" onkeyup="changed_shares()" autocomplete="off" onclick="click_shares()" onfocus="click_shares()"></asp:TextBox>
                            <br />
                            <asp:Label runat="server" ID="l_change"></asp:Label>
                            <br />
                            <span style="font-size:x-small;color:gray">INVESTMENT <i class="icon-info-sign" title="Estimate cash required to make the investment"></i></span>
                        </td>
                        <td width="25%" class="text-center" style="border-right:1px solid #eeeeee;position:relative">
                            <strong>Target price</strong>
                            <br />
                            <asp:TextBox runat="server" style="width:100px;margin-bottom:0;text-align:center;" ID="txt_target" onkeyup="changed_price()" autocomplete="off" onclick="click_target()" onfocus="click_target()" onblur="blur_price()" ClientIDMode="Static"></asp:TextBox>
                            <br />
                            <asp:Label runat="server" ID="l_target_percentage" style="font-size:small;color:#62c462"></asp:Label>
                            <br />
                            <asp:Label runat="server" ID="l_longshort" style="font-size:x-small;color:gray">UPSIDE <i class="icon-info-sign" title="Expected profit upside calculated based on your target price and the current price"></i></asp:Label>
                        </td>
                        <td width="25%" class="text-center" style="border-right:1px solid #eeeeee   ">
                            <strong>Timeframe</strong>
                            <br />
                            <select style="width:60px;margin:0;" id="months" onchange="months_changed()" onclick="click_timeframe()" onfocus="click_timeframe()">
                                <option>1</option>
                                <option>2</option>
                                <option>3</option>
                                <option>4</option>
                                <option>5</option>
                                <option>6</option>
                                <option>7</option>
                                <option>8</option>
                                <option>9</option>
                                <option>10</option>
                                <option>11</option>
                                <option selected="selected">12</option>
                                <option>13</option>
                                <option>14</option>
                                <option>15</option>
                                <option>16</option>
                                <option>17</option>
                                <option>18</option>
                                <option>19</option>
                                <option>20</option>
                                <option>21</option>
                                <option>22</option>
                                <option>23</option>
                                <option>24</option>
                            </select>
                            <br />
                            <span style="font-size:small"> months</span>
                            <br />
                            <span style="font-size:x-small;color:gray;text-transform:uppercase"><asp:Label runat="server" ID="l_term"></asp:Label> TERM</span> <i class="icon-info-sign" style="color:gray;font-size:x-small" title="Period of time you are expecting to hold this investment"></i>
                        </td>
                        <td width="25%" class="text-center">
                            <strong>Stop loss</strong>
                            <br />
                            <asp:TextBox runat="server" style="width:100px;margin-bottom:0;text-align:center;" ID="txt_stoploss" onkeyup="changed_lower()" autocomplete="off" onclick="click_stoploss()" onfocus="click_stoploss()" onblur="blur_lower()"></asp:TextBox>
                            <br />
                            <asp:Label runat="server" ID="l_stoploss_percentage" style="color:#ee5f5b;font-size:small">100%</asp:Label>
                            <br />
                            <span style="font-size:x-small;color:gray">DOWNSIDE <i class="icon-info-sign" title="Percentage of your investment that you are willing to lose in the worst case scenario"></i></span>
                        </td>
                    </tr>
                </table>

                <table border="0" width="100%">
                    <tr>
                        <td width="30%" style="vertical-align:top">
                            <div style="height:10px"></div>
                            <div style="height:130px" class="thumbnail">
                                <table border="0" cellpadding="0" cellspacing="0" width="100%">
                                    <tr style="height:20px;border-bottom:1px solid #eeeeee">
                                        <td width="100%" colspan="2">
                                            <table border="0" width="100%" cellpadding="0" cellspacing="0">
                                                <tr>
                                                    <td width="65%" style="font-size:x-small;color:gray;border-top:0;text-align:left">
                                                        SELECTED
                                                    </td>
                                                    <td width="35%" style="font-size:x-small;border-top:0;text-align:right">
                                                        <span id="l_selected" runat="server" style="font-size:10pt"></span>
                                                    </td>
                                                </tr>
                                                <tr style="display:none;height:50px" id="tr_selected_details" runat="server">
                                                    <td colspan="2">
                                                        <table border="0" width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:0;margin:auto">
                                                            <tr>
                                                                <td style="position:relative;border-top:0">
                                                                    <div id="analyst_confidence" style="position:absolute;top:0;left:0;display:none" runat="server"></div>
                                                                    <img src="/images/ajax_loader.gif" style="height:40px;width:40px" alt="" class="img-circle" id="analyst_img" runat="server" onerror="imgError_user(this)" />
                                                                </td>
                                                                <td style="border-top:0">
                                                                    <p style="margin-bottom:0;line-height:100%" class="text-left">
                                                                        <asp:Label runat="server" ID="analyst_name" style="font-size:x-small">&nbsp;</asp:Label>
                                                                        <br />
                                                                        <asp:Label runat="server" ID="analyst_broker" style="color:gray;font-size:x-small">&nbsp;</asp:Label>
                                                                    </p>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr style="height:20px;border-bottom:1px solid #eeeeee">
                                        <td style="font-size:x-small;color:gray;text-align:left">
                                            CONSENSUS <i class="icon-info-sign" title="Analyst target price consensus"></i>
                                        </td>
                                        <td style="text-align:right">
                                            <span style="font-size:10pt;" runat="server" id="consensus"></span>
                                        </td>
                                    </tr>
                                    <tr style="height:90px" id="tr_info" runat="server">
                                        <td colspan="2" style="vertical-align:middle;font-size:x-small;color:gray">
                                            Your position will be sold at the selected target price.
                                        </td>
                                    </tr>
                                    <tr style="height:20px;display:none" id="tr_consensus_variation" runat="server">
                                        <td style="font-size:x-small;color:gray;text-align:left">
                                            <span id="l_above_below" runat="server"></span>
                                        </td>
                                        <td style="text-align:right">
                                            <span style="font-size:10pt" id="average_variation" runat="server">&nbsp;</span>
                                        </td>
                                    </tr>
                                    <tr style="display:none" id="tr_consensus_set" runat="server">
                                        <td colspan="2" style="font-size:small">
                                            <a onclick="set_to_consensus();" class="urls" style="cursor:pointer">Reset to consensus</a>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <div style="height:10px"></div>

                            <%--<div class="thumbnail" style="width:80%;margin:auto;height:100px;display:none">
                                <table border="0" style="margin:auto;width:100%" id="div_average" runat="server">
                                    <tr>
                                        <td width="100%" style="position:relative">
                                            <div id="average_buysell" style="position:absolute;top:0;right:0" runat="server"></div>
                                            <div style="height:20px"></div>
                                            <div id="circle_conensus" runat="server" style="position:relative">
                                                <table border="0" width="100%" height="100%" style="margin:auto">
                                                    <tr style="height:100%">
                                                        <td width="100%" style="vertical-align:middle">
                                                            
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                                <table border="0" style="margin:auto;width:100%;display:none" id="div_analyst" runat="server">
                                    <tr>
                                        <td colspan="3" width="100%" style="position:relative">
                                            
                                            <div id="analyst_buysell" style="position:absolute;top:5px;right:5px" runat="server"></div>
                                            <div style="height:10px"></div>
                                            
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="text-align:center" colspan="3">
                                            <span style="font-size:x-small">
                                                
                                            </span>
                                        </td>
                                    </tr>
                                </table>
                            </div>--%>
                            
                        </td>
                        <td width="70%">
                            <div id="scatter_plot" style="height:150px;width:100%" class="text-center">
                                <img src="<%=Page.ResolveUrl("~") %>images/ajax_loader.gif" style="width:50px;padding:55px 0;" alt="" />
                            </div>
                        </td>
                    </tr>
                </table>
            </div>
            <div style="height:10px"></div>
            <table border="0" width="100%" style="margin:0" id="tbl_impact" cellpadding="0" cellspacing="5">
                <tr>
                    <td width="23%">
                        <div class="thumbnail" style="height:92px">
                            <table border="0" width="100%" cellpadding="0" cellspacing="0">
                                <tr style="height:10px">
                                    <td colspan="2">
                                        <strong>Cash</strong> <i style="color:gray;font-size:small" class="icon-info-sign" title="Portion of total cash to be invested in this stock"></i>
                                    </td>
                                </tr>
                                <tr style="height:30px">
                                    <td colspan="2">
                                        <div class="progress" style="margin-bottom:4px;margin-top:5px">
                                            <div class="bar" id="div_cash" style="width:0%"></div>
                                            <div id="div_cash_float" style="text-align:left">0%</div>
                                        </div>
                                    </td>
                                </tr>
                                <tr style="height:20px">
                                    <td colspan="2">
                                        <span id="remaining_cash" style="font-size:small"></span>
                                    </td>
                                </tr>
                                <tr style="height:10px">
                                    <td colspan="2">
                                        <span style="font-size:x-small;color:gray">REMAINING CASH</span>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </td>
                    <td width="2%"></td>
                    <td width="23%">
                        <div class="thumbnail" style="height:92px">
                            <table border="0" width="100%" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td colspan="2" width="100%">
                                        <strong>Diversification</strong>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2"> 
                                        <div style="height:7px"></div>
                                        <span style="font-size:x-small;color:gray">AFTER <i style="color:gray" class="icon-info-sign" title="Don't put all your eggs in one basket! Diversification increases with number of positions and has an inverse relationship with concentrating your investments in a position or a sector"></i></span>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2">
                                        <table border="0" cellpadding="0" cellspacing="0" width="100%">
                                            <tr>
                                                <td id="div_1" width="90%"></td>
                                                <td id="div_2" width="10%" style="">
                                                    <img src="<%=Page.ResolveUrl("~") %>images/caret_down.png" style="height:5px;display:block" alt="" />
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2">
                                        <table border="0" width="100%" cellpadding="0" cellspacing="0">
                                            <tr>
                                                <td width="5%"></td>
                                                <td width="90%">
                                                    <div class="gradient" style="height:2px;margin:0;width:100%"></div>
                                                </td>
                                                <td width="5%"></td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2">
                                        <table border="0" cellpadding="0" cellspacing="0" width="100%">
                                            <tr>
                                                <td id="div_1_before"></td>
                                                <td id="div_2_before" class="text-left">
                                                    <img src="<%=Page.ResolveUrl("~") %>images/caret_up.png" style="height:5px;opacity:0.3;display:block" alt="" />
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2">
                                        <span style="font-size:x-small;color:gray;">BEFORE</span>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </td>
                    <td width="2%"></td>
                    <td width="50%">
                        <strong>Why are you investing in this stock?</strong>
                        <textarea rows="2" style="margin:0;width:95%;resize:none" id="txt_rationale" placeholder="Optional"></textarea>
                                <%--<label class="checkbox inline" style="font-size:x-small;">
                                    <input type="checkbox" style="margin-bottom:0;margin-top:0" /> Fundamentals
                                </label>
                                <label class="checkbox inline" style="font-size:x-small;">
                                    <input type="checkbox" style="margin-bottom:0;margin-top:0" /> Technicals
                                </label>
                                <label class="checkbox inline" style="font-size:x-small;">
                                    <input type="checkbox" style="margin-bottom:0;margin-top:0" /> Sentiment
                                </label>--%>
                        <div style="height:2px"></div>
                        <table border="0" cellpadding="0" cellspacing="0" style="width:100%">
                            <tr>
                                <td style="width:0%" id="cell_cancel">
                                    <button runat="server" class="btn btn-danger btn-block" id="btn_cancel" onclick="return dismiss();" style="display:none">Cancel</button>
                                </td>
                                <td style="width:0%" id="cell_space"></td>
                                <td style="width:100%" id="cell_invest">
                                    <button runat="server" class="btn btn-success btn-block" id="btn" onclick="return invest_now();" disabled="disabled">Invest</button>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
            
        </div>
    </form>
</body>
</html>
