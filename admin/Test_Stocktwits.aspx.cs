using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using StockTwitsSharp;
using System.Configuration;

public partial class admin_Test_Social : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var service = new TwitterService(ConfigurationManager.AppSettings["StockTwitsClientID"], ConfigurationManager.AppSettings["StockTwitsClientSecret"]);
        service.AuthenticateWith(ConfigurationManager.AppSettings["InvesdStockTwitsAccessToken"]);
        TwitterStatus temp = service.SendTweet(new SendTweetOptions { Status = "$TXN Consensus: http://www.invesd.com/Company.aspx?ticker=TXN", AccessToken = ConfigurationManager.AppSettings["InvesdStockTwitsAccessToken"] });
         
    }
}