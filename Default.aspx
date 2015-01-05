<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="Default2" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">

    <script>(function () {
    var _fbq = window._fbq || (window._fbq = []);
    if (!_fbq.loaded) {
        var fbds = document.createElement('script');
        fbds.async = true;
        fbds.src = '//connect.facebook.net/en_US/fbds.js';
        var s = document.getElementsByTagName('script')[0];
        s.parentNode.insertBefore(fbds, s);
        _fbq.loaded = true;
    }
    _fbq.push(['addPixelId', '313928355436234']);
})();
        window._fbq = window._fbq || [];
        window._fbq.push(['track', 'PixelInitialized', {}]);
</script>
<noscript><img height="1" width="1" border="0" alt="" style="display:none" src="https://www.facebook.com/tr?id=313928355436234&amp;ev=NoScript" /></noscript>
    
     <meta name="google-site-verification" content="NeksfKxU6bqRofT9w0KgbMJ_ff66MGd25oFSnQmupC0" />
    <meta property="og:url" content="https://www.invesd.com/"/>
    <meta property="og:title" content="Can you beat Wall St?"/>
    <meta property="og:type" content="video"/>
    <meta property="og:image" content="https://invesd.com/images/invesd_logo_full_square.png"/>
    <meta property="og:description" content="Are you holding the best stocks? Ask a community of accurate analysts and real investors"/>
    <meta property="og:video" content="http://www.youtube.com/v/B2blfbjNlMU?autohide=1&amp;version=3"/>

    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no" />
    <link rel="shortcut icon" href="<%=Page.ResolveUrl("~") %>invesd.ico" />
    <meta property="og:image" content="<%= Page.ResolveUrl("~/")%>images/invesd_logo_square.png"/>

    <!-- Bootstrap & Font Awesome -->
    <link rel="stylesheet" href="//netdna.bootstrapcdn.com/bootstrap/3.1.1/css/bootstrap.min.css" />
    <script src="//netdna.bootstrapcdn.com/bootstrap/3.1.1/js/bootstrap.min.js"></script>
    <link href="//netdna.bootstrapcdn.com/font-awesome/4.0.3/css/font-awesome.css" rel="stylesheet" />
    <script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js"></script>

    <!-- JQuery Sparkline -->
    <script src="<%=Page.ResolveUrl("~") %>js/jquery.sparkline.min.js" type="text/javascript"></script>

    <!-- Google Charts -->
    <script type="text/javascript" src="//www.google.com/jsapi"></script>

    <!-- JQuery Knob -->
    <script src="<%= Page.ResolveUrl("~/")%>js/jquery.knob.js" type="text/javascript"></script>

    <style type="text/css">
        .margin_large{
            margin-top:30px;margin-bottom:30px;
        }
        .margin_small{
            margin-top:15px;margin-bottom:15px;
        }

        .footer_links{
            color:gray;
        }
        .footer_links:hover{
            text-decoration:none;
            color:silver;
        }

        .arrow_box {
	        position: relative;
	        background: #ffffff;

        }
        .arrow_box:after, .arrow_box:before {
	        right: 100%;
	        top: 50%;
	        border: solid transparent;
	        content: " ";
	        height: 0;
	        width: 0;
	        position: absolute;
	        pointer-events: none;
        }

        .arrow_box:after {
	        border-color: rgba(255, 255, 255, 0);
	        border-right-color: #ffffff;
	        border-width: 30px;
	        margin-top: -30px;
        }
        .arrow_box:before {
	        border-color: rgba(255, 255, 255, 0);
	        border-right-color: #eeeeee;
	        border-width: 33px;
	        margin-top: -33px;
        }

    </style>

    <script type="text/javascript">
        // google analytics
        var _gaq = _gaq || [];
        _gaq.push(['_setAccount', 'UA-39326079-1']);
        _gaq.push(['_trackPageview']);
        (function () {
            var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
            ga.src = ('https:' == document.location.protocol ? 'https://' : 'http://') + 'stats.g.doubleclick.net/dc.js';
            var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
        })();

        //google.load('visualization', '1', { packages: ['corechart'] });
        //google.setOnLoadCallback(drawVisualization);
        //google.setOnLoadCallback(drawVisualization2);
        //google.setOnLoadCallback(drawVisualization3);

        $(document).ready(function () {
            //$("#SecA").show();

            //var AB = parseInt(getParameterByName("des"));
            //if (AB == 1) {
                
            //    $("#SecB").hide();
            //}
            //else {
            //    $("#SecA").hide();
            //    $("#SecB").show();
            //}

            //$("#portfolio_price").knob();
            //$("#portfolio_time").knob();
            //$("#portfolio_price2").knob();
            //$("#portfolio_time2").knob();
            //$("#portfolio_price3").knob();
            //$("#portfolio_time3").knob();
            
            //$("#sparkline1").sparkline([553.13, 540.98, 543.93, 540.04, 543.46, 536.47, 532.94, 535.73, 546.39, 557.37, 554.25, 540.6, 549.07, 551.51, 556.18, 546.07, 550.5, 506.5, 500.57, 499.44, 500.6, 501.53, 508.79, 512.59, 512.51, 519.95, 528.99, 535.96, 535.92, 544.43, 543.99, 545.99, 537.37, 531.15, 525.25, 527.55], {
            //    type: 'line',
            //    width: '100%',
            //    height: '70',
            //    lineColor: '#ffffff',
            //    fillColor: 'transparent',
            //    lineWidth:3,
            //    minSpotColor: undefined,
            //    maxSpotColor: undefined,
            //    highlightSpotColor: undefined,
            //    highlightLineColor: undefined,
            //    spotRadius: 0,
            //    disableTooltips: true,
            //    disableHighlight: true,
            //    chartRangeMax:600
            //});

            //$("#sparkline2").sparkline([150.1, 149.56, 147, 149.36, 151.28, 147.53, 145.72, 139.34, 161.25, 164.13, 170.97, 170.01, 176.68, 178.56, 181.5, 174.6, 169.62, 178.38, 175.23, 182.84, 181.41, 177.11, 178.73, 174.42, 178.38, 186.53, 196.56, 196.62, 195.32, 199.63, 198.23, 203.7, 193.64, 209.97, 209.64, 217.65], {
            //    type: 'line',
            //    width: '100%',
            //    height: '70',
            //    lineColor: '#ffffff',
            //    fillColor: 'transparent',
            //    lineWidth: 3,
            //    minSpotColor: undefined,
            //    maxSpotColor: undefined,
            //    highlightSpotColor: undefined,
            //    highlightLineColor: undefined,
            //    spotRadius: 0,
            //    disableTooltips: true,
            //    disableHighlight: true,
            //    chartRangeMax: 240
            //});

            //$("#sparkline3").sparkline([77.17, 76.96, 76.17, 77.28, 78.03, 77.6, 77.67, 75.12, 75.46, 76.19, 75.29, 74.9, 73.65, 73.6, 73.39, 74.98, 74.21, 73.89, 71.56, 71.91, 71.12, 68.97, 70.65, 70.49, 72.36, 74.06, 74.8, 74.5, 73.91, 74.69, 75.03, 73.97, 73.32, 73.55, 72.56, 72.56], {
            //    type: 'line',
            //    width: '100%',
            //    height: '70',
            //    lineColor: '#ffffff',
            //    fillColor: 'transparent',
            //    lineWidth: 3,
            //    minSpotColor: undefined,
            //    maxSpotColor: undefined,
            //    highlightSpotColor: undefined,
            //    highlightLineColor: undefined,
            //    spotRadius: 0,
            //    disableTooltips: true,
            //    disableHighlight: true,
            //    chartRangeMax: 90
            //});
            
        });

        function getParameterByName(name) {
            name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
            var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
                results = regex.exec(location.search);
            return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
        }

    </script>
    <title>Can you beat Wall St? | Invesd</title>
