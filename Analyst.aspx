<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Analyst.aspx.cs" Inherits="Analyst" MasterPageFile="~/MasterPage_Root.master" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
    <script type="text/javascript" src="<%=Page.ResolveUrl("~")%>js/querystring.js"></script>
    <script type="text/javascript" src="<%=Page.ResolveUrl("~")%>js/url_cleaner.js"></script>
    <!-- HighCharts -->
    <script src="<%= Page.ResolveUrl("~/")%>js/highstock.js" type="text/javascript"></script>

    <!-- Read more -->
    <script src="<%=Page.ResolveUrl("~") %>js/readmore.min.js" type="text/javascript"></script>

    <!-- JQuery Knob -->
    <script src="<%= Page.ResolveUrl("~/")%>js/jquery.knob.js" type="text/javascript"></script>

    <script type="text/javascript">
        followed_type = 'user';
        var analyst_id = <%=analyst_id%>;
        followed_id = analyst_id;
        var seriesOptions = [];
        var expanded_row = 1;
        var loaded_rows = [];
        var logged_in = <%=logged_in_page%>;
        var pre = "body_";
        var mouse_is_out = true;
        var invested_actions = '<%=invested_actions%>'.split("_");
        var colors_array = ["#0088cc",
                    "#00A3F5",
                    "#1FB4FF",
                    "#47C2FF",
                    "#70CFFF",
                    "#99DDFF",
                    "#C2EBFF",
                    "#EBF8FF", ];

        if (/Android|webOS|iPhone|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
            //window.location.replace("/analyst.aspx" + location.search);
        }

        $(document).ready(function () {
            count_follow_user();
            coverage();
            similar();
        });

        function getParameterByName(name) {
            name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
            var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
                results = regex.exec(location.search);
            return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
        }

        function confidence_func(confidence) {
            var on = '<i class="icon-star" style="color:#0088cc"></i>';
            var off = '<i class="icon-star-empty" style="color:rgba(0, 136, 204, 0.4)"></i>';

            if (confidence >= 0.8) {
                return on + on + on;
            }
            else if (confidence >= 0.5) {
                return on + on + off;
            }
            else if (confidence >= 0.2) {
                return on + off + off;
            }
            else {
                return off + off + off;
            }
        }

        function plot(div) {
            

            $(div).highcharts('StockChart', {
                chart: {
                    backgroundColor: 'transparent'
                },
                legend: {
                    enabled: true,
                },
                rangeSelector: {
                    selected: 4
                },

                yAxis: {
                    plotLines: [{
                        value: 0,
                        width: 2,
                        color: 'silver'
                    }]
                },

                credits: { enabled: false },
                exporting: { enabled: false },
                rangeSelector: { enabled: false },
                scrollbar: { enabled: false },
                navigator: { enabled: false },
                tooltip: {
                    pointFormat: '<span style="color:{series.color}">{series.name}</span>: <b>{point.y}</b><br/>',
                    valueDecimals: 2
                },
                series: seriesOptions
            });

            seriesOptions = [];
        }

        function count_follow_user() {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Users.asmx/count_follow_user",
                dataType: "json",
                data: "{user_id:" + analyst_id + "}",
                success: function (data) {
                    var content = (data.d == 0 ? 'NO' : data.d) + " FOLLOWER" + (data.d == 1 ? '' : 'S');
                    if (data.d > 0) {
                        content = '<a href="#follower_modal" class="urls" data-toggle="modal">' + content + '</a>';
                    }
                    $("#btn_follow_count").html(content);
                }
            });
        }

        function similar() {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Scaffolding.asmx/fetch_similar_analysts",
                dataType: "json",
                data: "{analyst_id:" + analyst_id + "}",
                success: function (data) {
                    if (data.d.length > 0) {
                        $("#txt_similar").show();
                        $("#div_similar").show();
                        $("#table_similar").show();

                        for (var i = 0; i < data.d.length; i++) {
                            var url = '<%=Page.ResolveUrl("~")%>analyst/' + data.d[i].analyst_id + '/' + url_cleaner(data.d[i].analyst_name) + '/similar';
                            $('#table_similar > tbody:last').append('<tr><td style="vertical-align:middle;width:20%;text-align:center;' + (i == 0 ? 'border-top:0' : '') + '"><a href="' + url + '"><img class="img-circle" src="<%=Page.ResolveUrl("~")%>images/user/' + data.d[i].analyst_id + '.png" style="width:40px;' + (i == 0 ? 'border-top:0' : '') + '" onerror="imgError_user(this)"></a></td><td style="vertical-align:middle;width:60%;' + (i == 0 ? 'border-top:0' : '') + '"><a href="' + url + '" class="urls">' + data.d[i].analyst_name + '</a><br><span style="color:gray;font-size:x-small">' + data.d[i].analyst_broker + '</span></td><td style="vertical-align:middle;width:20%;' + (i == 0 ? 'border-top:0' : '') + '"><a href="' + url + '" class="btn btn-info btn-block btn-small">View</a></td></tr>');
                        }
                    }
                }
            });
        }

        function coverage() {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Scaffolding.asmx/analyst_coverage",
                dataType: "json",
                data: "{analyst:" + analyst_id + "}",
                success: function (data) {
                    $("#loader").hide();
                    sectors(data.d.sector);
                    $("#confidence").html( confidence_func(data.d.confidence_overall) );

                    var sector_counter = 0;
                    var industry_counter = 0;
                    var stock_counter = 0;

                    for (var i = 0; i < data.d.target.length; i++) {
                        if (data.d.is_sector[i] || data.d.is_industry[i]) {
                            $('#Positions > tbody:last').append(header_builder(data.d.company[i], data.d.is_sector[i], data.d.is_industry[i], data.d.confidence[i], sector_counter));
                            if (data.d.is_sector[i]) {
                                sector_counter++;
                            }
                            else {
                                industry_counter++;
                            }
                        }
                        else {
                            stock_counter++;
                            $('#Positions > tbody:last').append(row_builder_master(data.d.company[i], data.d.ticker[i], data.d.ticker_id[i], data.d.target[i], data.d.upside[i], data.d.confidence[i], data.d.coverage_duration[i], data.d.action[i], data.d.rationale[i], stock_counter,data.d.progress_profit[i],data.d.progress_time[i]));
                            $(".progress_knob").knob();
                            
                            if (stock_counter == 1)
                            {
                                if (data.d.rationale[i].trim().length > 0) {
                                    $("#rationale" + stock_counter).readmore({ maxHeight: 40 });
                                }
                                count_follow_company(data.d.ticker_id[i], stock_counter);
                                user_following_company(data.d.ticker_id[i], stock_counter);
                                count_number_of_investors_in_company(data.d.ticker_id[i], stock_counter);
                                ajax(stock_counter, data.d.ticker[i]);
                            }
                        }
                    }

                    $("#count_stocks").text(stock_counter);
                    $("#count_industries").text(industry_counter);
                    $("#count_sectors").text(sector_counter);
                    enable_popover();
                    enable_popover_stock();
                    $("[rel='tooltip']").tooltip();
                    $("#body_tr_sector").show();
                    $("#body_tr_stats").attr("style", "border-bottom:1px solid #eeeeee");

                    var actionAgree = getParameterByName("agree_action");
                    var tickerAgree = getParameterByName("agree_ticker");
                    if(actionAgree != "" && tickerAgree != ""){
                        AgreeWith(actionAgree, tickerAgree);
                    }

                }
            });
        }

        function enable_popover_stock() {
            $(".show-tooltip-stock").hover(function () {
                mouse_is_out = false;
                var el = $(this);
                var ticker = el.attr("id").split("_")[1];

                $(".show-tooltip-stock").popover('destroy');

                $.ajax({
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    url: "<%=Page.ResolveUrl("~")%>Scaffolding.asmx/company_description",
                    dataType: "json",
                    data: "{ticker:'" + ticker + "'}",
                    success: function (data) {
                        $(".show-tooltip-stock").popover('destroy');
                        if (!mouse_is_out) {
                            var show = '<span style="font-size:small;color:black">' + data.d + '</span>';
                            el.attr('data-content', show);
                            el.popover('show');
                        }
                    }
                });
            },
            function () {
                $(".show-tooltip-stock").popover('destroy');
                mouse_is_out = true;
            });
        }

        function follow_company(ticker_id,id) {

            document.getElementById("btn_follow" + id).disabled = true;
            document.getElementById("btn_follow" + id).innerHTML = '<i class="icon-spinner icon-spin"></i>';

            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Users.asmx/follow_company_ajax",
                dataType: "json",
                data: "{company:" + ticker_id + "}",
                success: function (data) {
                    if (data.d)
                        set_btn_company(true,id);
                    else
                        set_btn_company(false,id);

                    count_follow_company(ticker_id,id);
                }
            });

            return false;
        }

        function count_follow_company(ticker_id,id) {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Users.asmx/count_follow_company",
                dataType: "json",
                data: "{company:" + ticker_id + "}",
                success: function (data) {
                    var content = (data.d == 0 ? 'NO' : data.d) + " FOLLOWER" + (data.d == 1 ? '' : 'S');
                    //if (data.d > 0) {
                    //    content = '<a href="#follower_modal" class="urls" data-toggle="modal">' + content + '</a>';
                    //}
                    $("#btn_follow_count" + id).html(content);
                }
            });
        }

        function user_following_company(company, id) {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Users.asmx/user_following_company",
                dataType: "json",
                data: "{company:" + company + "}",
                success: function (data) {
                    if (data.d) {
                        $("#btn_follow" + id).text("Following");
                        $("#btn_follow" + id).attr("class", "btn btn-success");
                    }
                    else {
                        $("#btn_follow" + id).text("Follow");
                        $("#btn_follow" + id).attr("class", "btn btn-info");
                    }
                    $("#btn_follow" + id).show();
                    $("#btn_follow_count" + id).show();
                }
            });
        }

        function count_number_of_investors_in_company(ticker_id, id) {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Users.asmx/count_number_of_investors_in_company",
                dataType: "json",
                data: "{company:" + ticker_id + "}",
                success: function (data) {
                    var content = (data.d[0] == 0 ? 'NO' : data.d[0]) + ' INVESTOR' + (data.d[0] == 1 ? '' : 'S');
                    //if (data.d[0] > 0) {
                    //    content = '<a href="#investor_modal" class="urls" data-toggle="modal">' + content + '</a>';
                    //}
                    $("#btn_follow_investors" + id).html(content);

                }
            });
        }

        function ajax(id, ticker) {

            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Views.asmx/analyst_revision_history_for_stock_analyst",
                dataType: "json",
                data: "{ticker:'" + ticker + "',analyst:" + analyst_id + "}",
                success: function (data) {
                    loaded_rows.push(id);

                    for (var i = 0; i < data.d.list.length; i++) {
                        seriesOptions[i] = {
                            data: data.d.list[i].data,
                            name: data.d.list[i].name,
                            color: data.d.list[i].color,
                            type: (i != (data.d.list.length - 1) ? 'spline' : null)
                        }
                    }

                    $('#consensus' + id).text('$' + accounting.formatNumber(data.d.consensus,0) );
                    $('#upside' + id).html(color_code((100 * data.d.upside).toFixed(1), false));
                    var target_price = parseFloat($('#above' + id).text());
                    $('#above' + id).html(color_code((100 * (target_price / data.d.consensus - 1)).toFixed(1), false));
                    $('#above' + id).show();
                    $('#above_text' + id).text( (target_price > data.d.consensus ? 'ABOVE':'BELOW')  );
                    plot('#plot' + id);
                    //$('#place' + i).text(data.d.consensus);
                }
            });
        }

        var last_element;
        function expand_row(id, el, ticker,ticker_id) {
            if (last_element) {
                last_element.children().first().attr('class', 'icon-plus');
                last_element.attr('class', 'btn btn-small');
            }
            else {
                $("#expand1").children().first().attr('class', 'icon-plus');
                $("#expand1").attr('class', 'btn btn-small');
            }

            $(el).children().first().attr('class', 'icon-minus');
            $(el).attr('class', 'btn btn-small btn-success');
            last_element = $(el);

            if (!expanded_row == 0) {
                $("#row" + expanded_row).hide();
            }

            if (expanded_row != id) {
                expanded_row = id;
                $("#row" + id).show();
                if (loaded_rows.indexOf(id) == -1) {
                    $("#rationale" + id).readmore({ maxHeight: 40 });

                    count_follow_company(ticker_id, id);
                    user_following_company(ticker_id, id);
                    count_number_of_investors_in_company(ticker_id, id);
                    ajax(id, ticker);
                }
            }
            else {
                expanded_row = 0;
                $(el).children().first().attr('class', 'icon-plus');
                $(el).attr('class', 'btn btn-small');
            }
        }
        function AgreeWith(action, ticker_id) {
            //alert("Agreeing with " + action);
            $("#agree_" + action).html("<i class=\"icon-spinner icon-spin\"></i>");
            if(logged_in==1){            

                $.ajax({
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    url: "<%= Page.ResolveUrl("~/")%>Estimate.asmx/GetUserEstimateTicker",
                    dataType: "json",
                    data: "{ticker:" + ticker_id + "}",
                    success: function (data) {
                        user_rank = (data.d != null) ? data.d.ranks : -2;
                        if (user_rank == -2) {

                        }
                        else if (user_rank == -1) {
                            $.ajax({
                                type: "POST",
                                contentType: "application/json; charset=utf-8",
                                url: "<%= Page.ResolveUrl("~/")%>Estimate.asmx/AgreeEstimate",
                                dataType: "json",
                                data: "{actionID:" + action + "}",
                                success: function (data) {
                                    if (data.d == "success") {
                                        $("#agree_" + action).html("Got it");
                                        $("#agree_" + action).attr("disabled", "disabled");
                                        $("#noagree_" + action).remove();
                                        //$("#noagree_" + action).attr("disabled", "disabled");
                                        //$("#noagree_" + action).removeAttr("href");
                                    }
                                }
                            });
                        }
                        else {
                            $("#agree_" + action).html("Estimate Exists");
                        }
                    }
                
             
                });
            }
            else {
                window.location.href = "https://www.invesd.com/signup.aspx?ReturnUrl=Analyst.aspx"+location.search.replace(/\&/g,"%26")+"%26agree_action="+action+"%26agree_ticker="+ticker_id;
            }
        
        }

        function row_builder_master(name, ticker, ticker_id, target, upside, confidence, coverage_duration, action, rationale, i,progress_profit,progress_time)
        {
            return '<tr><td colspan="2" style="margin:0;padding:0"><table style="width:100%">' + row_builder(name, ticker, ticker_id, target, upside, confidence, coverage_duration, action, i,progress_profit,progress_time) + row_builder_expanded(i, rationale, ticker, ticker_id, i, target) + '</table></td></tr>';
        }
        
        function row_builder_expanded(i, rationale, ticker, ticker_id,i, target) {
            return '<tr' + (i == 1 ? '' : ' style="display:none"') + ' id="row' + i + '"><td style="width:400px;background-color:transparent;border-top:0;" colspan="5"><table style="width:100%;"><tr><td style="width:25%;margin:0;padding:0;border:0;background-color:transparent;text-align:center;vertical-align:middle;height:60px"><span id="consensus' + i + '"></span><br><span style="color:gray;font-size:x-small">CONSENSUS</span></td><td rowspan="3" style="width:75%;margin:0;padding:0;border:0;background-color:transparent;text-align:center"><div style="width:100%;height:180px;text-align:center" id="plot' + i + '"><img src="<%=Page.ResolveUrl("~")%>images/ajax_loader.gif" style="width:50px;height:50px;margin-top:75px;"></div></td></tr><tr><td style="margin:0;padding:0;border:0;background-color:transparent;text-align:center;vertical-align:middle;height:60px"><span id="upside' + i + '"></span><span style="color:gray;font-size:x-small"><br>UPSIDE</span></td></tr><tr><td style="margin:0;padding:0;border:0;background-color:transparent;text-align:center;vertical-align:middle;height:60px"><span id="above' + i + '" style="display:none">' + target + '</span><br><span style="color:gray;font-size:x-small" id="above_text' + i + '"></span></td></tr><tr><td colspan="2" style="margin:0;padding:0;border:0;background-color:transparent;text-align:right"><table style="border:0;width:100%"><tr><td style="width:60%;margin:0;padding:0;border:0;background-color:transparent;text-align:left;vertical-algin:top"><div style="color:gray;font-size:9pt" id="rationale' + i + '">' + rationale + '</div></td><td style="width:20%;margin:0;padding:0;border:0;background-color:transparent;text-align:center"><a class="btn btn-info" id="btn_follow' + i + '"' + (userid > 0 ? (' onclick="return follow_company(' + ticker_id + ',' + i + ');"') : ' href="<%=Page.ResolveUrl("~")%>Signup.aspx?ReturnUrl=<%=Request.Url%>"') + ' style="display:none">Follow</a><br /><span style="color:gray;font-size:x-small;display:none;margin-bottom:0;padding-bottom:0" id="btn_follow_count' + i + '"><i class="icon-spinner icon-spin"></i> FOLLOWERS</span><br><a href="#"><span style="font-size:x-small"></span></a></td><td style="width:20%;margin:0;padding:0;border:0;background-color:transparent;text-align:center"><a href="company/' + ticker + '/analyst_expanded" class="btn btn-success">More info</a><br><span style="color:gray;font-size:x-small;margin-bottom:0;padding-bottom:0" id="btn_follow_investors' + i + '"><i class="icon-spinner icon-spin"></i> INVESTORS</span></a></td></tr></table></td></tr></table></td></tr>';
        }

        function row_builder_agree(i, rationale, ticker, ticker_id, i, target, action) {
            return '<tr' + (i > 0 ? '' : ' style="display:none"') + ' id="row_agree' + i + '"><td style="width:100%;background-color:transparent;border-top:0;" colspan="5"><table style="width:100%;"><tr><td style="width=35%;border-top:0"></td><td style="width:40%;margin:0;padding:0;border:0;background-color:transparent;text-align:right;vertical-align:middle">Do you agree with this estimate?</td><td style="width:10%;margin:0;padding:0;border:0;background-color:transparent;text-align:center;vertical-align:middle"><a href="company/' + ticker + '&ref=analyst_dontagree" class="btn btn-danger" id="noagree_' + action + '">No</a></td><td style="width:15%;margin:0;padding:0;border:0;background-color:transparent;text-align:left;vertical-align:middle"><div type="button" onclick="AgreeWith(' + action + ',' + ticker_id + ')" class="btn btn-success" id="agree_' + action + '">Yes</div></td></tr></table></td></tr>';
        }

        function row_builder(name, ticker, ticker_id,target, upside, confidence,coverage_duration,action,id,progress_price,progress_time) {
            //if (confidence>.8){
            //    target = -100;
            //}

            return '<tr> \
                        <td style="width:50%;vertical-align:middle;background-color:transparent;border-top:0"> \
                            <table border="0" width="100%"> \
                                <tr> \
                                    <td width="30%" style="margin:0;padding:0;border:0;background-color:transparent;text-align:right"> \
                                        <a href="<%=Page.ResolveUrl("~")%>company/' + ticker.toLowerCase() + '/' + url_cleaner(name) + '/analyst"><img id="img_' + ticker + '" src="<%=Page.ResolveUrl("~")%>images/logo/' + ticker + '.png" style="height:40px;width:40px;" class="img-polaroid img-rounded show-tooltip-stock" alt="" onerror="imgError(this)" title="" data-trigger="manual" data-html="true" data-placement="right" /></a> \
                                    </td> \
                                    <td width="5%" style="margin:0;padding:0;border:0;background-color:transparent;text-align:right"> \
                                    <td width="65%" style="margin:0;padding:0;border:0;vertical-align:middle;background-color:transparent"> \
                                        <a href="<%=Page.ResolveUrl("~")%>company/' + ticker.toLowerCase() + '/' + url_cleaner(name) + '/analyst" class="urls"><strong>' + cutter(name, 15) + '</strong></a> \
                                        <br><span style="color:gray;font-size:x-small;text-transform:uppercase">' + duration_builder(coverage_duration) + '</span>    \
                                    </td> \
                                </tr> \
                            </table> \
                        </td> \
                        <td style="width:15%;text-align:center;vertical-align:middle;background-color:transparent;border-top:0"><span data-toggle="tooltip" rel="tooltip" title="$' + accounting.formatNumber(target, target>10?0:(target>5?1:2)) + ' price target">' + (target==-100?'':(upside == -1000 ? '' : (color_code((100 * upside).toFixed(1), false) + '<br><span style="font-size:x-small;color:gray">' + (upside>=0?'UPSIDE':'DOWNSIDE') + '</span>'))) + '</span></td>\
                        <td style="width:10%;text-align:center;vertical-align:middle;background-color:transparent;border-top:0">' + signal(confidence, ticker_id, true) + '<br><span style="font-size:x-small;color:gray">RECORD</span></td>\
                        <td style="width:20%;text-align:center;vertical-align:middle;background-color:transparent;border-top:0"> \
                            <div style="width: 60px; height: 60px;margin-right:auto;margin-left:auto;vertical-align:middle;position:relative"> \
                                <div style="position:absolute;left:0px;top:0px"> \
                                    <input data-fgColor="' + (progress_price > 0 ? '#62c462' : progress_price < 0 ? '#ee5f5b' : 'gray') + '" data-reverse="' + (progress_price >= 0 ? 'false' : 'true') + '" class="progress_knob" data-skin="tron" data-width="60" data-height="60" data-thickness=".2 " data-min="0" data-max="100" data-readOnly="true" value="' + Math.abs((100 * progress_price).toFixed(0)) + '%" data-displayInput="true" /> \
                                </div> \
                                <div style="position:absolute;left:12px;top:12px"> \
                                    <input class="progress_knob" data-min="0" value="' + (100 * progress_time).toFixed(0) + '" data-fgColor="#5B5A5A" data-skin="tron" data-max="100" data-width="36" data-height="36" data-thickness=".25" data-readOnly="true" data-displayInput="false"/> \
                                </div> \
                            </div> \
                        </td>\
                        <td style="width:5%;text-align:center;vertical-align:middle;background-color:transparent;border-top:0"><a class="btn btn-small' + (id == 1 ? ' btn-success' : '') + '" data-toggle="tooltip" rel="tooltip" title="Expand/collapse" id="expand' + id + '" onclick="expand_row(' + id + ',this,\'' + ticker + '\',' + ticker_id + ')"><i class="icon-' + (id==1?'minus':'plus') + '"></i></a></td> \
                    </tr>';
        }

        function sectors(data) {
            new Highcharts.Chart({
                chart: {
                    renderTo: 'div_sectors',
                    height: 180,
                    width: 250,
                    spacingBottom: 0,
                    spacingTop: 0,
                    spacingLeft: 0,
                    spacingRight: 0,
                    margin: 0
                },
                credits: {
                    enabled: false
                },
                exporting: {
                    enabled: false
                },
                tooltip: {
                    formatter: function () {
                        return '<b>' + this.point.name + ':</b> ' + this.y;
                    }
                },
                colors: colors_array,
                legend: {
                    enabled: true,
                    borderWidth: 0,
                    padding: 0,
                    itemMarginBottom: 5,
                    layout: 'vertical',
                    align: 'right',
                    verticalAlign: 'middle',
                    floating: true
                },
                title: {
                    text: null
                },
                plotOptions: {
                    pie: {
                        allowPointSelect: false,
                        cursor: 'pointer',
                        center: [50, 75],
                        size: '85%',
                        dataLabels: {
                            enabled: false
                        },
                        showInLegend: true,
                        point: {
                            events: {
                                legendItemClick: function () {
                                    return false;
                                }
                            }
                        }
                    }
                },
                series: [{
                    type: 'pie',
                    data: data
                }]
            });
        }

        function enable_popover() {
            $(".show-tooltip").hover(function () {

                mouse_is_out = false;
                var el = $(this);
                var ticker = parseInt(el.attr("id").split("_")[1]);
                var analyst = analyst_id;

                $(".show-tooltip").popover('destroy');

                $.ajax({
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    url: "<%=Page.ResolveUrl("~")%>Views.asmx/analyst_confidence_details",
                    dataType: "json",
                    data: "{ticker:" + ticker + ",analyst:" + analyst + "}",
                    success: function (data) {
                        $(".show-tooltip").popover('destroy');
                        if (!mouse_is_out) {
                            var show = "";

                            if (data.d.views == 0) {
                                show = '<p class="text-center"><span style="color:gray"><i class="icon-info-sign icon-2x"></i><br>No track record</span></p>';
                            }
                            else {
                                show = '<table class="table table-condensed" style="width:220px;border:0;margin:0;padding:0"> \
                                            <tr> \
                                                <td style="width:65%;text-align:left;border:0;border-bottom:1px solid #eeeeee"> \
                                                    <span style="font-size:small">Average return</span> \
                                                </td> \
                                                <td style="width:15%;text-align:right;border:0;border-bottom:1px solid #eeeeee"> \
                                                    <span style="font-size:small">' + percentage_builder((100 * data.d.ret).toFixed(1)) + '</span>\
                                                </td> \
                                                <td style="width:20%;text-align:right;border:0;border-bottom:1px solid #eeeeee"> \
                                                    <img src="<%=Page.ResolveUrl("~")%>images/signal' + (data.d.ret >= .1 ? '4' : data.d.ret >= .08 ? '3' : data.d.ret >= .05 ? '2' : data.d.ret >= .02 ? '1' : '0') + '.png"> \
                                                </td> \
                                            </tr> \
                                            <tr> \
                                                <td style="text-align:left;border:0;border-bottom:1px solid #eeeeee"> \
                                                    <span style="font-size:small">Average accuracy</span> \
                                                </td> \
                                                <td style="text-align:right;border:0;border-bottom:1px solid #eeeeee"> \
                                                    <span style="font-size:small">' + accuracy_color_coder((100 * data.d.accuracy).toFixed(1)) + '</span>\
                                                </td> \
                                                <td style="border:0;text-align:right;border-bottom:1px solid #eeeeee"> \
                                                    <img src="<%=Page.ResolveUrl("~")%>images/signal' + (data.d.accuracy >= .8 ? '4' : data.d.accuracy >= .7 ? '3' : data.d.accuracy >= .5 ? '2' : data.d.accuracy >= .3 ? '1' : '0') + '.png"> \
                                                </td> \
                                            </tr> \
                                            <tr> \
                                                <td style="text-align:left;border:0"> \
                                                    <span style="font-size:small">Views tracked</span> \
                                                </td> \
                                                <td style="text-align:right;border:0"> \
                                                    <span style="font-size:small">' + data.d.views + '</span>\
                                                </td> \
                                                <td style="border:0;text-align:right;"> \
                                                    <img src="<%=Page.ResolveUrl("~")%>images/signal' + (data.d.views >= 10 ? '4' : data.d.views >= 8 ? '3' : data.d.views >= 6 ? '2' : data.d.views >= 4 ? '1' : '0') + '.png"> \
                                                </td> \
                                            </tr> \
                                        </table> \
                                        <p class="text-center" style="margin-top:5px;margin-bottom:0;font-size:x-small;color:gray;text-transform:uppercase"><i class="icon-info-sign"></i> Based on past performance</p>';
                            }


                            el.attr('data-content', show);
                            el.popover('show');
                        }
                    }
                });
            },
            function () {
                $(".show-tooltip").popover('destroy');
                mouse_is_out = true;
            });
        }

        function percentage_builder(x) {
            if (x > 0) {
                return "<span style=\"color:#62c462\">" + x + "%</span>";
            }
            else if (x < 0) {
                return "<span style=\"color:#ee5f5b\">-" + Math.abs(x) + "%</span>";
            }
            else {
                return "0%";
            }
        }

        function accuracy_color_coder(x) {
            if (x >= 80) {
                return "<span style=\"color:#62c462\">" + x + "%</span>";
            }
            else if (x < 80 && x > 0)
                return x + "%";
            else
                return "<span style=\"color:#ee5f5b\">-" + -x + "%</span>";
        }

        function header_builder(name, sector, industry, confidence,color_index) {
            return '<tr> \
                        <th style="width:85%;text-align:left;vertical-align:middle;color:' + ((sector && color_index < 4) ? 'white' : 'black') + ';background-color:' + (sector ? colors_array[color_index] : "#e1f4e1") + '">' + (industry ? '&nbsp;&nbsp;&nbsp;&nbsp;' : '') + name + '</th>\
                        <th style="width:15%;text-align:right;vertical-align:middle;color:' + ((sector && color_index < 4) ? 'white' : 'black') + ';background-color:' + (sector ? colors_array[color_index] : "#e1f4e1") + '"><span style="font-size:x-small;font-weight:normal">' + (industry ? 'INDUSTRY' : 'SECTOR') + '</span></th>\
                    </tr>';
        }

        function color_code(x, money) {
            if (x > 0) {
                return '<span style="color:#62c462">' + (money ? "$" + accounting.formatNumber(x) : x) + (money ? "" : "%") + "</span>";
            }
            else if (x < 0) {
                return '<span style="color:#ee5f5b">' + (money ? "($" + accounting.formatNumber(Math.abs(x)) : x) + (money ? ")" : "%") + "</span>";
            }
            else {
                return money ? "$0" : "0.0%";
            }
        }

        function cutter(x, n) {
            if (x.length > n)
                return "<span rel=\"tooltip\" data-toggle=\"tooltip\" title=\"" + x + "\">" + x.substring(0, n) + "...</span>";
            else
                return x;
        }

        function signal(x,id,tooltip) {
            return '<img src="<%=Page.ResolveUrl("~")%>images/signal' + (x >= .8 ? '4' : x >= .7 ? '3' : x >= .5 ? '2' : x >= .3 ? '1' : '0') + '.png" alt="Confidence" id="ticker_' + id + '"' + (tooltip ? ' class="show-tooltip" data-trigger="manual" data-html="true" data-placement="top" style="cursor:pointer"' : '') + '>';
        }

        function follow() {
            function onSucess(result) {
                count_follow_user();

                switch (result) {
                    case "followed":
                        set_btn(true);
                        break;
                    default:
                        set_btn(false);
                }
            }

            function onError(result) {
                set_btn(false);
            }

            document.getElementById(pre + "btn").disabled = true;
            document.getElementById(pre + "btn").innerHTML = '<i class="icon-spinner icon-spin"></i>';
            PageMethods.follow(analyst_id, onSucess, onError);
            return false;
        }

        function set_btn(followed) {
            document.getElementById(pre + "btn").disabled = false;

            if (followed) {
                document.getElementById(pre + "btn").innerHTML = "Following";
                $("#" + pre + "btn").attr("class", "btn btn-success");
            }
            else {
                document.getElementById(pre + "btn").innerHTML = "Follow";
                $("#" + pre + "btn").attr("class", "btn btn-info");
            }
        }

        function set_btn_company(followed,id) {
            document.getElementById("btn_follow" + id).disabled = false;

            if (followed) {
                document.getElementById("btn_follow" + id).innerHTML = "Following";
                $("#btn_follow" + id).attr("class", "btn btn-success");
            }
            else {
                document.getElementById("btn_follow" + id).innerHTML = "Follow";
                $("#btn_follow" + id).attr("class", "btn btn-info");
            }
        }

        function duration_builder(x) {
            if (x < 30)
                return 'Recent coverage';
            else if (x>=30 && x<365)
                return 'Covering for ' + (x / 30).toFixed(0) + ' months';
            else if (x >= 365 && x < 2 * 365)
                return 'Covering for over a year';
            else
                return 'Covering for ' + (x / 365).toFixed(0) + ' years';
        }
    </script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="body">
    <%--<asp:ScriptManager runat="server" EnablePageMethods="true"></asp:ScriptManager>--%>
    <div class="row">
        <div class="span12">
            <div class="alert alert-info text-center" style="display:none;font-size:1.2em" id="div_quick_tips">
                <table style="border:0;width:100%">
                    <tr>
                        <td style="width:75%;text-align:left">
                            <span id="title" style="font-weight:bold">Free daily alerts</span>
                            <br />
                            <span id="text">Recommendations and revisions from 4,500+ analysts in your mailbox.</span>
                        </td>
                        <td style="width:25%">
                            <asp:TextBox runat="server" placeholder="E-mail address" class="form-control" id="inEmail" autocomplete="off" style="margin-bottom:5px" />
                            <asp:Button ClientIDMode="Static" ID="go" runat="server" CssClass="btn btn-block btn-success btn-lg" PostBackUrl="~/Signup.aspx?ref=analyst" Text="Subscribe for free"></asp:Button>
                        </td>
                    </tr>
                </table>
            </div>

            <table border="0" width="100%">
                <tr>
                    <td width="35%" style="vertical-align:top">
                        <h4>Analyst coverage</h4>
                        <div class="thumbnail text-center" style="background-color:white">
                            <table border="0" width="100%" id="logged_in" runat="server">
                                <tr style="border-bottom:1px solid #eeeeee">
                                    <td colspan="4">
                                        <table border="0" width="100%">
                                            <tr>
                                                <td width="25%">
                                                    <asp:Image runat="server" ID="image" style="width:50px;height:50px" CssClass="img-circle" />
                                                </td>
                                                <td width="45%">
                                                    <asp:Label runat="server" ID="l_analyst"></asp:Label>
                                                    <br />
                                                    <asp:Label runat="server" ID="l_broker" style="color:gray;font-size:9pt"></asp:Label>
                                                </td>
                                                <td width="30%">
                                                    <a runat="server" id="btn" onclick="return follow();" style="cursor:pointer">Follow</a>
                                                    <br />
                                                    <span style="color:gray;font-size:x-small" id="btn_follow_count"><i class="icon-spinner icon-spin"></i> FOLLOWERS</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="4">
                                        <div style="height:10px"></div>
                                    </td>
                                </tr>
                                <tr id="tr_stats">
                                    <td width="31%" style="border-right:1px solid #eeeeee">
                                        <span style="font-size:14pt;font-family:Sans-Serif;font-weight:normal">
                                            <span id="confidence"><img src="<%=Page.ResolveUrl("~") %>images/ajax_loader.gif" style="width:20px;height:20px" /></span>
                                        </span>
                                        <br />
                                        <small style="color:gray;font-size:x-small">CONFIDENCE <i class="icon-info-sign" data-toggle="tooltip" rel="tooltip" title="Average confidence for all covered stocks. A measure of analyst's overall track record." data-placement="bottom"></i></small>
                                    </td>
                                    <td width="23%" style="border-right:1px solid #eeeeee">
                                        <span style="font-size:14pt;font-family:Sans-Serif;font-weight:normal">
                                            <span id="count_stocks"><img src="<%=Page.ResolveUrl("~") %>images/ajax_loader.gif" style="width:20px;height:20px" /></span>
                                        </span>
                                        <br />
                                        <small style="color:gray;font-size:x-small">STOCKS</small>
                                    </td>
                                    <td width="23%" style="border-right:1px solid #eeeeee">
                                        <span style="font-size:14pt;font-family:Sans-Serif;font-weight:normal">
                                            <span id="count_industries"><img src="<%=Page.ResolveUrl("~") %>images/ajax_loader.gif" style="width:20px;height:20px" /></span>
                                        </span>
                                        <br />
                                        <small style="color:gray;font-size:x-small">INDUSTRIES</small>
                                    </td>
                                    <td width="23%">
                                        <span style="font-size:14pt;font-family:Sans-Serif;font-weight:normal">
                                            <span id="count_sectors"><img src="<%=Page.ResolveUrl("~") %>images/ajax_loader.gif" style="width:20px;height:20px" /></span>
                                        </span>
                                        <br />
                                        <small style="color:gray;font-size:x-small">SECTORS</small>
                                    </td>
                                </tr>
                                <tr id="tr_sector" style="display:none">
                                    <td colspan="4">
                                        <div style="width:250px;height:180px;margin-right:auto;margin-left:auto" id="div_sectors"></div>
                                        <span style="font-size:x-small;color:gray">SECTOR BREAKDOWN <i class="icon-info-sign" data-toggle="tooltip" rel="tooltip" title="Number of stocks covered in each sector" data-placement="bottom"></i></span>
                                    </td>
                                </tr>
                            </table>
                        </div>

                        <br />
                        <h4 style="display:none" id="txt_similar">Similar analysts</h4>
                        <div class="thumbnail text-center" style="display:none;background-color:white" id="div_similar">
                            <table class="table table-condensed" style="margin:0" id="table_similar">
                                <tbody></tbody>
                            </table>
                        </div>
                    </td>
                    <td width="5%"></td>
                    <td width="60%" style="vertical-align:top">
                        <h4>Covered companies</h4>
                        <div class="thumbnail" style="background-color:white">
                            <table border="0" class="table table-hover" width="100%" style="margin:0" id="Positions">
                                <tbody></tbody>
                            </table>
                            <!-- AJAX Loader -->
                            <table border="0" id="loader" width="100%" cellpadding="5" cellspacing="5">
                                <tr>
                                    <td width="100%">
                                        <div class="text-center">
                                            <img src="<%=Page.ResolveUrl("~") %>images/ajax_loader.gif" alt="" style="width:50px" />
                                        </div>                        
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </td>
                </tr>
            </table>
        </div>
    </div>

</asp:Content>