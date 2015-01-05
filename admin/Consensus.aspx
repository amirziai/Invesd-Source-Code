<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Consensus.aspx.cs" Inherits="admin_Consensus" MasterPageFile="~/admin/MasterPage_Admin.master" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
    <script src="<% =Page.ResolveUrl("~")%>js/tablesorter/jquery.tablesorter.min.js"></script>
    <script src="<%= Page.ResolveUrl("~/")%>js/highstock.js" type="text/javascript"></script>
    <script src="https://code.highcharts.com/modules/exporting.js"></script>
    <script src="https://rawgithub.com/highslide-software/export-csv/master/export-csv.js"></script>
    <%--<script src="<%= Page.ResolveUrl("~/")%>js/highcharts-more.js" type="text/javascript"></script>
    <script src="<%= Page.ResolveUrl("~/")%>js/modules/exporting.src.js" type="text/javascript"></script>
    <script src="<%= Page.ResolveUrl("~/")%>js/draggable-points.js" type="text/javascript"></script>
    <script type="text/javascript" src="<%= Page.ResolveUrl("~/")%>highslide/highslide-full.js"></script>--%>
    
    <script type="text/javascript">

        var categories = new Array();

        var total_flat = new Array();
        var total_weighted = new Array();
        var ws_flat = new Array();
        var ws_weighted = new Array();
        var crowd_flat = new Array();
        var crowd_weighted = new Array();

        var total_flat_changes = new Array();
        var total_weighted_changes = new Array();
        var ws_flat_changes = new Array();
        var ws_weighted_changes = new Array();
        var crowd_flat_changes = new Array();
        var crowd_weighted_changes = new Array();

        var total_flat_changes_2nd = new Array();
        var total_weighted_changes_2nd = new Array();
        var ws_flat_changes_2nd = new Array();
        var ws_weighted_changes_2nd = new Array();
        var crowd_flat_changes_2nd = new Array();
        var crowd_weighted_changes_2nd = new Array();

        var price = new Array();
        var count_ws;
        var count_crowd;
        var count_ws_weighted;
        var count_crowd_weighted;
        var consensus_weight_contribution_ws_flat;
        var consensus_weight_contribution_ws_weighted;
        var consensus_weight_contribution_crowd_flat;
        var consensus_weight_contribution_crowd_weighted;
        var analysts = new Array;
        var confidence = new Array;
        var contribution_flat = new Array;
        var contribution_weighted = new Array;
        var range = new Array;
        var recency = new Array;
        var expiration = new Array;
        var range_upper;
        var range_lower;

        var scatter_ws = new Array;
        var scatter_crowd = new Array;

        var trends_total = new Array;
        var trends_total_2nd = new Array;
        var trends_total_w = new Array;
        var trends_total_w_2nd = new Array;
        var trends_ws = new Array;
        var trends_ws_2nd = new Array;
        var trends_ws_w = new Array;
        var trends_ws_w_2nd = new Array;
        var trends_crowd = new Array;
        var trends_crowd_2nd = new Array;
        var trends_crowd_w = new Array;
        var trends_crowd_w_2nd = new Array;

        var sector_p = new Array;
        var sector_pt = new Array;
        var sector_u = new Array;
        var sector_u_1q = new Array;
        var sector_u_4q = new Array;

        var colors_array = ["#0088cc", "#62c462"];

        function toggle_scatter(ws) {
            var chart = $('#scatterplot').highcharts();
            var series = chart.series[ws ? 0 : 1];
            if (series.visible) {
                series.hide();
            }
            else {
                series.show();
            }
        }

        function pie_chart(div,ws,crowd) {

            new Highcharts.Chart({
                chart: {
                    renderTo: div,
                    height: 150,
                    width: 150,
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
                    enabled: false,
                },
                title: {
                    text: null
                },
                plotOptions: {
                    pie: {
                        allowPointSelect: false,
                        cursor: 'pointer',
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
                    data: [["Wall St",ws],["Crowd",crowd]]
                }]
            });
        }

        function isBiggerThanZero(val) {
            return val.y > 0;
        }

        $(document).ready(function () {
            plot_consensus();
            plot_consensus_changes();
            plot_consensus_changes_2nd();

            plot_trends();
            plot_trends_2nd();

            plot_sector();

            $("#tbl_co").tablesorter();
            $("#tbl").tablesorter();
            $("#tbl_today").tablesorter();
            $("#tbl_consensus_now").tablesorter();
            
            pie_chart('pie_flat_no', count_ws, count_crowd);
            pie_chart('pie_weighted_no', count_ws_weighted, count_crowd_weighted);
            pie_chart('pie_flat_consensus', consensus_weight_contribution_ws_flat, consensus_weight_contribution_crowd_flat);
            pie_chart('pie_weighted_consensus', consensus_weight_contribution_ws_weighted, consensus_weight_contribution_crowd_weighted);
            plot_confidence('#chart_confidence', analysts, confidence);
            plot_confidence('#chart_contribution_flat', analysts, contribution_flat);
            plot_confidence('#chart_contribution_weighted', analysts, contribution_weighted);
            scatter_plot();

            if (range.filter(isBiggerThanZero).length > 0)
                plot_confidence('#chart_range', analysts, range);
            else
                $("#chart_range").hide();

            if (recency.filter(isBiggerThanZero).length > 0)
                plot_confidence('#chart_recency', analysts, recency);
            else
                $("#chart_recency").hide();

            if (expiration.filter(isBiggerThanZero).length > 0)
                plot_confidence('#chart_expiration', analysts, expiration);
            else
                $("#chart_expiration").hide();
        });

        function scatter_plot() {
            Highcharts.setOptions({ global: { useUTC: true } });
            var now = new Date();
            $('#scatterplot').highcharts({
                chart: {type:'bubble'},
                title: { text: null },
                legend:{enabled:false},
                xAxis: {type: 'datetime', offset: 30},
                yAxis: {gridLineColor: '#eeeeee',
                    plotLines: [{value: range_upper, id: 'last', color: 'silver', dashStyle: 'dashed', width: 1,label: { text: range_upper.toFixed(2) + ' Upper bound (mu+2sig)', align: 'left', rotation: 0 }},
                    {value: range_lower, id: 'last', color: 'silver', dashStyle: 'dashed', width: 1,label: { text: range_lower.toFixed(2) + ' Lower bound (mu-2sig)', align: 'left', rotation: 0 }},
                    {value: current_price, id: 'last', color: 'gray', dashStyle: 'dashed', width: 1,label: { text: current_price + ' Current price', align: 'left', rotation: 0 }}]},
                credits: { enabled: false },
                exporting: { enabled: false },
                tooltip: { formatter: function () {return "<b>" + this.series.name + "</b><br>" + this.point.name + ": " + this.point.y;}},
                series: [{
                    name: 'WS PTs',
                    data: scatter_ws,
                    type: 'scatter',
                    marker: { fillColor: 'rgba(0, 0, 0, 1)', radius: 5, symbol: 'circle' },
                    shadow: false,
                    tooltip: { formatter: function() {return '<b>' + this.point.name + "<br>" + this.point.name + "</b>: " + this.y;} },
                    pointInterval: 24 * 3600 * 1000,
                    pointStart: Date.UTC(now.getYear() - 1, now.getMonth(), now.getDate()),

                },
                {
                    name: 'Crowd PTs',
                    data: scatter_crowd,
                    type: 'scatter',
                    marker: { fillColor: 'rgba(0, 0, 0, 1)', radius: 5, symbol: 'square' },
                    shadow: false,
                    tooltip: { valueDecimals: 2 },
                    pointInterval: 24 * 3600 * 1000,
                    pointStart: Date.UTC(now.getYear() - 1, now.getMonth(), now.getDate()),

                }]
            });
        }

        function plot_confidence(div,analysts,data)
        {
            var max_value = 0;
            for (var i = 0; i <data.length; i++) {
                if (data[i].y > max_value)
                    max_value = data[i].y;
            }

            $(div).highcharts({    
                chart: {type: 'column'},    
                title: {text: null},
                legend: { enabled: false },
                credits: { enabled: false },
                exporting: { enabled: false },
                xAxis: { categories: analysts, labels: { enabled: false } },
                yAxis: { max: max_value, title: { text: div.replace("#chart_", "").replace("contribution_","cont. ") }, labels: { enabled: false } },
                tooltip: {formatter: function() {return '<b>'+ this.x +'</b><br/>'+this.series.name +': '+ this.y;}},
                series: [{ name: div.replace("#chart_", "").replace("contribution_", "cont. "), data: data }]
            });
        }

        function plot_consensus_changes_2nd() {
            $('#chart_changes_2nd').highcharts({
                title: { text: 'Consensus changes of changes', x: -20 },
                tooltip: { shared: true, crosshairs: true },
                xAxis: { categories: categories },
                yAxis: { title: { text: null }, },
                legend: { layout: 'vertical', align: 'right', verticalAlign: 'middle', borderWidth: 0 },
                series: [
                { name: 'Total Flat', data: total_flat_changes_2nd },
                { name: 'Total Weighted', data: total_weighted_changes_2nd },
                { name: 'WS Flat', data: ws_flat_changes_2nd },
                { name: 'WS Weighted', data: ws_weighted_changes_2nd },
                { name: 'Crowd Flat', data: crowd_flat_changes_2nd },
                { name: 'Crowd Weighted', data: crowd_weighted_changes_2nd }]
            });
        }

        function plot_consensus_changes() {
            $('#chart_changes').highcharts({
                title: { text: 'Consensus changes', x: -20 },
                tooltip: { shared: true, crosshairs: true },
                xAxis: { categories: categories },
                yAxis: { title: { text: null }, },
                legend: { layout: 'vertical', align: 'right', verticalAlign: 'middle', borderWidth: 0 },
                series: [
                { name: 'Total Flat', data: total_flat_changes },
                { name: 'Total Weighted', data: total_weighted_changes },
                { name: 'WS Flat', data: ws_flat_changes },
                { name: 'WS Weighted', data: ws_weighted_changes },
                { name: 'Crowd Flat', data: crowd_flat_changes },
                { name: 'Crowd Weighted', data: crowd_weighted_changes }]
            });
        }

        function plot_consensus() {
            $('#chart').highcharts({
                title: { text: 'Consensus', x: -20 },
                tooltip: { shared: true, crosshairs: true },
                xAxis: { categories: categories },
                yAxis: { title: { text: null }, },
                legend: { layout: 'vertical', align: 'right', verticalAlign: 'middle', borderWidth: 0 },
                series: [{ name: 'Price', data: price },
                { name: 'Total Flat', data: total_flat },
                { name: 'Total Weighted', data: total_weighted },
                { name: 'WS Flat', data: ws_flat },
                { name: 'WS Weighted', data: ws_weighted },
                { name: 'Crowd Flat', data: crowd_flat },
                { name: 'Crowd Weighted', data: crowd_weighted }]
            });
        }

        function plot_trends() {
            trends_total = trends_total.reverse();
            trends_total_w = trends_total_w.reverse();
            trends_ws = trends_ws.reverse();
            trends_ws_w = trends_ws_w.reverse();
            trends_crowd = trends_crowd.reverse();
            trends_crowd_w = trends_crowd_w.reverse();

            $('#chart_trends').highcharts({
                title: { text: 'Trend', x: -20 },
                tooltip: { shared: true, crosshairs: true },
                xAxis: { categories: ['12m', '6m', '3m', '1m', '1w'] },
                yAxis: { title: { text: null }, },
                legend: { layout: 'vertical', align: 'right', verticalAlign: 'middle', borderWidth: 0 },
                series: [
                { name: 'Total Flat', data: trends_total },
                { name: 'Total Weighted', data: trends_total_w },
                { name: 'WS Flat', data: trends_ws },
                { name: 'WS Weighted', data: trends_ws_w },
                { name: 'Crowd Flat', data: trends_crowd },
                { name: 'Crowd Weighted', data: trends_crowd_w }]
            });
        }

        function plot_trends_2nd() {
            trends_total_2nd = trends_total_2nd.reverse();
            trends_total_w_2nd = trends_total_w_2nd.reverse();
            trends_ws_2nd = trends_ws_2nd.reverse();
            trends_ws_w_2nd = trends_ws_w_2nd.reverse();
            trends_crowd_2nd = trends_crowd_2nd.reverse();
            trends_crowd_w_2nd = trends_crowd_w_2nd.reverse();

            $('#chart_trends_2nd').highcharts({
                title: { text: 'Trend 2nd', x: -20 },
                tooltip: { shared: true, crosshairs: true },
                xAxis: { categories: ['12m', '6m', '3m', '1m', '1w'] },
                yAxis: { title: { text: null }, },
                legend: { layout: 'vertical', align: 'right', verticalAlign: 'middle', borderWidth: 0 },
                series: [
                { name: 'Total Flat', data: trends_total_2nd },
                { name: 'Total Weighted', data: trends_total_w_2nd },
                { name: 'WS Flat', data: trends_ws_2nd },
                { name: 'WS Weighted', data: trends_ws_w_2nd },
                { name: 'Crowd Flat', data: trends_crowd_2nd },
                { name: 'Crowd Weighted', data: trends_crowd_w_2nd }]
            });
        }

        function plot_sector() {
            $('#chart_sector').highcharts({
                title: { text: 'Trend', x: -20 },
                tooltip: { shared: true, crosshairs: true },
                yAxis: { title: { text: null }, },
                legend: { layout: 'vertical', align: 'right', verticalAlign: 'middle', borderWidth: 0 },
                series: [
                { name: 'Price', data: sector_p },
                { name: 'Price target', data: sector_pt },
                { name: 'Upside', data: sector_u },
                { name: 'Upside 1Q', data: sector_u_1q },
                { name: 'Upside 4Q', data: sector_u_4q } ]
            });
        }
    </script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="body">
    <table style="width:100%">
        <tr>
            <td style="width:10%">
                <asp:Image runat="server" ID="logo" />
            </td>
            <td style="width:90%">
                <asp:Label runat="server" ID="ticker_info"></asp:Label>
            </td>
        </tr>
    </table>
    
    <hr />

    <asp:Label runat="server" ID="info"></asp:Label>

    <asp:Table runat="server" style="width:100%;border:2px solid gray">
        <asp:TableRow>
            <asp:TableCell style="width:25%;text-align:center">
                <b>FLAT # OF ANALYSTS</b>
                <div id="pie_flat_no" style="width:150px;height:150px;margin-right:auto;margin-left:auto"></div>
            </asp:TableCell>
            <asp:TableCell style="width:25%;text-align:center">
                <b>CONS WEIGHT FLAT</b>
                <div id="pie_flat_consensus" style="width:150px;height:150px;margin-right:auto;margin-left:auto"></div>
            </asp:TableCell>
            <asp:TableCell style="width:25%;text-align:center">
                <b>WEIGHTED # OF ANALYSTS</b>
                <div id="pie_weighted_no" style="width:150px;height:150px;margin-right:auto;margin-left:auto"></div>
            </asp:TableCell>
            <asp:TableCell style="width:25%;text-align:center">
                <b>CONS WEIGHT WEIGHTED</b>
                <div id="pie_weighted_consensus" style="width:150px;height:150px;margin-right:auto;margin-left:auto"></div>
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell ColumnSpan="4">
                <asp:Table runat="server" style="width:100%" CssClass="table table-striped" ID="tbl_consensus_now" ClientIDMode="Static">
                    <asp:TableHeaderRow TableSection="TableHeader">
                        <asp:TableHeaderCell></asp:TableHeaderCell>
                        <asp:TableHeaderCell>Price target</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Upside</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Analysts</asp:TableHeaderCell>
                        <asp:TableHeaderCell></asp:TableHeaderCell>
                        <asp:TableHeaderCell></asp:TableHeaderCell>
                        <asp:TableHeaderCell>Trend</asp:TableHeaderCell>
                    </asp:TableHeaderRow>
                    <asp:TableRow>
                        <asp:TableCell style="text-align:right">Total Flat</asp:TableCell>
                        <asp:TableCell ID="total_flat_pt"></asp:TableCell>
                        <asp:TableCell ID="total_flat_upside"></asp:TableCell>
                        <asp:TableCell ID="total_flat_analysts"></asp:TableCell>
                        <asp:TableCell ID="total_flat_analysts_viz"></asp:TableCell>
                        <asp:TableCell></asp:TableCell>
                        <asp:TableCell ID="total_flat_analysts_trend"></asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell style="text-align:right">Total Weighted</asp:TableCell>
                        <asp:TableCell ID="total_weighted_pt"></asp:TableCell>
                        <asp:TableCell ID="total_weighted_upside"></asp:TableCell>
                        <asp:TableCell ID="total_weighted_analysts"></asp:TableCell>
                        <asp:TableCell ID="total_weighted_analysts_viz"></asp:TableCell>
                        <asp:TableCell></asp:TableCell>
                        <asp:TableCell ID="total_weighted_analysts_trend"></asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell style="text-align:right">WS Flat</asp:TableCell>
                        <asp:TableCell ID="ws_flat_pt"></asp:TableCell>
                        <asp:TableCell ID="ws_flat_upside"></asp:TableCell>
                        <asp:TableCell ID="ws_flat_analysts"></asp:TableCell>
                        <asp:TableCell ID="ws_flat_analysts_viz"></asp:TableCell>
                        <asp:TableCell><a onclick="toggle_scatter(true)" class="btn"><i class="icon-eye-open"></i></a></asp:TableCell>
                        <asp:TableCell ID="ws_flat_analysts_trend"></asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell style="text-align:right">WS Weighted</asp:TableCell>
                        <asp:TableCell ID="ws_weighted_pt"></asp:TableCell>
                        <asp:TableCell ID="ws_weighted_upside"></asp:TableCell>
                        <asp:TableCell ID="ws_weighted_analysts"></asp:TableCell>
                        <asp:TableCell ID="ws_weighted_analysts_viz"></asp:TableCell>
                        <asp:TableCell></asp:TableCell>
                        <asp:TableCell ID="ws_weighted_analysts_trend"></asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell style="text-align:right">Crowd Flat</asp:TableCell>
                        <asp:TableCell ID="crowd_flat_pt"></asp:TableCell>
                        <asp:TableCell ID="crowd_flat_upside"></asp:TableCell>
                        <asp:TableCell ID="crowd_flat_analysts"></asp:TableCell>
                        <asp:TableCell ID="crowd_flat_analysts_viz"></asp:TableCell>
                        <asp:TableCell><a class="btn" onclick="toggle_scatter(false)"><i class="icon-eye-open"></i></a></asp:TableCell>
                        <asp:TableCell ID="crowd_flat_analysts_trend"></asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell style="text-align:right">Crowd Weighted</asp:TableCell>
                        <asp:TableCell ID="crowd_weighted_pt"></asp:TableCell>
                        <asp:TableCell ID="crowd_weighted_upside"></asp:TableCell>
                        <asp:TableCell ID="crowd_weighted_analysts"></asp:TableCell>
                        <asp:TableCell ID="crowd_weighted_analysts_viz"></asp:TableCell>
                        <asp:TableCell></asp:TableCell>
                        <asp:TableCell ID="crowd_weighted_analysts_trend"></asp:TableCell>
                    </asp:TableRow>
                </asp:Table>
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell ColumnSpan="4">
                <div style="width:100%;height:300px" id="scatterplot"></div>
                <asp:Label runat="server" ID="scatterplot_notes" style="color:red;font-size:small"></asp:Label>
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell ColumnSpan="4">
                <div style="width:100%;height:120px" id="chart_confidence"></div>
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell ColumnSpan="4">
                <div style="width:100%;height:120px" id="chart_contribution_flat"></div>
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell ColumnSpan="4">
                <div style="width:100%;height:120px" id="chart_contribution_weighted"></div>
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell ColumnSpan="4">
                <div style="width:100%;height:120px" id="chart_range"></div>
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell ColumnSpan="4">
                <div style="width:100%;height:120px" id="chart_recency"></div>
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell ColumnSpan="4">
                <div style="width:100%;height:120px" id="chart_expiration"></div>
            </asp:TableCell>
        </asp:TableRow>
    </asp:Table>
    <br />
    <h5>Analyst + scoring</h5>
    <asp:Table runat="server" ID="tbl_co" CssClass="table" ClientIDMode="Static" style="width:100%;border:2px solid gray">
        <asp:TableHeaderRow TableSection="TableHeader">
            <asp:TableHeaderCell Font-Size="8">Rank</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="8">Analyst</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="8">Confidence</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="8">Return</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="8">Accuracy</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="8">Win</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="8">Relative</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="8">Estimates</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="8">Target</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="8">Upside</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="8">Horizon</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="8">Started</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="8">Reiter</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="8">Rationale</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="8">Perf</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="8">Since</asp:TableHeaderCell>
        </asp:TableHeaderRow>
    </asp:Table>
    <br />
    <h5>Analyst contribution to total consensus</h5>
    <asp:Table runat="server" ID="tbl_today" CssClass="table" ClientIDMode="Static" style="width:100%;border:2px solid gray">
        <asp:TableHeaderRow TableSection="TableHeader">
            <asp:TableHeaderCell Font-Size="9">Analyst</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="9">Broker</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="9">Confidence</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="9">Recency</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="9">Range</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="9">Expiration</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="9">Target</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="9">Flat</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="9">Weighted</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="9">% F</asp:TableHeaderCell>
            <asp:TableHeaderCell Font-Size="9">% W</asp:TableHeaderCell>
        </asp:TableHeaderRow>
    </asp:Table>

    <br />
    <table style="width:100%;border:2px solid gray">
        <tr>
            <td>
                <div id="chart" style="height: 400px; margin: 0 auto"></div>
                <br />
                <div id="chart_changes" style="height: 400px; margin: 0 auto"></div>
                <br />
                <div id="chart_changes_2nd" style="height: 400px; margin: 0 auto"></div>
            </td>
        </tr>
    </table>

    

    <br />
    <h5>Consensus history</h5>
    <asp:Table runat="server" ID="tbl" CssClass="table" ClientIDMode="Static">
        <asp:TableHeaderRow TableSection="TableHeader">
            <asp:TableHeaderCell>Date</asp:TableHeaderCell>
            <asp:TableHeaderCell>Total F</asp:TableHeaderCell>
            <asp:TableHeaderCell>Total W</asp:TableHeaderCell>
            <asp:TableHeaderCell>WS F</asp:TableHeaderCell>
            <asp:TableHeaderCell>WS W</asp:TableHeaderCell>
            <asp:TableHeaderCell>Crowd F</asp:TableHeaderCell>
            <asp:TableHeaderCell>Crowd W</asp:TableHeaderCell>
            <asp:TableHeaderCell>Price</asp:TableHeaderCell>
        </asp:TableHeaderRow>
    </asp:Table>

    <br />
    <h5>Trends</h5>
    <table style="width:100%;border:2px solid gray">
        <tr>
            <td>
                <div id="chart_trends" style="height: 400px; margin: 0 auto"></div>
                <br />
                <div id="chart_trends_2nd" style="height: 400px; margin: 0 auto"></div>
                <br />
                <asp:Table runat="server" ID="tbl_trends" CssClass="table" ClientIDMode="Static">
                    <asp:TableHeaderRow TableSection="TableHeader">
                        <asp:TableHeaderCell>Set</asp:TableHeaderCell>
                        <asp:TableHeaderCell>1w</asp:TableHeaderCell>
                        <asp:TableHeaderCell>1m</asp:TableHeaderCell>
                        <asp:TableHeaderCell>3m</asp:TableHeaderCell>
                        <asp:TableHeaderCell>6m</asp:TableHeaderCell>
                        <asp:TableHeaderCell>12m</asp:TableHeaderCell>
                    </asp:TableHeaderRow>
                    <asp:TableRow>
                        <asp:TableCell runat="server" RowSpan="2" style="vertical-align:middle" Text="Total"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_changes_1w"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_changes_1m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_changes_3m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_changes_6m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_changes_12m"></asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell runat="server" ID="trends_total_changes_2nd_1w"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_changes_2nd_1m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_changes_2nd_3m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_changes_2nd_6m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_changes_2nd_12m"></asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell runat="server" RowSpan="2" style="vertical-align:middle" Text="Total W"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_w_changes_1w"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_w_changes_1m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_w_changes_3m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_w_changes_6m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_w_changes_12m"></asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell runat="server" ID="trends_total_w_changes_2nd_1w"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_w_changes_2nd_1m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_w_changes_2nd_3m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_w_changes_2nd_6m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_total_w_changes_2nd_12m"></asp:TableCell>
                    </asp:TableRow>

                    <asp:TableRow>
                        <asp:TableCell runat="server" RowSpan="2" style="vertical-align:middle" Text="WS"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_changes_1w"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_changes_1m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_changes_3m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_changes_6m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_changes_12m"></asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell runat="server" ID="trends_ws_changes_2nd_1w"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_changes_2nd_1m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_changes_2nd_3m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_changes_2nd_6m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_changes_2nd_12m"></asp:TableCell>
                    </asp:TableRow>

                    <asp:TableRow>
                        <asp:TableCell runat="server" RowSpan="2" style="vertical-align:middle" Text="WS W"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_w_changes_1w"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_w_changes_1m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_w_changes_3m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_w_changes_6m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_w_changes_12m"></asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell runat="server" ID="trends_ws_w_changes_2nd_1w"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_w_changes_2nd_1m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_w_changes_2nd_3m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_w_changes_2nd_6m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_ws_w_changes_2nd_12m"></asp:TableCell>
                    </asp:TableRow>

                    <asp:TableRow>
                        <asp:TableCell runat="server" RowSpan="2" style="vertical-align:middle" Text="Crowd"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_changes_1w"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_changes_1m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_changes_3m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_changes_6m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_changes_12m"></asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell runat="server" ID="trends_crowd_changes_2nd_1w"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_changes_2nd_1m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_changes_2nd_3m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_changes_2nd_6m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_changes_2nd_12m"></asp:TableCell>
                    </asp:TableRow>

                    <asp:TableRow>
                        <asp:TableCell runat="server" RowSpan="2" style="vertical-align:middle" Text="Crowd W"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_w_changes_1w"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_w_changes_1m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_w_changes_3m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_w_changes_6m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_w_changes_12m"></asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell runat="server" ID="trends_crowd_w_changes_2nd_1w"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_w_changes_2nd_1m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_w_changes_2nd_3m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_w_changes_2nd_6m"></asp:TableCell>
                        <asp:TableCell runat="server" ID="trends_crowd_w_changes_2nd_12m"></asp:TableCell>
                    </asp:TableRow>

                </asp:Table>
            </td>
        </tr>
    </table>
    
    <br />
    <h5>Sector</h5>
    <table style="border:2px solid gray;width:100%">
        <tr>
            <td>
                <div id="chart_sector" style="height: 400px; margin: 0 auto"></div>
                <asp:Label runat="server" ID="l_peers"></asp:Label>
            </td>
        </tr>
    </table>

</asp:Content>