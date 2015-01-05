using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class admin_classifier_v1 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        crude_classifier_v1();
    }

    protected void crude_classifier_v1() {
        string[] good = new string[] { "upside", "downside", "discount", "/share","attractive","undervalued","expensive","potential","push","up","headed","worth","target","price target","gain","fail","estimate","value","valuation"};
        string[] bad = new string[] { "million", "billion","dividend","yield","production"};
        string[] suffix_prefix = new string[] { "upside","downside","potential","gain","per","valuation","price","target","worth"};

        int count_bad = 0;
        int count_good = 0;
        bool upside_downside = false;

        Response.Write("<table>");

        DataClassesDataContext db = new DataClassesDataContext();
        var articles = from temp in db.articles where temp.type == 1 select temp;
        int count = 0;

        if (articles.Any()) {
            foreach (var article in articles) {
                var article_ticker = from temp in db.ArticleTickers where temp.article == article.idarticle select temp;

                count_bad = 0;
                count_good = 0;
                upside_downside = false;

                if (article_ticker.Count() == 1) {

                    string[] title = article.title.Split(' ');

                    int title_index = 0;
                    foreach (string word in title) {
                        if (good.Contains(word.ToLower()))
                            count_good++;
                        else if (bad.Contains(word.ToLower()))
                            count_bad++;
                        else if (word.Contains("%") || word.Contains("$")){
                            if (title_index>0){
                                if (suffix_prefix.Contains(title[title_index-1].ToLower()))
                                    upside_downside = true;
                            }

                            if (title_index<title.Count()-1){
                                if (suffix_prefix.Contains(title[title_index + 1].ToLower()))
                                    upside_downside = true;
                            }

                        }

                        title_index++;
                    }

                    count++;

                    if (count == 100)
                        break;

                    Response.Write("<tr><td>" + article.title + "</td><td>" + count_good + "</td><td>" + count_bad + "</td><td>" + upside_downside + "</td><td>" + article.action + "</td></tr>");

                    
                }
            }

            Response.Write("</table>");
        }
    }
}