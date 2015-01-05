<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Insights.aspx.cs" Inherits="admin_Insights" MasterPageFile="~/admin/MasterPage_Admin.master" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
    <script src="https://code.highcharts.com/highcharts.js"></script>
    <script src="https://code.highcharts.com/modules/exporting.js"></script>
    <script type="text/javascript">
        $(function () {
            $('#div_sectors').highcharts({
                title: {
                    text: ' day stats',
                    x: -20 //center
                },
                tooltip: {
                    shared: true,
                    crosshairs: true
                },
                xAxis: {
                    categories: timeline
                },
                yAxis: {
                    title: {
                        text: 'Sectors'
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
                    name: 'Sectors',
                    data: sector
                },
                {
                    name: 'Industry',
                    data: industry
                },
                {
                    name: 'Bench. Ind',
                    data: benchmark_industry
                }

                ]
            });
        });
    </script>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="body">
    <asp:Label runat="server" ID="high_level"></asp:Label>

    <div id="div_sectors" style="min-width: 310px; height: 400px; margin: 0 auto"></div>

    <asp:Label runat="server" ID="comps"></asp:Label>
</asp:Content>