</head>
<body itemscope itemtype="http://schema.org/Product">
    <form id="form1" runat="server">
        <section id="SecA" style="background-image:url('<%=Page.ResolveUrl("~") %>images/header_background.jpg');background-color:#000000">
            <div class="container">
                <div class="row margin_small">
                    <div class="col-md-12 text-right">
                        <a href="Login.aspx" class="btn btn-info btn-lg">&nbsp;&nbsp;Login&nbsp;&nbsp;</a>
                    </div>
                </div>
            </div>

            <div class="container">
                <div class="row margin_large">
                    <div class="col-md-2"></div>
                    <div class="col-md-8 text-center" style="color:white">
                        <img src="images/invesd_logo.png" style="height:35px" />
                        <h2>Can you beat Wall St?</h2>
                        <h4 style="color:#39b3d7">Build your track record as an accurate investor</h4>
                        <h4 style="color:#47a447">Access positions and estimates of other analysts and investors</h4>
                        <br />
                        <a href="Signup.aspx" class="btn btn-success btn-lg">Signup for FREE</a>
                    </div>
                    <div class="col-md-2"></div>
                </div>
            </div>

            <div class="container">
                <div class="row margin_large">
                    <div class="col-md-2"></div>
                    <div class="col-md-8">
                        <a href="<%=Page.ResolveUrl("~") %>Company.aspx?ticker=googl">
                            <img src="images/landing_01.png" style="width:100%" class="img-rounded" />
                        </a>
                    </div>
                    <div class="col-md-2"></div>
                </div>
            </div>

            <div class="container">
                <div class="row margin_large">
                    <div class="col-md-2"></div>
                    <div class="col-md-8">
                        <table style="width:100%" class="table">
                            <tr>
                                <td style="width:10%">
                                    <a href="Company.aspx?ticker=aapl">
                                        <img src="images/logo/aapl.png" class="img-thumbnail" style="width:50px" />
                                    </a>
                                </td>
                                <td style="width:40%">
                                    <a href="Company.aspx?ticker=aapl" class="btn btn-info btn-lg">Estimate on AAPL</a>
                                </td>
                                <td style="width:10%">
                                    <a href="Company.aspx?ticker=xom">
                                        <img src="images/logo/xom.png" class="img-thumbnail" style="width:50px" />
                                    </a>
                                </td>
                                <td style="width:40%">
                                    <a href="Company.aspx?ticker=xom" class="btn btn-info btn-lg">Estimate on XOM</a>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <a href="Company.aspx?ticker=pcln">
                                        <img src="images/logo/pcln.png" class="img-thumbnail" style="width:50px" />
                                    </a>
                                </td>
                                <td>
                                    <a href="Company.aspx?ticker=pcln" class="btn btn-info btn-lg">Estimate on PCLN</a>
                                </td>
                                <td>
                                    <a href="Company.aspx?ticker=fb">
                                        <img src="images/logo/fb.png" class="img-thumbnail" style="width:50px" />
                                    </a>
                                </td>
                                <td>
                                    <a href="Company.aspx?ticker=fb" class="btn btn-info btn-lg">Estimate on FB</a>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <a href="Company.aspx?ticker=vz">
                                        <img src="images/logo/vz.png" class="img-thumbnail" style="width:50px" />
                                    </a>
                                </td>
                                <td>
                                    <a href="Company.aspx?ticker=vz" class="btn btn-info btn-lg">Estimate on VZ</a>
                                </td>
                                <td>
                                    <a href="Company.aspx?ticker=v">
                                        <img src="images/logo/v.png" class="img-thumbnail" style="width:50px" />
                                    </a>
                                </td>
                                <td>
                                    <a href="Company.aspx?ticker=v" class="btn btn-info btn-lg">Estimate on V</a>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div class="col-md-2"></div>
                </div>
            </div>

            <div class="container">
                <div class="row margin_large">
                    <div class="col-md-2"></div>
                    <div class="col-md-4">
                        <a href="Signup.aspx" class="btn btn-success btn-lg btn-block" style="margin-bottom:10px">Signup for FREE</a>
                    </div>
                    <div class="col-md-4">
                        <a href="Company.aspx?ticker=googl" class="btn btn-info btn-lg btn-block">Discover more</a>
                    </div>
                    <div class="col-md-2"></div>
                </div>
            </div>

            
        </section>

        <%--<section style="background-color:#39b3d7">
            <div class="container">
                <div class="row margin_large">
                    <div class="col-md-6 text-left">
                        <h2 style="color:white">A fresh perspective on your stocks</h2>
                        <p itemprop="description" style="color:white">How much upside are analysts and investors expecting? How well have they performed in the past? Quickly find these answers and see the big picture before investing.</p>
                    </div>
                    <div class="col-md-6 text-left">
                        <table border="0" style="border:0;margin:auto">
                            <tr style="background-color:rgba(255, 255, 255, 0.5)">
                                <td style="width:10%;border-top-left-radius:10px">
                                    <img src="images/logo/aapl.png" class="img-thumbnail" />
                                </td>
                                <td style="width:60%">
                                    <span id="sparkline1"></span>
                                </td>
                                <td style="width:30%;border-top-right-radius:10px">
                                    <div id="gchart1" style="width:100%;height:70px"></div>
                                </td>
                            </tr>
                            <tr style="background-color:rgba(255, 255, 255, 0.2)">
                                <td>
                                    <img src="images/logo/tsla.png" class="img-thumbnail" />
                                </td>
                                <td>
                                    <span id="sparkline2"></span>
                                </td>
                                <td>
                                    <div id="gchart2" style="width:100%;height:70px"></div>
                                </td>
                            </tr>
                            <tr style="background-color:rgba(255, 255, 255, 0.5)">
                                <td style="border-bottom-left-radius:10px">
                                    <img src="images/logo/sbux.png" class="img-thumbnail" />
                                </td>
                                <td>
                                    <span id="sparkline3"></span>
                                </td>
                                <td style="border-bottom-right-radius:10px">
                                    <div id="gchart3" style="width:100%;height:70px"></div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
            </div>
        </section>--%>
        
        <%--<section>
            <div class="container">
                <div class="row margin_large">
                    <div class="col-md-6 text-center">
                        <table border="0" style="border:0;width:100%">
                            <tr>
                                <td style="width:30%" class="text-left">
                                    <div class="thumbnail text-center" style="margin-bottom:0">
                                        <table class="table table-condensed" style="margin-bottom:0">
                                            <tr>
                                                <td style="border-top:0">
                                                    <img src="images/user/2673166.png" class="img-circle" style="width:50px" />
                                                    <br />
                                                    <strong>Nikoo</strong>
                                                </td>
                                            </tr>
                                            <tr class="active">
                                                <td>
                                                    <img src="images/logo/lnkd.png" style="width:30px" /> <img src="images/logo/mdt.png" style="width:30px" />
                                                    <br />
                                                    <span style="color:gray;font-size:x-small">INVESTMENTS</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <i class="fa fa-medkit fa-2x"></i> <i class="fa fa-laptop fa-2x"></i>
                                                    <br />
                                                    <span style="color:gray;font-size:x-small">SECTORS</span>
                                                </td>
                                            </tr>
                                            <tr class="active">
                                                <td>
                                                    <img src="images/user/2646082.png" style="width:30px" class="img-circle" /> <img src="images/user/2644801.png" style="width:30px" class="img-circle" />
                                                    <br />
                                                    <span style="color:gray;font-size:x-small">FOLLOWING</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                </td>
                                <td style="width:2%"></td>
                                <td style="width:68%">
                                    <div class="thumbnail arrow_box" style="margin-bottom:0">
                                        <table style="border:0;margin-bottom:0" class="table table-condensed">
                                            <tr>
                                                <td style="width:34%;border-top:0">
                                                    <img src="images/logo/googl.png" style="width:50px;height:50px" />
                                                </td>
                                                <td style="width:33%;border-top:0;vertical-align:middle">
                                                    <span style="font-size:large;color:#62c462">28%</span>
                                                    <br />
                                                    <span style="color:gray;font-size:x-small">UPSIDE</span>
                                                </td>
                                                <td style="width:33%;border-top:0;vertical-align:middle">
                                                    <img src="images/signal4.png" />
                                                    <br />
                                                    <span style="color:gray;font-size:x-small">COVERAGE</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="vertical-align:middle">
                                                    <img src="images/logo/hlf.png" style="width:50px;height:50px" />
                                                </td>
                                                <td style="vertical-align:middle">
                                                    <span style="font-size:large;color:#62c462">12%</span>
                                                    <br />
                                                    <span style="color:gray;font-size:x-small">UPSIDE</span>
                                                </td>
                                                <td style="vertical-align:middle">
                                                    <img src="images/signal2.png" />
                                                    <br />
                                                    <span style="color:gray;font-size:x-small">COVERAGE</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="vertical-align:middle">
                                                    <img src="images/logo/fb.png" style="width:50px;height:50px" />
                                                </td>
                                                <td style="vertical-align:middle">
                                                    <span style="font-size:large;color:#62c462">10%</span>
                                                    <br />
                                                    <span style="color:gray;font-size:x-small">UPSIDE</span>
                                                </td>
                                                <td style="vertical-align:middle">
                                                    <img src="images/signal4.png" />
                                                    <br />
                                                    <span style="color:gray;font-size:x-small">COVERAGE</span>
                                                </td>
                                            </tr>
                                            <tr class="success">
                                                <td colspan="3" style="border-top:0">
                                                    <strong>SELECTED FOR NIKOO</strong>
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                </td>
                            </tr>
                    </table>
                    </div>
                    <div class="col-md-6 text-left">
                        <h2>Personalized investment discovery</h2>
                        <p>Invesd learns your investment style and automatically notifies you on the latest developments and relevant investment opportunities.</p>
                    </div>
                </div>
            </div>
        </section>--%>

        <%--<section style="background-color:#47a447">
            <div class="container">
                <div class="row margin_large">
                    <div class="col-md-6">
                        <h2 style="color:white">Worry-free portfolio tracking</h2>
                        <p style="color:white">It is hard to keep up with the market. Invesd notifies you on the progress of your positions and overall portfolio as well as significant changes in price or analyst consensus.</p>
                    </div>
                    <div class="col-md-6 text-center">
                        <table style="border:0;margin-bottom:0" class="table table-condensed">
                            <tr style="background-color:rgba(255, 255, 255, 1)">
                                <td style="width:25%;vertical-align:middle;border-top:0;border-top-left-radius:10px">
                                    <img src="images/logo/amzn.png" style="width:50px" />
                                </td>
                                <td style="width:25%;vertical-align:middle;border-top:0">
                                    10
                                    <br />
                                    <span style="color:gray;font-size:x-small">SHARES</span>
                                </td>
                                <td style="width:25%;vertical-align:middle;border-top:0">
                                    <span style="color:#62c462;">$989</span>
                                    <br />
                                    <span style="color:gray;font-size:x-small">PROFIT</span>
                                </td>
                                <td style="width:25%;vertical-align:middle;border-top:0;border-top-right-radius:10px">
                                    <div style="width: 60px; height: 60px;margin-right:auto;margin-left:auto;vertical-align:middle;position:relative">
                                        <div style="position:absolute;left:0px;top:0px">
                                            <input id="portfolio_price" data-skin="tron" data-fgColor="#62c462" data-displayInput="false" data-width="60" data-height="60" data-thickness=".2 " data-min="0" data-max="100" data-readOnly="true" value="92" />
                                        </div>
                                        <div style="position:absolute;left:12px;top:12px">
                                            <input id="portfolio_time" data-min="0" data-fgColor="#5B5A5A" data-skin="tron" data-max="100" data-width="36" data-height="36" data-thickness=".25" data-readOnly="true" data-displayInput="false" value="20"/>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                            <tr style="background-color:rgba(255, 255, 255, 0.9)">
                                <td style="width:15%;vertical-align:middle;border-top:0">
                                    <img src="images/logo/qcom.png" style="width:50px"/>
                                </td>
                                <td style="width:15%;vertical-align:middle;border-top:0">
                                    100
                                    <br />
                                    <span style="color:gray;font-size:x-small">SHARES</span>
                                </td>
                                <td style="width:15%;vertical-align:middle;border-top:0">
                                    <span style="color:#62c462;">$1,080</span>
                                    <br />
                                    <span style="color:gray;font-size:x-small">PROFIT</span>
                                </td>
                                <td style="width:20%;vertical-align:middle;border-top:0">
                                    <div style="width: 60px; height: 60px;margin-right:auto;margin-left:auto;vertical-align:middle;position:relative">
                                        <div style="position:absolute;left:0px;top:0px">
                                            <input id="portfolio_price2" data-skin="tron" data-fgColor="#62c462" data-displayInput="false" data-width="60" data-height="60" data-thickness=".2 " data-min="0" data-max="100" data-readOnly="true" value="32" />
                                        </div>
                                        <div style="position:absolute;left:12px;top:12px">
                                            <input id="portfolio_time2" data-min="0" data-fgColor="#5B5A5A" data-skin="tron" data-max="100" data-width="36" data-height="36" data-thickness=".25" data-readOnly="true" data-displayInput="false" value="70"/>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                            <tr style="background-color:rgba(255, 255, 255, 1)">
                                <td style="width:15%;vertical-align:middle;border-top:0;border-bottom-left-radius:10px;">
                                    <img src="images/logo/ebay.png" style="width:50px"/>
                                </td>
                                <td style="width:15%;vertical-align:middle;border-top:0">
                                    25
                                    <br />
                                    <span style="color:gray;font-size:x-small">SHARES</span>
                                </td>
                                <td style="width:15%;vertical-align:middle;border-top:0">
                                    <span style="color:#ee5f5b;">$319</span>
                                    <br />
                                    <span style="color:gray;font-size:x-small">PROFIT</span>
                                </td>
                                <td style="width:20%;vertical-align:middle;border-top:0;border-bottom-right-radius:10px">
                                    <div style="width: 60px; height: 60px;margin-right:auto;margin-left:auto;vertical-align:middle;position:relative">
                                        <div style="position:absolute;left:0px;top:0px">
                                            <input id="portfolio_price3" data-skin="tron" data-fgColor="#ee5f5b" data-displayInput="false" data-width="60" data-height="60" data-thickness=".2 " data-min="0" data-max="100" data-readOnly="true" value="10" data-reverse="true" />
                                        </div>
                                        <div style="position:absolute;left:12px;top:12px">
                                            <input id="portfolio_time3" data-min="0" data-fgColor="#5B5A5A" data-skin="tron" data-max="100" data-width="36" data-height="36" data-thickness=".25" data-readOnly="true" data-displayInput="false" value="15"/>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
            </div>
        </section>--%>

        <%--<section>
            <div class="container">
                <div class="row margin_large">
                <div class="col-md-6">
                    <div class="thumbnail" style="margin-bottom:0">
                    <table class="table" style="width:100%;margin-bottom:0">
                        <tbody>
                            <tr>
                                <td style="border-top:0;width:15%;vertical-align:middle">
                                    <img src="images/user/2646082.png" style="height:50px" class="img-circle" />
                                </td>
                                <td style="border-top:0;width:45%;vertical-align:middle">
                                    <strong>Mehdi Saedi</strong>
                                    <br />
                                    <span style="color:gray;font-size:small">Member for 4 months</span>
                                </td>
                                <td style="border-top:0;width:20%;vertical-align:middle" class="text-center">
                                    <span style="color:#62c462;"><strong>$26,070</strong></span>
                                    <br />
                                    <span style="color:gray;font-size:x-small">PROFIT</span>
                                </td>
                                <td style="border-top:0;width:20%;vertical-align:middle" class="text-center">
                                    <span style="color:#62c462"><strong>$23,135</strong></span>
                                    <br />
                                    <span style="color:gray;font-size:x-small">VS. S&P</span>
                                </td>
                            </tr>
                            <tr>
                                <td style="vertical-align:middle">
                                    <img src="images/user/1.png" style="height:50px" class="img-circle" />
                                </td>
                                <td style="vertical-align:middle">
                                    <strong>Mehrad Tavakoli</strong>
                                    <br />
                                    <span style="color:gray;font-size:small">Member for 4 months</span>
                                </td>
                                <td class="text-center" style="vertical-align:middle">
                                    <span style="color:#62c462;"><strong>$21,534</strong></span>
                                    <br />
                                    <span style="color:gray;font-size:x-small">PROFIT</span>
                                </td>
                                <td class="text-center" style="vertical-align:middle">
                                    <span style="color:#62c462;"><strong>$17,618</strong></span>
                                    <br />
                                    <span style="color:gray;font-size:x-small">VS. S&P</span>
                                </td>
                            </tr>
                            <tr>
                                <td style="vertical-align:middle">
                                    <img src="images/user/2649559.png" style="height:50px" class="img-circle" />
                                </td>
                                <td style="vertical-align:middle">
                                    <strong>Parmis Tabar</strong>
                                    <br />
                                    <span style="color:gray;font-size:small">Member for 3 months</span>
                                </td>
                                <td class="text-center" style="vertical-align:middle">
                                    <span style="color:#62c462;"><strong>$6,311</strong></span>
                                    <br />
                                    <span style="color:gray;font-size:x-small">PROFIT</span>
                                </td>
                                <td class="text-center" style="vertical-align:middle">
                                    <span style="color:#62c462;"><strong>$5,067</strong></span>
                                    <br />
                                    <span style="color:gray;font-size:x-small">VS. S&P</span>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    </div>
                </div>
                <div class="col-md-6">
                    <h2>$100,000 investment challenge</h2>
                    <p>You get $100k in virtual funds to try it out risk-free. See how other investors are outperforming the market. What do you have to lose? Other than outsized gains of course!</p>
                </div>
            </div>
            </div>
        </section>--%>

        <section style="background-color:#1b1b1b">
            <div class="container">
                <div class="row margin_small">
                    <div class="col-md-10 text-left">
                        <a href="Home.aspx" class="footer_links">Home</a>&nbsp;&nbsp;&nbsp;
                        <a href="Companies.aspx" class="footer_links">Companies</a>&nbsp;&nbsp;&nbsp;
                        <a href="Challenge.aspx" class="footer_links">Challenge</a>&nbsp;&nbsp;&nbsp;
                        <a href="FAQ.aspx" class="footer_links">FAQ</a>&nbsp;&nbsp;&nbsp;
                        <a href="Terms.aspx" class="footer_links">Terms</a>&nbsp;&nbsp;&nbsp;
                        <a href="Privacy.aspx" class="footer_links">Privacy</a>
                    </div>
                    <div class="col-md-2 text-right">
                        <a href="https://plus.google.com/118315223845486755233" rel="publisher"><i class="fa fa-google-plus-square fa-2x"></i></a>&nbsp;&nbsp;<a href="http://facebook.com/invesd"><i class="fa fa-facebook-square fa-2x"></i></a>&nbsp;&nbsp;<a href="http://twitter.com/invesd"><i class="fa fa-twitter-square fa-2x"></i></a>
                    </div>
                </div>
            </div>
        </section>

    </form>
</body>
</html>
