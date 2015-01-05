<%@ Page Language="C#" AutoEventWireup="true" CodeFile="engagement.aspx.cs" Inherits="admin_engagement" MasterPageFile="~/admin/MasterPage_Admin.master" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
    <script src="https://code.highcharts.com/highcharts.js"></script>
    <script src="https://code.highcharts.com/modules/exporting.js"></script>
    <script src="https://rawgithub.com/highslide-software/export-csv/master/export-csv.js"></script>
    <script type="text/javascript">
        var span = <%=span%>;
        var offset = <%=offset%>;
        var cumulativeChart;
        $(function () {
            $('#div_first').highcharts({
                title: {
                    text: span + ' day stats',
                    x: -20 //center
                },
                tooltip: {
                    shared: true,
                    crosshairs:true
                },
                xAxis: {
                    categories: categories
                },
                yAxis: {
                    title: {
                        text: 'Estimates'
                    },
                    plotLines: [{
                        value: 0,
                        width: 1,
                        color: '#808080'
                    }]
                },
                legend: {
                    layout: 'vertical',
                    align: 'right',
                    verticalAlign: 'middle',
                    borderWidth: 0
                },
                series: [{
                    name: 'Estimates',
                    data: estimates
                }, {
                    name: 'Verified',
                    data: estimates_ver
                }, {
                    name: 'Users',
                    data: users
                }, {
                    name: 'Users (ver)',
                    data: users_ver
                }, {
                    name: 'Positions',
                    data: positions
                }]
            });

            cumulativeChart = $('#container_cumulative').highcharts({
                title: {
                    text: span + ' day cumulative user stats',
                    x: -20 //center
                },
                chart: {
                    type: 'area'
                },
                tooltip: {
                    shared: true,
                    crosshairs: true
                },
                xAxis: {
                    categories: categories
                },
                yAxis: {
                    title: {
                        text: 'Users'
                    },
                    plotLines: [{
                        value: 0,
                        width: 1,
                        color: '#808080'
                    }]
                },
                legend: {
                    layout: 'vertical',
                    align: 'right',
                    verticalAlign: 'middle',
                    borderWidth: 0
                },
                series: [{
                    name: 'Users',
                    data: cum_users
                }, {
                    name: 'Verified',
                    data: cum_users_ver
                }]
            });

            $('#container_actions').highcharts({
                title: {
                    text: span + ' day cumulative actions',
                    x: -20 //center
                },
                tooltip: {
                    shared: true,
                    crosshairs: true
                },
                xAxis: {
                    categories: categories
                },
                yAxis: {
                    title: {
                        text: 'Users'
                    },
                    plotLines: [{
                        value: 0,
                        width: 1,
                        color: '#808080'
                    }]
                },
                legend: {
                    layout: 'vertical',
                    align: 'right',
                    verticalAlign: 'middle',
                    borderWidth: 0
                },
                series: [{
                    name: 'Estimates',
                    data: cum_estimates
                }, {
                    name: 'Estimates (ver)',
                    data: cum_estimates_ver
                },
                {
                    name: 'Positions',
                    data: cum_positions
                }
                ]
            });

            $('#engagement').highcharts({
                chart: {
                    type: 'column'
                },
                title: {
                    text: 'Engagement'
                },
                xAxis: {
                    categories: ['1', '2', '3-5', '6-10', '11-20','20+'],
                    title:{
                        text:'No of total estimates'
                    }
                },
                yAxis: {
                    min: 0,
                    title: {
                        text: 'Users'
                    },
                    stackLabels: {
                        enabled: true,
                        style: {
                            fontWeight: 'bold',
                            color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                        }
                    }
                },
                legend: {
                    align: 'right',
                    x: -70,
                    verticalAlign: 'top',
                    y: 20,
                    floating: true,
                    backgroundColor: (Highcharts.theme && Highcharts.theme.background2) || 'white',
                    borderColor: '#CCC',
                    borderWidth: 1,
                    shadow: false
                },
                tooltip: {
                    formatter: function() {
                        return '<b>'+ this.x +'</b><br/>'+
                            this.series.name +': '+ this.y +'<br/>'+
                            'Total: '+ this.point.stackTotal;
                    }
                },
                plotOptions: {
                    column: {
                        stacking: 'normal',
                        dataLabels: {
                            enabled: true,
                            color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'white',
                            style: {
                                textShadow: '0 0 3px black, 0 0 3px black'
                            }
                        }
                    }
                },
                series: [{
                    name: 'Verified',
                    data: engagement_verified
                }, {
                    name: 'Not verified',
                    data: engagement_not_verified
                }]
            });

        });

        cumulativeChart.getOptions().exporting.buttons.contextButton.menuItems.push({
            text: 'Download CSV',
            onclick: function () {
                Highcharts.post('http://www.highcharts.com/studies/csv-export/csv.php', {
                    csv: this.getCSV()
                });
            }
        });

   
    </script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="body">
    <div class="thumbnail">
        <div id="div_first" style="min-width: 310px; height: 400px; margin: 0 auto"></div>
    </div>
    <div style="height:20px"></div>
    <div class="thumbnail">
        <div id="container_cumulative" style="min-width: 310px; height: 400px; margin: 0 auto"></div>
        
    </div>
    <div style="height:20px"></div>
    <div class="thumbnail">
        <div id="container_actions" style="min-width: 310px; height: 400px; margin: 0 auto"></div>
    </div>
    <div style="height:20px"></div>
    <div class="thumbnail">
        <div id="engagement" style="min-width: 310px; height: 400px; margin: 0 auto"></div>
    </div>
    <div style="height:20px"></div>
    <asp:Label runat="server" ID="ticker_frequency" style="color:blue"></asp:Label>
    <div style="height:20px"></div>
    <asp:Label runat="server" ID="output"></asp:Label>
</asp:Content>