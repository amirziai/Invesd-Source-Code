<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="admin_Default" MasterPageFile="~/admin/MasterPage_Admin.master" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
    <!-- JQuery Sparkline -->
    <script src="<%=Page.ResolveUrl("~") %>js/jquery.sparkline.min.js" type="text/javascript"></script>

    <script type="text/javascript">
        var extend = false;
        var toggle_afternoon = false;
        var toggle_evening = false;
        var toggle_quick = false;
        var toggle_full = false;
        var articles = <%=type1%>;
        var spinner = " <i class=\"icon-spinner icon-spin\"></i>";

        $(document).ready(function () {
            $("#daily_users").sparkline(daily_users, {
                type: 'bar',
                height: '40',
                barWidth: 10,
                barSpacing: 2,
                barColor: '#0088cc'
            });

            $("#pages_per_user").sparkline(pages_per_user, {
                type: 'bar',
                height: '40',
                barWidth: 10,
                barSpacing: 2,
                barColor: '#0088cc'
            });

            $("#emails").sparkline(emails, {
                type: 'bar',
                height: '40',
                barWidth: 10,
                barSpacing: 2,
                barColor: '#0088cc'
            });

            $("#digest").sparkline(digest, {
                type: 'bar',
                height: '40',
                barWidth: 10,
                barSpacing: 2,
                barColor: '#0088cc'
            });

            $("#actions").sparkline(actions, {
                type: 'bar',
                height: '40',
                barWidth: 10,
                barSpacing: 2,
                barColor: '#0088cc'
            });
        });

        function toggle() {

            if (extend) {
                $("#div_extended").hide();
                extend = false;
            }
            else {
                $("#div_extended").show();
                extend = true;
            }
        }
        inactivate_funds
        function inactivate_funds() {
            var txt = document.getElementById("btn_inactive").innerHTML;

            function onSucess(result) {
                switch (result) {
                    case "":
                        document.getElementById("btn_inactive").innerHTML = txt_update_actions;
                        break;
                    default:
                        alert(result);
                }
            }

            function onError(result) {
                alert(result);
            }

            var txt_update_actions = document.getElementById("btn_inactive").innerHTML;
            document.getElementById("btn_inactive").disabled = true;
            document.getElementById("btn_inactive").innerHTML = txt + " <i class=\"icon-spinner icon-spin\"></i>";
            PageMethods.inactivate_fund($('#in_tickers').val(), onSucess, onError);

            return false;
        }

        function update_some_funds() {
            var txt = document.getElementById("btn_add").innerHTML;

            function onSucess(result) {
               // switch (result) {
                 //   case "":
                document.getElementById("btn_add").innerHTML = result;
                        //break;
                    //default:
                      //  error();
                //}
            }

            function onError(result) {
                error();
            }

            var txt_update_actions = document.getElementById("btn_add").innerHTML;
            document.getElementById("btn_add").disabled = true;
            document.getElementById("btn_add").innerHTML = txt + " <i class=\"icon-spinner icon-spin\"></i>";
            PageMethods.add_tickers($('#in_tickers').val(), onSucess, onError);

            return false;
        }

        function update_actions() {


            var element_name = "btn_update_actions";

            function onSucess(result) {
                reset_button(element_name, txt);
                $("#cancel_update_actions").html('<font style="color:#62c462"><i class="icon-ok"></i></font>');
                //msg("Success", ("#" + element_name), result);
            }

            function onError(result) {
                reset_button(element_name, txt);
                msg(result, element_name, "");
            }

            if (toggle_afternoon) {

                var txt = document.getElementById(element_name).innerHTML;
                document.getElementById(element_name).disabled = true;
                document.getElementById(element_name).innerHTML = txt + spinner;
                PageMethods.update_actions_1pm(onSucess, onError);
            }
            else{
                document.getElementById("btn_update_actions").innerHTML = "Are you sure?";
                $("#cancel_update_actions").show();
                toggle_afternoon = true;
            }

            return false;
        }

        function cancel_update_actions() {
            document.getElementById("btn_update_actions").innerHTML = '<i class="icon-bell"></i> Update after close';
            $("#cancel_update_actions").hide();
            toggle_afternoon = false;
        }

        function update_actions_evening() {


            var element_name = "btn_update_actions_evening";

            function onSucess(result) {
                reset_button(element_name, txt);
                $("#cancel_update_actions_evening").html('<font style="color:#62c462"><i class="icon-ok"></i></font>');
                //msg("Success", ("#" + element_name), result);
            }

            function onError(result) {
                reset_button(element_name, txt);
                msg(result, element_name, "");
            }

            if (toggle_evening) {

                var txt = document.getElementById(element_name).innerHTML;
                document.getElementById(element_name).disabled = true;
                document.getElementById(element_name).innerHTML = txt + spinner;
                PageMethods.update_actions_6pm(onSucess, onError);
            }
            else {
                document.getElementById("btn_update_actions_evening").innerHTML = "Are you sure?";
                $("#cancel_update_actions_evening").show();
                toggle_evening = true;
            }

            return false;
        }

        function cancel_update_actions_evening() {
            document.getElementById("btn_update_actions_evening").innerHTML = '<i class="icon-bell"></i> Update after close';
            $("#cancel_update_actions_evening").hide();
            toggle_evening = false;
        }

        function quick_wall_st() {
            var element_name = "btn_quick_wall_st";

            function onSucess(result) {
                reset_button(element_name, txt);
                msg("Success", ("#" + element_name), result);
            }

            function onError(result) {
                reset_button(element_name, txt);
                msg(result, element_name, "");
            }

            if (toggle_quick){
                var txt = document.getElementById(element_name).innerHTML;
                document.getElementById(element_name).disabled = true;
                document.getElementById(element_name).innerHTML = txt + spinner;
                PageMethods.quick_wall_st(onSucess, onError);
            }
            else{
                var txt = document.getElementById(element_name).innerHTML;
                document.getElementById(element_name).innerHTML = "Are you sure?";
                toggle_quick = true;
            }

            return false;
        }

        function full_wall_st() {
            var element_name = "btn_full_wall_st";

            function onSucess(result) {
                reset_button(element_name, txt);
                msg("Success", ("#" + element_name), result);
            }

            function onError(result) {
                reset_button(element_name, txt);
                msg(result, element_name, "");
            }

            if (toggle_full){
                var txt = document.getElementById(element_name).innerHTML;
                document.getElementById(element_name).disabled = true;
                document.getElementById(element_name).innerHTML = txt + spinner;
                PageMethods.full_wall_st(onSucess, onError);
            }
            else{
                var txt = document.getElementById(element_name).innerHTML;
                document.getElementById(element_name).innerHTML = "Are you sure?";
                toggle_full = true;
            }

            return false;
        }

        function scrape() {
            var element_name = "btn_scrape";

            function onSucess(result) {
                reset_button(element_name, txt);
                //msg("Success", ("#" + element_name), result);
                $("#body_type1articles").html('<span style="color:#62c462">' + (articles + parseInt(result)).toString() + '</span>');
            }

            function onError(result) {
                reset_button(element_name, txt);
                msg(result, ("#" + element_name), "");
            }

            var txt = document.getElementById(element_name).innerHTML;
            document.getElementById(element_name).disabled = true;
            document.getElementById(element_name).innerHTML = txt + spinner;
            PageMethods.scrape(onSucess, onError);

            return false;
        }

        function reset_button(element_name, txt) {
            document.getElementById(element_name).disabled = false;
            document.getElementById(element_name).innerHTML = txt;
        }

        function msg(x, popover_element, extended) {
            $(popover_element).attr("data-placement", "top");

            if (x == "Success") {
                $(popover_element).attr("data-title", x);
                $(popover_element).attr("data-origin-title", x);
                $(popover_element).attr("data-content", extended);
            }
            else {
                $(popover_element).attr("data-title", "Error");
                $(popover_element).attr("data-origin-title", "Error");
                $(popover_element).attr("data-content", x);
            }

            $(popover_element).popover('show');
        }
    </script>
