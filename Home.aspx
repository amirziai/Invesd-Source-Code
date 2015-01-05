<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Home.aspx.cs" Inherits="Home" MasterPageFile="~/MasterPage_Root.master" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
    <!-- JQuery Knob -->
    <script src="<%=Page.ResolveUrl("~") %>js/jquery.knob.js" type="text/javascript"></script>

    <!-- HighCharts -->
    <script src="<%= Page.ResolveUrl("~/")%>js/highstock.js" type="text/javascript"></script>
    <script src="<%= Page.ResolveUrl("~/")%>js/modules/exporting.js" type="text/javascript"></script>
    <script type="text/javascript" src="<%= Page.ResolveUrl("~/")%>highslide/highslide-full.js"></script>
    <script src="<%= Page.ResolveUrl("~/")%>js/bootstrap-slider.js" type="text/javascript"></script>
    <link rel="Stylesheet" media="screen" href="<%= Page.ResolveUrl("~/")%>css/slider.css" />
    <link href="<%=Page.ResolveUrl("~/") %>css/highslide.css" rel="stylesheet" />
    
    <!-- Sectors pie chart -->
    <script src="<%=Page.ResolveUrl("~/") %>js/sectors.js" type="text/javascript"></script>

    <!-- Readmore -->
    <script src="<%=Page.ResolveUrl("~") %>js/readmore.min.js" type="text/javascript"></script>

    <script type="text/javascript">
        var index = 0;
        var index_watchlist = 0;
        var index_followed = 0;
        var index_estimates = 0;
        var number_of_views = 10;
        var user = <%=user%>;
        var progress_value = 0;
        var investor_type = false;
        var count_universal = 0; // counts feed items

        //if( /Android|webOS|iPhone|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) ) {
        //    window.location.replace("Home_mob.aspx"+location.search);
        //}

        $(document).ready(function () {
            load_feed();
            control_top_panel();
            user_stats();
            quick_tip();
        });

        function control_top_panel(){
            if (userid>0){
                noteworthy();
                watchlist();
                estimates();
            }
            else{
                $("#watchlist_link").hide();
                $("#watchlist").hide();
                $("#estimates_link").hide();
                $("#estimates").hide();
                $("#noteworthy_link").attr('class','active');
                $("#noteworthy").attr('class','tab-pane active');
                noteworthy();
            }
        }

        function quick_tip(){
            if (user==0){
                $("#div_quick_tips2").show();
            }
            else{
                $.ajax({
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    url: "<%= Page.ResolveUrl("~/")%>Users.asmx/quick_tips",
                dataType: "json",
                data: "",
                success: function (data) {
                    if (data.d){
                        $("#div_quick_tips").show();
                        $("#l_quick_tip").html(generate_tip_html(data.d));
                    }
                }
                });
            }
        }

        function generate_tip_html(stage){
            switch (stage) {
                case "noob":  
                    return 'Create your first <strong>estimate</strong> and start building your track record';
                    break;
                case "more":
                    return 'High ranked investors have <strong>20+</strong> estimates. Create more estimates.';
                    break;
                case "challenge":
                    return 'Showcase your portfolio management skills. Take the <strong>$100k</strong> investment challenge.';
                    break;
                default:
                    return "";
            }
        }

        function sector_icon(sector) {
            switch (sector) {
                case 1:
                    return "medkit";
                    break;
                case 2:
                    return "shopping-cart";
                    break;
                case 3:
                    return "beaker";
                    break;
                case 4:
                    return "wrench";
                    break;
                case 5:
                    return "money";
                    break;
                case 6:
                    return "cogs";
                    break;
                case 7:
                    return "laptop";
                    break;
                case 8:
                    return "lightbulb";
                    break;
            }
        }

        <%--function close_carousel(){
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Portfolio.asmx/close_home_carousel",
                dataType: "json",
                data: "{index:" + index + "}",
                success: function (data) {
                    if (data.d){
                        $("#carousel").hide();
                        $("#carousel_space").hide();
                    }
                }
            });
        }--%>

        function note(ind) {
            index = index + ind;
            noteworthy();
        }

        function navigate_watchlist(ind){
            index_watchlist += ind;
            watchlist();
        }

        function navigate_followed(ind){
            index_followed += ind;
            investors_analysts_following();
        }

        function navigate_estimates(ind){
            index_estimates += ind;
            estimates();
        }

        
        function load_feed(){
          //  $("#TableNotification").html("");
            $("#loader").show();
            $("#loadmore").hide();
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Scaffolding.asmx/homepage_feed",
                dataType: "json",
                data: "{index:"+index+"}",
                success: function (data) {
                    $("#loader").hide();
                    $("#loadmore").show();
                    for (var i = 0; i < data.d.Feeds.length; i++) {
                        var x = feed_builder_v2(i,data.d.Feeds[i].fundID,data.d.Feeds[i].userID,data.d.Feeds[i].span,data.d.Feeds[i].data,data.d.Feeds[i].type,user);
                        $('#TableNotification').append(x);
                        $("#rationale" + count_universal).readmore({ maxHeight: 40 });
                        count_universal++;
                    }
                    loading = false;
                }
            });
        }

        
        function feed_builder_v2(i,company,user,span,data,type,Cuser) {
            var selfUser;
            if (user == Cuser)
                selfUser = true;
            else
                selfUser = false;

            //var text = feed_builder_text(type, data, selfUser, false);
            var item = feed_builder_content(i,type, data, selfUser, user,count_universal); 
            var row = '<div class="thumbnail" style="background-color:white;margin-bottom:5px"><table style="width:100%;margin-top:10px;margin-bottom:10px"><tr>'+item+'</span></td>';
            row += '<td class="text-right" width="15%" style="vertical-align:bottom;text-align:right;font-size:x-small;color:gray;' + (i > 0 ? '' : 'border-top:0') + '">' + horizon_builder(span) + '</td></tr></table></div>';
            return row;
        }
        var loading = true;
        var index = 0;
        var end_reached = false;
        $(window).scroll(function () {
            if ($(window).scrollTop() + $(window).height() > $(document).height()-120) {
                //alert("bottom!");
                if (!end_reached && !loading) {
                    index++;
                    loading = true;
                    //ajax_views(index, ticker_id,top_global);
                    load_feed();
                }
            }
        });

        function load_more() {
            if (!end_reached) {
                index++;
                //ajax_views(index, ticker_id,top_global);
                load_feed();
            }
            else {
                $("#loadmore").hide();
            }

            return false;
        }

        function user_stats(){
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Users.asmx/user_stats_estimates",
                dataType: "json",
                data: "",
                success: function (data) {
                    $("#estimates_count").text(data.d.estimates);
                    $("#sectors").text(data.d.sectors);
                    $("#score").text( accounting.formatNumber(data.d.score * 100) );
                    $("#avg").html( color_coder(data.d.avg * 100,true,1) );
                    if (data.d.sector.length>0){
                        $("#show_sector").show();
                        sectors_pie_chart(data.d.sector);
                    }
                }
            });
        }

        function estimates() {
            for (var i = 0; i < 4; i++) {
                $("#estimate" + (i+1)).html('<img src="images/ajax_loader.gif" style="width:30px">');
            }

            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Users.asmx/estimates",
                dataType: "json",
                data: "{index:" + index_estimates + "}",
                success: function (data) {

                    if (data.d.count == 0){
                        $("#EstimatesDiv").html('<p class="text-center""><i class="icon-info-sign"></i> You have not estimated on any stocks</p>');

                        $("#noteworthy_link").attr('class','active');
                        $("#noteworthy").attr('class','tab-pane active');
                        $("#estimates_link").attr('class','');
                        $("#estimates").attr('class','tab-pane');
                    }
                    else{

                        var counter = 0;
                        var y = "";
                        var total_counter = data.d.ticker.length;

                        for (var i = 0; i < 4; i++) {
                            counter++;
                            $("#estimate" + counter).html("");
                            if (total_counter>=counter){
                                y = row_builder_estimates(data.d.ticker[i], data.d.name[i], data.d.sector[i],data.d.broker[i],data.d.sector_id[i]);
                                $("#estimate" + counter).append(y);
                            }
                        }

                        $("[rel='tooltip']").tooltip();
                        enable_popover();
                    }
                }
            });
        }

        function return_relative_bars(data) {
            var pos = new Array();
            var neg = new Array();

            for (var i = 0; i < data.length; i++) {
                if (data[i] > 0) {
                    pos.push(data[i]);
                }
                else {
                    neg.push(data[i]);
                }
            }

            var y = 0;
            if (pos.length == 2) {
                y = 100;
            }
            else if (neg.length<2) {
                y = (100*Math.max.apply(null, pos) / (Math.max.apply(null, pos) - Math.min.apply(null, neg))).toFixed(0);
            }

            $("#bigpicture_parent_ticker_neg").attr("style", "width:" + (100-y) + "%");
            $("#bigpicture_parent_ticker_pos").attr("style", "width:" + y + "%");
            $("#bigpicture_parent_industry_neg").attr("style", "width:" + (100 - y) + "%");
            $("#bigpicture_parent_industry_pos").attr("style", "width:" + y + "%");

            $("#bigpicture_child_ticker_neg").attr("style", "float:right;width:" + (data[0] > 0 ? 0 : (100 * data[0] / Math.min.apply(null, neg)).toFixed(0)) + "%");
            $("#bigpicture_child_ticker_pos").attr("style", "width:" + (data[0] > 0 ? (100 * data[0] / Math.max.apply(null, pos)).toFixed(0) : 0) + "%");
            $("#bigpicture_child_industry_neg").attr("style", "float:right;width:" + (data[1] > 0 ? 0 : (100 * data[1] / Math.min.apply(null, neg)).toFixed(0)) + "%");
            $("#bigpicture_child_industry_pos").attr("style", "width:" + (data[1] > 0 ? (100 * data[1] / Math.max.apply(null, pos)).toFixed(0) : 0) + "%");
        }

        function color_coder(value,percentage,float){
            var out = accounting.formatNumber(Math.abs(value),float );
            out = percentage?(out + '%'):('$' + out);
            out = value>=0?out:('-' + out);

            if (value>0){
                return '<span style="color:#62c462">' + out + '</span>';
            }
            else if (value<0){
                return '<span style="color:#ee5f5b">' + out + '</span>';
            }
            else
            {
                return out;
            }
        }

        function rank_builder(rank){
            var suffix = "";
            if (rank==1){
                suffix = "ST";
            }
            else if (rank==2)
            {
                suffix = "ND";
            }
            else if (rank==3){
                suffix = "RD";
            }
            else{
                suffix = "TH";
            }

            return rank + "<sup style=\"font-size:0.6em\">" + suffix + "</sup>";
        }

        function noteworthy() {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Scaffolding.asmx/noteworthy_estimate",
                dataType: "json",
                data: "{index:" + index + "}",
                success: function (data) {
                    var counter = 0;
                    var y = "";

                    for (var i = 0; i < 4; i++) {
                        counter++;

                        y = row_builder(data.d.ticker[i], data.d.name[i], data.d.sector[i],data.d.target[i],data.d.sector_id[i]);
                        $("#noteworthy" + counter).html("");
                        $("#noteworthy" + counter).append(y);
                    }

                    $("[rel='tooltip']").tooltip();
                    enable_popover();
                }
            });
        }

        function watchlist() {
            for (var i = 0; i < 4; i++) {
                $("#watchlist" + (i+1)).html('<img src="images/ajax_loader.gif" style="width:30px">');
            }

            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Users.asmx/watchlist",
                dataType: "json",
                data: "{index:" + index_watchlist + "}",
                success: function (data) {
                    if (data.d.count == 0){
                        $("#WatchlistDiv").html('<p class="text-center"><i class="icon-info-sign"></i> No companies in your watchlist</p>');
                    }
                    else{
                        var counter = 0;
                        var y = "";
                        var total_counter = data.d.ticker.length;

                        for (var i = 0; i < 4; i++) {
                            counter++;
                            $("#watchlist" + counter).html("");
                            if ( total_counter>=counter ){
                                y = row_builder_watchlist(data.d.ticker[i], data.d.name[i], data.d.sector[i],data.d.broker[i],data.d.sector_id[i]);
                                $("#watchlist" + counter).append(y);
                            }
                        }

                        $("[rel='tooltip']").tooltip();
                        enable_popover();
                    }
                }
            });
        }

        function investors_analysts_following() {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Users.asmx/investors_analysts_following",
                dataType: "json",
                data: "{index:" + index_followed + "}",
                success: function (data) {
                    if (data.d.count == 0){
                        
                    }
                    else{
                        var counter = 0;
                        var y = "";

                        for (var i = 0; i < 4; i++) {
                            counter++;
                            y = row_builder_user(data.d.name[i], data.d.ticker[i],data.d.sector[i],0.1,1);
                            $("#followed" + counter).html("");
                            $("#followed" + counter).append(y);
                        }

                        $("[rel='tooltip']").tooltip();
                        //enable_popover();
                    }
                }
            });
        }

        function cutter(x, n) {
            if (x.length > n)
                return "<span rel=\"tooltip\" data-toggle=\"tooltip\" title=\"" + x + "\">" + x.substring(0, n) + "...</span>";
            else
                return x;
        }

        function enable_popover() {
            $(".show-tooltip").hover(function () {
                mouse_is_out = false;
                var el = $(this);
                var ticker = el.attr("id").split("_")[1];
                
                $(".show-tooltip").popover('destroy');

                $.ajax({
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    url: "<%=Page.ResolveUrl("~")%>Scaffolding.asmx/company_description",
                    dataType: "json",
                    data: "{ticker:'" + ticker + "'}",
                    success: function (data) {
                        $(".show-tooltip").popover('destroy');
                        if (!mouse_is_out) {
                            var show = '<span style="font-size:small;color:black">' + data.d + '</span>';
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

        function row_builder_watchlist(ticker,company,sector,upside,sector_id) {

            return '<table border="0"> \
                        <tr class="noteworthy_tr"> \
                            <td width="30%" class="text-center"> \
                                <a href="company/' + ticker + '"> \
                                    <img src="<%=Page.ResolveUrl("~")%>images/logo/' + ticker + '.png" class="img-polaroid img-rounded show-tooltip" style="width:50px;height:50px" onerror="imgError(this);" title="" data-trigger="manual" data-html="true" data-placement="bottom" id="imgticker_' + ticker + '" /> \
                                </a> \
                            </td> \
                            <td width="70%" style="padding-left:10px" class="text-left"> \
                                <p class="ticker_info"> \
                                    <a href="company/' + ticker + '/watchlist" class="urls"><strong>' + cutter(company,11) + '</strong></a> \
                                </p> \
                                <p class="ticker_info"> \
                                    <a href="companies?sectors=' + sector + '&ref=watchlist" class="urls"> \
                                        <span style="color:gray;font-size:x-small" data-toggle="tooltip" rel="tooltip" title="View more companies in this sector" data-placement="bottom"><i class="icon-' + sector_icon(sector_id) + '"></i> ' + sector + '</span> \
                                    </a> \
                                </p> \
                                <p class="ticker_info"> \
                                    <a href="company/' + ticker + '/watchlist" style="font-size:x-small;margin:0;padding:3px;line-height:1;" class="btn btn-small btn-info"><span style="font-size:x-small">Estimate</span></a> \
                                </p> \
                            </td> \
                        </tr> \
                    </table>';
        }

        function row_builder_estimates(ticker,company,sector,upside,sector_id) {

            return '<table border="0"> \
                        <tr class="noteworthy_tr"> \
                            <td width="30%" class="text-center"> \
                                <a href="company/' + ticker + '/estimates"> \
                                    <img src="<%=Page.ResolveUrl("~")%>images/logo/' + ticker + '.png" class="img-polaroid img-rounded show-tooltip" style="width:50px;height:50px" onerror="imgError(this);" title="" data-trigger="manual" data-html="true" data-placement="bottom" id="imgticker_' + ticker + '" /> \
                                </a> \
                            </td> \
                            <td width="70%" style="padding-left:10px" class="text-left"> \
                                <p class="ticker_info"> \
                                    <a href="company/' + ticker + '/estimates" class="urls"><strong>' + cutter(company,11) + '</strong></a> \
                                </p> \
                                <p class="ticker_info"> \
                                    <span style="color:gray;font-size:x-small">' + (sector=="1"?'<i class="icon-ok-sign" style="color:#62c462"></i> TARGET HIT!':master_horizon(sector_id)) + '</span> \
                                </p> \
                                <p class="ticker_info"> \
                                    <a href="company/' + ticker + '/estimates" style="font-size:x-small;margin:0;padding:3px;line-height:1;" class="btn btn-small btn-' + (sector=="1"?'success':'info') + '">Revise</a> <span rel="tooltip" data-toggle="tooltip" title="Return to date" style="font-size:x-small;float:right">' + color_coder(upside,true,1) + '</span> \
                                </p> \
                            </td> \
                        </tr> \
                    </table>';
        }

        function row_builder(ticker,company,sector,upside,sector_id) {
            var star = null;
            if (userid>0)
                star = '<i class="icon-star-empty" style="float:right;color:silver;cursor:pointer" onclick="return follow(this,\'' + sector + '\');" title="Add to watchlist" rel="tooltip" data-toggle="tooltip"></i>';
            else
                star = '<a href="signup" class="urls"><i class="icon-star-empty" style="float:right;color:silver" rel="tooltip" data-toggle="tooltip" title="Add to watchlist"></i></a>';

            return '<table border="0"> \
                        <tr class="noteworthy_tr"> \
                            <td width="30%" class="text-center"> \
                                <a href="company/' + ticker + '/noteworthy"> \
                                    <img src="<%=Page.ResolveUrl("~")%>images/logo/' + ticker + '.png" class="img-polaroid img-rounded show-tooltip" style="width:50px;height:50px" onerror="imgError(this);" title="" data-trigger="manual" data-html="true" data-placement="bottom" id="imgticker_' + ticker + '" /> \
                                </a> \
                            </td> \
                            <td width="70%" style="padding-left:10px" class="text-left"> \
                                <p class="ticker_info"> \
                                    <a href="company/' + ticker + '/noteworthy" class="urls"><strong>' + cutter(company,11) + '</strong></a> \
                                </p> \
                                <p class="ticker_info" data-toggle="tooltip" rel="tooltip" title="' + accounting.formatMoney(upside) + ' price target" data-placement="bottom">'
                                    +  color_coder(sector_id,true,0) + ' <span style="color:gray;font-size:x-small">CONSENSUS</span> \
                                </p> \
                                <p class="ticker_info"> \
                                    <a href="company/' + ticker + '/noteworthy" style="font-size:x-small;margin:0;padding:3px;line-height:1;" class="btn btn-small btn-info">Estimate</a>' + star +
                                '</p> \
                            </td> \
                        </tr> \
                    </table>';
        }

        function follow(el,ticker) {
            var x = $(el);

            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Users.asmx/follow_company_ajax",
                dataType: "json",
                data: "{company:" + ticker + "}",
                success: function (data) {
                    if (data.d){
                        x.attr('class','icon-star');
                        x.attr('style','float:right;color:gold;cursor:pointer');
                        x.attr('title', "Remove from watchlist").tooltip('fixTitle').tooltip('show');
                    }
                    else{
                        x.attr('class','icon-star-empty');
                        x.attr('style','float:right;color:silver;cursor:pointer');
                        x.attr('title', "Add to watchlist").tooltip('fixTitle').tooltip('show');
                    }

                    index_watchlist = 0;
                    watchlist();
                }
            });

            return false;
        }

        function row_builder_user(name,userid,sector,upside,sector_id,follow) {
            var link_user = (sector=="Investor"?"investor/":"analyst/") + userid + "/noteworthy";

            return '<table border="0"> \
                        <tr class="noteworthy_tr"> \
                            <td width="30%" class="text-center"> \
                                <a href="' + link_user + '" class="urls"> \
                                    <img src="<%=Page.ResolveUrl("~")%>images/user/' + userid + '.png" class="img-polaroid img-rounded show-tooltip" style="width:50px;height:50px" onerror="imgError_user(this);" title="" data-trigger="manual" data-html="true" data-placement="bottom" id="imgticker_' + userid + '" /> \
                                </a> \
                            </td> \
                            <td width="70%" style="padding-left:10px" class="text-left"> \
                                <p class="ticker_info"> \
                                    <a href="' + link_user + '" class="urls"><strong>' + cutter(name,10) + '</strong></a> \
                                </p> \
                                <p class="ticker_info"> \
                                    <span style="color:gray;font-size:x-small">' + cutter(sector,16) + '</span> \
                                </p> \
                                <p class="ticker_info"> \
                                    <a href="' + link_user + '" id="btn_follow" style="font-size:small;" class="urls">Follow</a> \
                                </p> \
                            </td> \
                        </tr> \
                    </table>';
        }

        function set_portfolio_doubleknob(price, time) {
            $("#portfolio_price").attr("data-fgColor", price > 0 ? "#62c462" : price < 0 ? "#ee5f5b" : "gray");
            $("#portfolio_price").attr("data-reverse", price >= 0 ? "false" : "true");
            $("#portfolio_price").attr("value", Math.abs(price) + "%");
            $("#portfolio_price").knob();
            $("#portfolio_price").show();
            $("#portfolio_time").val(time).trigger('change');
        }

    </script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="body">

    <%--<div style="height:5px"></div>--%>

    <div class="row" id="carousel" style="display:none">
        <div class="span12">
            <div class="thumbnail" style="background-color:white">
                 <div id="myCarousel" class="carousel slide" style="margin-bottom:0">
              <%--<ol class="carousel-indicators">--%>
                <%--<li data-target="#myCarousel" data-slide-to="0" class="active"></li>
                <li data-target="#myCarousel" data-slide-to="1"></li>
                <li data-target="#myCarousel" data-slide-to="2"></li>--%>
                  <%--<li data-target="#myCarousel" data-slide-to="0" class="active"></li>--%>
              
              <!-- Carousel items -->
            <span style="float:right;font-size:20pt"><a class="urls" style="cursor:pointer" onclick="close_carousel();">&times;</a></span>
              <div class="carousel-inner">
                <div class="active item">
                    <div class="text-center">
                        <img src="images/temp/tmp_01.png" class="img-rounded img-polaroid" style="height:200px">
                    </div>
                    
                    <div class="carousel-caption">
                        <h4>Discover</h4>
                        <p>Personalized social and analyst insights</p>
                    </div>
                </div>
                <div class="item">
                    <div class="text-center">
                        <img src="images/temp/tmp_01.png" class="img-rounded img-polaroid" style="height:200px">
                    </div>
                    <div class="carousel-caption">
                        <h4>Interact</h4>
                        <p>Build diverisied portfolios of top picks</p>
                    </div>
                </div>
                <div class="item">
                    <div class="text-center">
                        <img src="images/temp/tmp_01.png" class="img-rounded img-polaroid" style="height:200px">
                    </div>
                    <div class="carousel-caption">
                        <h4>Invesd</h4>
                        <p>Notifications and alerts on trends and insights</p>
                    </div>
                </div>
                <div class="item">
                    <div class="text-center">
                        <img src="images/temp/tmp_01.png" class="img-rounded img-polaroid" style="height:200px">
                    </div>
                    <div class="carousel-caption">
                        <h4>$100k investment challenge</h4>
                        <p>Deploy $100k and learn, build track record</p>
                    </div>
                </div>
              </div>
              <!-- Carousel nav -->
              <a class="carousel-control left" href="#myCarousel" data-slide="prev">&lsaquo;</a>
              <a class="carousel-control right" href="#myCarousel" data-slide="next">&rsaquo;</a>
            </div>
            </div>
           
            <%--<div class="alert alert-info">
                <a class="close" data-dismiss="alert" href="#" aria-hidden="true">&times;</a>
                <div style="height:250px">
                    <table border="0" width="100%">
                        <tr>
                            <td width="40%">
                                <h3>Analyst insights</h3>
                                <ul>
                                    <li style="font-size:x-large">4,500 analysts</li>
                                    <li style="font-size:x-large">4,000+ stocks</li>
                                    <li style="font-size:x-large">5% outperformance</li>
                                </ul>
                            </td>
                            <td width="60%" class="text-right">
                                <img src="images/temp/tmp_01.png" class="img-rounded img-polaroid" style="height:200px" />
                            </td>
                        </tr>
                    </table>
                </div>
            </div>--%>
        </div>
    </div>

    <div style="height:15px;display:none" id="carousel_space"></div>

    <div class="row">
        <div class="span12">
            <div class="alert alert-success text-center" style="display:none;font-size:1.2em" id="div_quick_tips">
                <i class="icon-lightbulb icon-2x" style="font-size:1.5em;float:left" title="Tips" data-placement="right" rel="tooltip" data-toggle="tooltip"></i>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span id="l_quick_tip" style="color:black"></span>
            </div>

            <div class="alert alert-info text-center" style="display:none;font-size:1.2em" id="div_quick_tips2">
                <table style="border:0;width:100%">
                    <tr>
                        <td style="width:75%;text-align:left">
                            <strong>Build your track record</strong>
                            <br />
                            Unlock powerful insights by contributing your estimates.
                        </td>
                        <td style="width:25%">
                            <asp:TextBox runat="server" placeholder="E-mail address" class="form-control" id="inEmail" autocomplete="off" style="margin-bottom:5px" />
                            <asp:Button ID="Button1" runat="server" CssClass="btn btn-block btn-success btn-lg" PostBackUrl="~/signup?ref=home" Text="Signup for free"></asp:Button>
                        </td>
                    </tr>
                </table>
            </div>

            <ul class="nav nav-tabs" style="margin-bottom:0">
                <li id="estimates_link" class="active">
                    <a href="#estimates" data-toggle="tab">Estimates</a>
                </li>
                <li id="watchlist_link">
                    <a href="#watchlist" data-toggle="tab">Watchlist</a>
                </li>
                <li id="noteworthy_link">
                    <a href="#noteworthy" data-toggle="tab">Noteworthy stocks</a>
                </li>
                
                <%--<li>
                    <a href="#followed" data-toggle="tab">Investors & Analysts</a>
                </li>--%>
                <%--<li style="float:right;display:none" id="personalized">
                    <div class="progress" style="margin-bottom:0;width:150px;margin-top:10px;">                        
                        <div class="bar" id="personalized_bar"></div>
                    </div>
                </li>--%>
                <%--<li style="float:right;display:none;margin-top:10px" id="personalized2">
                    <span class="urls" style="cursor:pointer;color:#0088cc" onclick="progress(progress_value);">Personalize</span> your experience&nbsp;&nbsp;&nbsp;
                </li>--%>
                
            </ul>
            <div class="tab-content">
                <div class="tab-pane active" id="estimates">
                    <div class="thumbnail" style="background-color:white;border-top:0;border-top-left-radius:0;border-top-right-radius:0">
                        <div id="EstimatesDiv" style="height:70px">
                            <table width="100%" style="margin:auto;height:70px" border="0">
                                <tr>
                                    <td width="4%" class="text-center">
                                        <a style="cursor:pointer" onclick="navigate_estimates(-1);" class="urls"><i class="icon-chevron-left icon-2x"></i></a>
                                    </td>
                                    <td width="21.5%" id="estimate1" style="text-align:center;vertical-align:middle"></td>
                                    <td width="1%" style="border-right:1px solid #eeeeee"></td><td width="1%"></td>
                                    <td width="21.5%" id="estimate2" style="text-align:center;vertical-align:middle"></td>
                                    <td width="1%" style="border-right:1px solid #eeeeee"></td><td width="1%"></td>
                                    <td width="21.5%" id="estimate3" style="text-align:center;vertical-align:middle"></td>
                                    <td width="1%" style="border-right:1px solid #eeeeee"></td><td width="1%"></td>
                                    <td width="21.5%" id="estimate4" style="text-align:center;vertical-align:middle"></td>
                                    <td width="4%" class="text-center">
                                        <a style="cursor:pointer" onclick="navigate_estimates(1);" class="urls"><i class="icon-chevron-right icon-2x"></i></a>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </div>
                </div>
                <div class="tab-pane" id="watchlist">
                    <div class="thumbnail" style="background-color:white;border-top:0;border-top-left-radius:0;border-top-right-radius:0">
                        <div id="WatchlistDiv" style="height:70px">
                            <table width="100%" style="margin:auto;height:70px" border="0" id="tbl_watchlist">
                                <tr>
                                    <td width="4%" class="text-center">
                                        <a style="cursor:pointer" onclick="navigate_watchlist(-1);" class="urls"><i class="icon-chevron-left icon-2x"></i></a>
                                    </td>
                                    <td width="21.5%" id="watchlist1" style="text-align:center;vertical-align:middle"></td>
                                    <td width="1%" style="border-right:1px solid #eeeeee"></td><td width="1%"></td>
                                    <td width="21.5%" id="watchlist2" style="text-align:center;vertical-align:middle"></td>
                                    <td width="1%" style="border-right:1px solid #eeeeee"></td><td width="1%"></td>
                                    <td width="21.5%" id="watchlist3" style="text-align:center;vertical-align:middle"></td>
                                    <td width="1%" style="border-right:1px solid #eeeeee"></td><td width="1%"></td>
                                    <td width="21.5%" id="watchlist4" style="text-align:center;vertical-align:middle"></td>
                                    <td width="4%" class="text-center">
                                        <a style="cursor:pointer" onclick="navigate_watchlist(1);" class="urls"><i class="icon-chevron-right icon-2x"></i></a>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </div>
                </div>
                <div class="tab-pane" id="noteworthy">
                    <div class="thumbnail" style="background-color:white;border-top:0;border-top-left-radius:0;border-top-right-radius:0">
                        <div id="PositionsDiv" style="height:70px">
                            <table width="100%" style="margin:auto;height:70px" border="0">
                                <tr>
                                    <td width="4%" class="text-center">
                                        <a style="cursor:pointer" onclick="note(-1);" class="urls"><i class="icon-chevron-left icon-2x"></i></a>
                                    </td>
                                    <td width="21.5%" id="noteworthy1"></td>
                                    <td width="1%" style="border-right:1px solid #eeeeee"></td><td width="1%"></td>
                                    <td width="21.5%" id="noteworthy2"></td>
                                    <td width="1%" style="border-right:1px solid #eeeeee"></td><td width="1%"></td>
                                    <td width="21.5%" id="noteworthy3"></td>
                                    <td width="1%" style="border-right:1px solid #eeeeee"></td><td width="1%"></td>
                                    <td width="21.5%" id="noteworthy4"></td>
                                    <td width="4%" class="text-center">
                                        <a style="cursor:pointer" onclick="note(1);" class="urls"><i class="icon-chevron-right icon-2x"></i></a>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </div>
                </div>
                <div class="tab-pane" id="followed">
                    <div class="thumbnail" style="background-color:white;border-top:0;border-top-left-radius:0;border-top-right-radius:0">
                        <div id="FollowedDiv" style="height:70px">
                            <table width="100%" style="margin:auto;height:70px" border="0">
                                <tr>
                                    <td width="4%" class="text-center">
                                        <a style="cursor:pointer" onclick="navigate_followed(-1);" class="urls"><i class="icon-chevron-left icon-2x"></i></a>
                                    </td>
                                    <td width="21.5%" id="followed1"></td>
                                    <td width="1%" style="border-right:1px solid #eeeeee"></td><td width="1%"></td>
                                    <td width="21.5%" id="followed2"></td>
                                    <td width="1%" style="border-right:1px solid #eeeeee"></td><td width="1%"></td>
                                    <td width="21.5%" id="followed3"></td>
                                    <td width="1%" style="border-right:1px solid #eeeeee"></td><td width="1%"></td>
                                    <td width="21.5%" id="followed4"></td>
                                    <td width="4%" class="text-center">
                                        <a style="cursor:pointer" onclick="navigate_followed(1);" class="urls"><i class="icon-chevron-right icon-2x"></i></a>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div style="height:20px"></div>

    <div class="row">
        <div class="span12">
            <table border="0" width="100%">
            <tr>
                <td width="32%" style="vertical-align:top">                 
                    <ul class="nav nav-tabs" style="margin-bottom:0">
                        <li class="active">
                            <a href="#estimate_stats">Estimate stats</a>
                        </li>
                        <li style="display:none">
                            <a href="#portfolio">Portfolio</a>
                        </li>
                    </ul>
                    <div class="tab-content">
                        <div class="tab-pane active" id="estimate_stats">
                            <div class="thumbnail" style="background-color:white;margin-top:0;border-top-left-radius:0;border-top-right-radius:0;border-top:0">
                                <table style="width:100%">
                                    <tr style="border-bottom:1px solid #eeeeee">
                                        <td style="width:50%;border-right:1px solid #eeeeee" class="text-center">
                                            <span style="font-size:large" id="estimates_count"><img src="images/ajax_loader.gif" style="width:30px" /></span>
                                            <br />
                                            <span style="color:gray;font-size:x-small">ESTIMATES</span>
                                        </td>
                                        <td style="width:50%;" class="text-center">
                                            <span style="font-size:large" id="sectors"><img src="images/ajax_loader.gif" style="width:30px" /></span>
                                            <br />
                                            <span style="color:gray;font-size:x-small">SECTORS</span>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="width:50%;border-right:1px solid #eeeeee" class="text-center">
                                            <span style="font-size:large" id="score"><img src="images/ajax_loader.gif" style="width:30px" /></span>
                                            <br />
                                            <span style="color:gray;font-size:x-small">POINTS</span>
                                        </td>
                                        <td style="width:50%" class="text-center">
                                            <span style="font-size:large" id="avg"><img src="images/ajax_loader.gif" style="width:30px" /></span>
                                            <br />
                                            <span style="color:gray;font-size:x-small">AVG. RETURN</span>
                                        </td>
                                    </tr>
                                    <tr style="display:none" id="show_sector">
                                        <td colspan="2" style="text-align:center">
                                            <div style="width:280px;height:200px;margin-right:auto;margin-left:auto" id="div_sectors"></div>
                                            <span style="color:gray;font-size:x-small">ESTIMATE BREAKDOWN BY SECTOR</span>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2" style="text-align:right">
                                            <a href="leaderboard" style="font-size:x-small">VIEW RANKING IN LEADERBOARD</a>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </div>
                        <div class="tab-pane" id="portfolio">
                            <div class="thumbnail" style="background-color:white;margin-top:0;border-top-left-radius:0;border-top-right-radius:0">
                                <table style="width:100%">
                                    <tr style="border-bottom:1px solid #eeeeee">
                                        <td style="width:50%;border-right:1px solid #eeeeee" class="text-center">
                                            <span style="font-size:large" id="portfolio_count"><img src="images/ajax_loader.gif" style="width:30px" /></span>
                                            <br />
                                            <span style="color:gray;font-size:x-small">ESTIMATES</span>
                                        </td>
                                        <td style="width:50%;" class="text-center">
                                            <span style="font-size:large" id="portfolio_sectors"><img src="images/ajax_loader.gif" style="width:30px" /></span>
                                            <br />
                                            <span style="color:gray;font-size:x-small">SECTORS</span>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="width:50%;border-right:1px solid #eeeeee" class="text-center">
                                            <span style="font-size:large" id="portfolio_score"><img src="images/ajax_loader.gif" style="width:30px" /></span>
                                            <br />
                                            <span style="color:gray;font-size:x-small">POINTS</span>
                                        </td>
                                        <td style="width:50%" class="text-center">
                                            <span style="font-size:large" id="portfolio_avg"><img src="images/ajax_loader.gif" style="width:30px" /></span>
                                            <br />
                                            <span style="color:gray;font-size:x-small">AVG. RETURN</span>
                                        </td>
                                    </tr>
                                    <tr style="display:none" id="portfolio_show_sector">
                                        <td colspan="2" style="text-align:center">
                                            <div style="width:280px;height:200px;margin-right:auto;margin-left:auto" id="portfolio_div_sectors"></div>
                                            <span style="color:gray;font-size:x-small">ESTIMATE BREAKDOWN BY SECTOR</span>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2" style="text-align:right">
                                            <a href="leaderboard" style="font-size:x-small">VIEW RANKING IN LEADERBOARD</a>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </div>
                    </div>
                    
                </td>
                <td width="3%"></td>
                <td width="65%" style="vertical-align:top">
                    <table border="0" width="100%">
                        <tr>
                            <td width="40%">
                                <h4>Feed <span style="font-size:medium"><i class="icon-info-sign" rel="tooltip" data-placement="right" title="Activity of companies and investors followed or invested in"></i></span></h4>
                            </td>
                            <td width="60%" class="text-right">
                                <%--<select style="margin:0;width:120px;display:none" onchange="get_feed();" id="feed_selector">
                                    <option value="Personal">Personal</option>
                                    <option value="Public">Public</option>
                                </select>--%>
                            </td>
                        </tr>
                    </table>
                    <div class="thumbnail" style="width:100%;margin:0;border:0;padding:0" id="TableNotification"></div>
                   <!-- Load more companies button -->
                <div class="text-center" id="loadmore">
                    <div style="height:5px"></div>
                    <a class="urls fake-link" onclick="return load_more();"><i class="icon-user"></i> Load more</a>
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

    <div id="quest" class="modal hide fade " tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-header">
            <button type="button" class="close" data-dismiss="modal" aria-hidden="true"><i class="icon-remove-sign"></i></button>
            <h3 id="quest_1st">Welcome</h3><h3 id="quest_2nd" style="display:none">Step <span id="quest_step"></span> of <span id="quest_total">3</span></h3>
        </div>
        <div class="modal-body text-center" id="modalData">
            <div id="step1" style="display:none">
                <h4>What type of investor are you?</h4>
                <table style="width:100%">
                <tr>
                    <td style="width:35%"></td>
                    <td style="width:30%">
                        <a onclick="return step_click(1,0)" class="btn btn-large btn-block btn-info">I'm a new investor</a>
                        <span style="color:gray;font-size:x-small">
                            I haven't invested in stocks before and I don't have a brokerage account.
                        </span>
                    </td>
                    <td style="width:35%"></td>
                </tr>
                <tr>
                    <td></td>
                    <td>&nbsp;</td>
                    <td></td>
                </tr>
                <tr>
                    <td></td>
                    <td>
                        <a onclick="return step_click(1,1)" class="btn btn-large btn-block btn-info">I'm experienced</a>
                        <span style="color:gray;font-size:x-small">
                            I have a brokerage account and I have invested in at least one stock.
                        </span>
                    </td>
                    <td></td>
                </tr>
            </table>
            </div>
            <div id="step2_newbie" style="display:none">
                <h4>What do you want to accomplish?</h4>
                <table style="width:100%">
                <tr>
                    <td style="width:10%"></td>
                    <td style="width:80%">
                        <label class="checkbox text-left">
                            <input type="checkbox" id="inlineCheckbox1" value="option1"> Learn how stock investing works in a risk-free environment
                        </label>
                        <label class="checkbox text-left">
                            <input type="checkbox" value="option2"> Open a brokerage account and start investing in the future
                        </label>
                        <label class="checkbox text-left">
                            <input type="checkbox" value="option2"> Find sources of information that I can understand and trust
                        </label>
                        <label class="checkbox text-left">
                            <input type="checkbox" value="option2"> Learn about fundamental or technical analysis
                        </label>
                        <label class="checkbox text-left">
                            <input type="checkbox" value="option2"> Learn from the community
                        </label>
                        <a href="#" class="btn btn-large btn-info">Next step</a>
                    </td>
                    <td style="width:10%"></td>
                </tr>
            </table>
            </div>
            <div id="step2_expert" style="display:none">
                <h4></h4>
            </div>
            <div id="step2" style="display:none">
                <h4>Which one best describes your investing goals?</h4>
                    <label class="radio text-left">
                        <input type="radio" name="risk" value="1">
                        I am looking for steady returns, dividends, low volatility, alternative to putting money in a saving account.
                    </label>
                    <label class="radio text-left">
                        <input type="radio" name="risk" value="2">
                        I want to have a balanced and diversified portfolio, taking on more risk and can tolerate modest market drops.
                    </label>
                    <label class="radio text-left">
                        <input type="radio" name="risk" value="3">
                        I am looking for huge gains and am comfortable losing substantial capital.
                    </label>
                    <a onclick="return step_click(2,0);" class="btn btn-large btn-info">Submit</a>
            </div>
            <div id="step3" style="display:none">
                <h4>Do you want to link your brokerage account?</h4>
                <a onclick="return step_click(3,true);" class="btn btn-large btn-info" style="cursor:pointer">Yes</a> <a onclick="return step_click(3,false);" class="btn btn-large btn-info" style="cursor:pointer">No</a>
            </div>

            <p class="text-right">
                <button type="button" class="close" data-dismiss="modal" aria-hidden="true">Skip for now</button>
            </p>
        </div>
    </div>

</asp:Content>