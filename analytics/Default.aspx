<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="analytics_Default" MasterPageFile="~/MasterPage_Root.master" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
    <!-- HighCharts -->
    <script src="<%=Page.ResolveUrl("~")%>js/highcharts.js"></script>

    <script type="text/javascript">
        var target = 100;

        $(document).ready(function () {
            x();
            area();
        });

        function x() {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "<%= Page.ResolveUrl("~/")%>Portfolio.asmx/stats_for_analytics_page",
            dataType: "json",
            data: "",
            success: function (data) {
                //$("#" + pre + "profit").html(color_code(data.d.profit, true));
                //$("#" + pre + "benchmark").html(color_code(data.d.profit - data.d.benchmark, true));
                //$("#" + pre + "deployed").text((100 * data.d.deployed).toFixed(0) + "%");
                pie(data.d.types, "div_types");

                $("#registered").text(data.d.users);
                $("#verified").text(data.d.verified);
                $("#verified_pct").text((100 * data.d.verified / data.d.users).toFixed(1) + "%");
                $("#investor").text(data.d.investors);
                $("#investor_pct").text((100 * data.d.investors / data.d.users).toFixed(1) + "%");
                $("#target").text((data.d.names_power.length + data.d.names_intermediate.length));
                $("#target_pct").text(((100 * data.d.names_power.length + data.d.names_intermediate.length) / data.d.users).toFixed(1) + "%");
                $("#invested_target").attr("style", "width:" + (100 * (data.d.names_power.length + data.d.names_intermediate.length) / target).toFixed(0) + "%");
                $("#all").html("follows:" + data.d.follows[0] + ", " + data.d.follows[1] + ", " + data.d.follows[2] + ", " + "<br>" + "positions: " + data.d.positions[0] + ", " + data.d.positions[1] + ", " + data.d.positions[2] + "<br>" + "positions all: " + data.d.positions_all[0] + ", " + data.d.positions_all[1] + ", " + data.d.positions_all[2] + "<br>" + "follow co: " + data.d.follows_company_sector[0] + ", " + data.d.follows_company_sector[1] + ", " + data.d.follows_company_sector[2] + "<br>" + "page visits: " + data.d.page_visits[0] + ", " + data.d.page_visits[1] + ", " + data.d.page_visits[2] + "<br>" + "# visits last week: " + data.d.visits_per_week[0] + ", " + data.d.visits_per_week[1] + ", " + data.d.visits_per_week[2] + "<br>" + "last visit: " + data.d.last_visit[0].toFixed(0) + ", " + data.d.last_visit[1].toFixed(0) + ", " + data.d.last_visit[2].toFixed(0) + "<br>" + "email: " + data.d.email[0] + ", " + data.d.email[1] + ", " + data.d.email[2] + "<br>" + "weekly: " + data.d.weekly[0] + ", " + data.d.weekly[1] + ", " + data.d.weekly[2]);

                var names = "";
                for (var i=0;i<data.d.names_power.length;i++){
                    names += data.d.names_power[i] + ", ";
                }
                $("#names_power").attr("title",names);
                names = "";
                for (var i = 0; i < data.d.names_intermediate.length; i++) {
                    names += data.d.names_intermediate[i] + ", ";
                }
                $("#names_intermediate").attr("title", names);
                names = "";
                for (var i = 0; i < data.d.names_lazy.length; i++) {
                    names += data.d.names_lazy[i] + ", ";
                }
                $("#names_lazy").attr("title", names);
            }
            });
        }

        function area() {
                $('#user_acquisition').highcharts({
                    chart: {
                        type: 'area'
                    },
                    title: {
                        text: null
                    },
                    subtitle: {
                        text: null
                    },
                    xAxis: {
                        categories: ['1750', '1800', '1850', '1900', '1950', '1999', '2050'],
                        tickmarkPlacement: 'on',
                        title: {
                            enabled: false
                        }
                    },
                    yAxis: {
                        title: {
                            text: null
                        },
                        labels: {
                            formatter: function () {
                                return this.value / 1000;
                            }
                        }
                    },
                    tooltip: {
                        shared: true,
                        valueSuffix: ' millions'
                    },
                    plotOptions: {
                        area: {
                            stacking: 'normal',
                            lineColor: '#666666',
                            lineWidth: 1,
                            marker: {
                                lineWidth: 1,
                                lineColor: '#666666'
                            }
                        }
                    },
                    series: [{
                        name: 'Asia',
                        data: [502, 635, 809, 947, 1402, 3634, 5268]
                    }, {
                        name: 'Africa',
                        data: [106, 107, 111, 133, 221, 767, 1766]
                    }, {
                        name: 'Europe',
                        data: [163, 203, 276, 408, 547, 729, 628]
                    }, {
                        name: 'America',
                        data: [18, 31, 54, 156, 339, 818, 1201]
                    }, {
                        name: 'Oceania',
                        data: [2, 2, 2, 6, 13, 30, 46]
                    }]
                });
        }

        function pie(data,div) {
            new Highcharts.Chart({
                chart: {
                    renderTo: div,
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
                colors: [
                    "silver",
                    "#62c462",
                    "green",
                ],
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
    </script>


</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="body">
    <div class="row">
        <div class="span4">
            <h4>Users</h4>
            <div class="thumbnail" style="background-color:white">
                
                <table border="0" class="table table-striped">
                    <tr>
                        <td style="border-top:0">Registered</td>
                        <td style="border-top:0">
                            <span id="registered"></span>
                        </td>
                        <td style="border-top:0">

                        </td>
                    </tr>
                    <tr>
                        <td>Verified</td>
                        <td>
                            <span id="verified"></span>
                        </td>
                        <td>
                            <span id="verified_pct"></span>
                        </td>
                    </tr>
                    <tr>
                        <td>Invested</td>
                        <td>
                            <span id="investor"></span>
                        </td>
                        <td>
                            <span id="investor_pct"></span>
                        </td>
                    </tr>
                    <tr>
                        <td>Target</td>
                        <td>
                            <span id="target"></span>
                        </td>
                        <td>
                            <span id="target_pct"></span>
                        </td>
                    </tr>
                </table>
                
                <div class="progress" style="margin-bottom:0">
                    <div class="bar" id="invested_target"></div>
                </div>
            </div>
        </div>

        <div class="span4">
            <h4>Investor Progress</h4>
            <div class="thumbnail" style="background-color:white">
                <div id="div_types"></div>

                Deployed <i class="icon-info-sign" style="color:gray" id="names_power"></i>
                Middle <i class="icon-info-sign" style="color:gray" id="names_intermediate"></i>
                Started <i class="icon-info-sign" style="color:gray" id="names_lazy"></i>
            </div>
        </div>

        <div class="span4">
            <h4>Engagement metrics</h4>
            <div class="thumbnail" style="background-color:white">
                <span id="all"></span>
            </div>
        </div>

    </div>

    <div class="row">
        <div class="span6">
            <h4>User acquisition</h4>
            <div class="thumbnail" style="background-color:white">
                <div style="width:100%" id="user_acquisition"></div>
            </div>
        </div>
    </div>
</asp:Content>