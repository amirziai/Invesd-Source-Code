<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Update_Progress.aspx.cs" Inherits="admin_Update_Progress" MasterPageFile="~/admin/MasterPage_Admin.master" %>

<asp:Content runat="server" ContentPlaceHolderID="head"></asp:Content>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="body">
    <div class="row">
        <div class="span12">
            <div class="span4">
                <div class="thumbnail">
                    <div class="progress">
                        <div class="bar" id="progress_fundvalues" runat="server" style="margin-bottom:0">
                            <span id="inside_progress" runat="server"></span>
                        </div>
                    </div>
                    <div class="text-right">
                        <span id="l_fundvalues" runat="server"></span>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>