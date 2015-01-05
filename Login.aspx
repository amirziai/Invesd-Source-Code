<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Signup4" MasterPageFile="~/MasterPage_Signup_Login.master" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
    <script src="<%=Page.ResolveUrl("~") %>js/login_new2.js"></script>
    <script type="text/javascript">
        var ReturnUrl = '<%=ReturnUrl%>';
        var page_resolve_url_root = '<%=page_resolve_url_root%>';
    </script>
    <script>
    function STSuccess(access_token){                    
                    $.getJSON("https://api.stocktwits.com/api/2/account/verify.json?callback=?", { access_token: access_token },
                         function(data) {
                             if (data.user) {
                                 $.ajax({
                                     type: "POST",
                                     contentType: "application/json; charset=utf-8",
                                     url: "<%=Page.ResolveUrl("~")%>Users.asmx/Check_with_StockTwits",
                                      dataType: "json",
                                      data: "{stocktwits_access_token:\"" + access_token + "\",\
                                                       stocktwits_username:\"" + data.user.username + "\", \
                                                        stocktwits_name:\"" + data.user.name + "\", \
                                                        stocktwits_avatorurl:\"" + data.user.avatar_url + "\", \
                                                        stocktwits_avatorurlssl:\"" + data.user.avatar_url_ssl + "\", \
                                                        stocktwits_bio:\"" + data.user.bio + "\" \
                                          }",
                                      success: function (result) {
                                          url = getParameterByName("ReturnUrl");
                                          if (url == "") {
                                              url = "/";
                                          }
                                          switch (result.d) {
                                              case "gotologin_email_exists":
                                                  window.location.href = url;
                                                  break;
                                              case "not_signedup":
                                                  $("#msg").html('<i class="fa fa-warning" style="color:#62c462"></i>There is no Invesd account linked to this StockTwits account, so you can\'t login using it. But you can go to Sign up page and Signup as a new user, or if you already have an Invesd account, first log in and then link your StockTwits account to it.');                                                  
                                                  break;
                                              case "returnuser_entered":
                                                  window.location.href = url;
                                                  break;
                                              case "bad_email":
                                                  alert("Please enter a valid Email address.")
                                                  break;
                                              case "already_loggedin":
                                                  window.location.href = url;
                                                  break;
                                              default:
                                                  alert("Unkown status:" +result.d);
                                                  break;
                                          }

                                      }
                                  });
                              }
                          });

                }
                var win;
                function STSignUp() {
                    //var a = document.createElement('a');
                    //a.href = url;
                    var hostname = "https://" + location.hostname;
                    var url = "https://api.stocktwits.com/api/2/oauth/authorize?client_id=<%=ConfigurationManager.AppSettings["StockTwitsClientID"]%>&redirect_uri=" + hostname + "/SignupST.aspx&response_type=token&scope=read,watch_lists,publish_messages";
                    win = window.open(url, 'stocktwits_oauth', 'width=500,height=550');
                }     
        
        function getParameterByName(name) {
            name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
            var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
                results = regex.exec(location.search);
            return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
        }
            </script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="body">

    <div class="row margin_small">
        <div class="col-md-4"></div>
        <div class="col-md-4 text-right">
            <a href="/signup<%=string.IsNullOrEmpty(Request.QueryString["ReturnUrl"])?"":("?ReturnUrl=" + Request.QueryString["ReturnUrl"]) %>" class="btn btn-success btn-lg">&nbsp;&nbsp;Signup&nbsp;&nbsp;</a>
        </div>
        <div class="col-md-4"></div>
    </div>

    <div class="row margin_large">
        <div class="col-md-4"></div>
        <div class="col-md-4 text-center div_back_gray thumbnail" style="color:black">
                <img src="images/invesd_logo.png" style="height:35px" />
                <h3>Login using StockTwits</h3>
                <div id="connect" onclick="return STSignUp()" ><img src="images/logo/stconnect.png" style="cursor:pointer" /><br /></div>
                <hr style="color:black" />
                <h3>Or, login using email</h3>
                <div class="form-horizontal">
                    <div class="form-group" id="div_email">
                        <div class="col-sm-12">
                            <input type="text" placeholder="Email" class="form-control input-lg" id="email"/>
                        </div>
                    </div>
                    <div class="form-group" id="div_password">
                        <div class="col-sm-12">
                            <input type="password" placeholder="Password" class="form-control input-lg" id="passWord"/>
                        </div>
                    </div>
                    <button class="btn btn-block btn-info btn-lg" id="logInBtn" onclick="return login();">Login</button>
                    
                    <button class="btn btn-block btn-info btn-lg" id="btn_forgot" onclick="return forgot();" style="display:none">Retrieve</button>
                    <span id="msg">&nbsp;</span>
                    <br />
                    <a style="color:#5bc0de;cursor:pointer" class="btn btn-link urls" onclick="return forgot_clicked();" id="toggle_login_forgot"><small>Forgot your password?</small></a>
                </div>
        </div>
        <div class="col-md-4"></div>
    </div>
</asp:Content>

