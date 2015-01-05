using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TweetSharp;
using System.Configuration;

public partial class admin_Test_Social : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var service = new TwitterService(ConfigurationManager.AppSettings["TwitterAPIKey"], ConfigurationManager.AppSettings["TwitterAPISecret"]);
        service.AuthenticateWith(ConfigurationManager.AppSettings["InvesdTwitterAccessToken"], ConfigurationManager.AppSettings["InvesdTwitterAccessSecret"]);

        /*var tweets = service.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions());
        foreach (var tweet in tweets)
        {
            Console.WriteLine("{0} says '{1}'", tweet.User.ScreenName, tweet.Text);
        }*/
        TwitterStatus temp = service.SendTweet(new SendTweetOptions { Status = "Hello" });
         
    }
}