</asp:Content>
    
<asp:Content runat="server" ContentPlaceHolderID="body">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>
    <div class="container">

        <h4>USERS</h4>
        <div class="row">
            <div class="span2 text-center">
                <div class="thumbnail">
                    <div style="height:10px"></div>
                    <span id="daily_users"></span>
                    <br />
                    <div style="height:10px"></div>
                    <span style="color:silver;text-transform:uppercase">Daily users</span>
                </div>
            </div>
            <div class="span2 text-center">
                <div class="thumbnail">
                    <div style="height:10px"></div>
                    <span id="pages_per_user"></span>
                    <br />
                    <div style="height:10px"></div>
                    <span style="color:silver;text-transform:uppercase">Pages/user</span>
                </div>
            </div>
            <div class="span2 text-center">
                <div class="thumbnail">
                    <div style="height:10px"></div>
                    <span id="emails"></span>
                    <br />
                    <div style="height:10px"></div>
                    <span style="color:silver;text-transform:uppercase">EMAILS</span>
                </div>
            </div>
            <div class="span2 text-center">
                <div class="thumbnail">
                    <div style="height:10px"></div>
                    <span id="digest"></span>
                    <br />
                    <div style="height:10px"></div>
                    <span style="color:silver;text-transform:uppercase">DIGEST</span>
                </div>
            </div>
            <div class="span2 text-center">
                <div class="thumbnail">
                    <div style="height:10px"></div>
                    <span id="actions"></span>
                    <br />
                    <div style="height:10px"></div>
                    <span style="color:silver;text-transform:uppercase">ACTIONS</span>
                </div>
            </div>
            <div class="span2 text-center">
                <div class="thumbnail">
                    <asp:Label runat="server" ID="positions"></asp:Label>
                    <span style="color:silver;text-transform:uppercase">Positions</span>
                </div>
            </div>
        </div>

        <h4>METRICS</h4>
        <div class="row">
            <div class="span2 text-center">
                <div class="thumbnail">
                    <asp:Label runat="server" ID="companies"></asp:Label>
                    <span style="color:silver;text-transform:uppercase">COVERED</span>
                </div>
            </div>
            <div class="span2 text-center">
                <div class="thumbnail">
                    <asp:Label runat="server" ID="industries"></asp:Label>
                    <span style="color:silver;text-transform:uppercase">CROWD COVERED</span>
                </div>
            </div>
            <div class="span2 text-center">
                <div class="thumbnail">
                    <asp:Label runat="server" ID="analysts"></asp:Label>
                    <span style="color:silver;text-transform:uppercase">Analysts</span>
                </div>
            </div>
            <div class="span2 text-center">
                <div class="thumbnail">
                    <asp:Label runat="server" ID="actions_all"></asp:Label>
                    <span style="color:silver;text-transform:uppercase">Actions</span>
                </div>
            </div>
            <div class="span2 text-center">
                <div class="thumbnail">
                    <asp:Label runat="server" ID="actions_active"></asp:Label>
                    <span style="color:silver;text-transform:uppercase">Active</span>
                </div>
            </div>
            <div class="span2 text-center">
                <div class="thumbnail">
                    <asp:HyperLink runat="server" ID="gripes" CssClass="urls"></asp:HyperLink>
                    <span style="color:silver;text-transform:uppercase">Gripes</span>
                </div>
            </div>
        </div>

        <div style="height:50px"></div>
        <table border="0" width="100%">
            <tr>
                <td width="50%">
                    <h4>UPDATE DATA</h4>
                </td>
                <td width="50%">
                    <div class="text-right">
                        <div onclick="return toggle();">
                            <i class="icon-chevron-down" style="cursor:pointer;color:black"></i>
                        </div>
                    </div>
                </td>
            </tr>
        </table>
            

        <div class="row" runat="server" id="tbl_admin1">
            <div class="span4 text-center">
                <button class="btn btn-large btn-success btn-block" id="btn_scrape" onclick="return scrape();"><i class="icon-file-alt"></i> Scrape Seeking Alpha</button>
                <table border="0" width="100%">
                    <tr>
                        <td width="65%">
                            <small style="color:gray;float:left">Last updated: 1 day ago</small>
                        </td>
                        <td width="35%">
                            <div class="text-right">
                                <a href="sa.aspx" class="urls">
                                    <asp:Label runat="server" ID="type1articles" style="color:black"></asp:Label> <i class="icon-file-text-alt"></i>
                                </a>
                            </div>
                        </td>
                    </tr>
                </table>
            </div>
            <div class="span4 text-center">
                <button class="btn btn-large btn-success btn-block" id="btn_update_actions" onclick="return update_actions();"><i class="icon-bell"></i> Update after close</button>
                <table border="0" width="100%">
                    <tr>
                        <td width="65%">
                            <small style="color:gray;float:left">Last updated: 7 hours ago</small>
                        </td>
                        <td width="35%">
                            <div class="text-right">
                                <a class="urls" onclick="cancel_update_actions()" style="display:none;cursor:pointer" id="cancel_update_actions">Cancel</a>
                                <%--<i class="icon-warning-sign" style="color:red"></i>--%>
                            </div>
                        </td>
                    </tr>
                </table>
            </div>
            <div class="span4 text-center">
                <button class="btn btn-large btn-success btn-block" id="btn_update_actions_evening" onclick="return update_actions_evening();"><i class="icon-moon"></i> Update evening</button>
                    <table border="0" width="100%">
                    <tr>
                        <td width="65%">
                            <small style="color:gray;float:left">Last updated: 21 hours ago</small>
                        </td>
                        <td width="35%">
                            <div class="text-right">
                                <a class="urls" onclick="cancel_update_actions_evening()" style="display:none;cursor:pointer" id="cancel_update_actions_evening">Cancel</a>
                            </div>
                        </td>
                    </tr>
                </table>   
            </div>
        </div>
            
        <div class="row" id="div_extended" style="display:none">
            <div class="span4">
                <button class="btn btn-success btn-large btn-block">Update adjusted values</button>
                <small style="color:gray;">Last updated: 1/1/2013</small>
            </div>
            <div class="span4">
                <button class="btn btn-success btn-large btn-block">Update fund attributes</button>
                <small style="color:gray;">Last updated: 1/1/2013</small>
            </div>
            <div class="span4">
                <input id="in_tickers" type="text" style="width:150px;margin:0" /> <button class="btn btn-success" onclick="return update_some_funds();" id="btn_add"><i class="icon-ok"></i></button> <button id="btn_inactive" onclick="return inactivate_funds();" class="btn btn-danger"><i class="icon-trash"></i>Inactivate Funds</button>
                <br /><small style="color:gray;">Comma separated: GOOG,AAPL</small>
            </div>
        </div>
            
        <div style="height:50px"></div>
        <h4>UPDATE WALL ST. DATA</h4>
        <div class="row" runat="server" id="tbl_admin3">
            <div class="span6 text-center">
                <button class="btn btn-large btn-info btn-block" id="btn_full_wall_st" onclick="return full_wall_st();">Full Wall St.</button>
                <small style="color:gray">Last updated: 1/1/2013</small>
            </div>
            <div class="span6 text-center">
                <button class="btn btn-large btn-info btn-block" id="btn_quick_wall_st" onclick="return quick_wall_st();">Quick Wall St.</button>
                <small style="color:gray">Last updated: 1/1/2013</small>
            </div>
        </div>
    </div>
</asp:Content>