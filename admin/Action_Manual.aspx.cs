using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class admin_Action_Manual : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        
    }

    protected void build_analyst() {
        DataClassesDataContext db = new DataClassesDataContext();
        var analysts = from temp in db.users where temp.bloomberg_broker.HasValue select temp;

        if (analysts.Any()) {
            foreach (var u in analysts) {
                analyst.Items.Add(new ListItem(u.display_name, u.userID.ToString()));
            }
        }
    }

    protected void add_article(object sender, EventArgs e) {
        DataClassesDataContext db = new DataClassesDataContext();
        article a = new article();

        try
        {
            a.date = Convert.ToDateTime(date.Text);
            a.origin = Convert.ToInt32(analyst.SelectedValue);
            a.summary = Ancillary.string_cutter(summary.Text,40,false,"top");
            a.text = summary.Text;

            // junk
            
            a.text = "";
            a.rateCount = 0;
            a.rateSum = 0;
            a.provider = 1410832;
            a.type = 3;
            a.action = true;
            a.is_ticket = false;
            a.price = 0;
            a.not_actionable = false;
            a.premium = false;
            a.Publish = true;
            a.actions = 1;
            a.backdated = true;

            db.articles.InsertOnSubmit(a);
            db.SubmitChanges();
            articleid.Text = a.idarticle.ToString();
            

            ArticleTicker at = new ArticleTicker();
            int id = (from temp in db.funds where temp.ticker == ticker.Text select temp).First().fundID;
            tickerid.Text = id.ToString();
            at.ticker = id;
            at.article = a.idarticle;
            db.ArticleTickers.InsertOnSubmit(at);
            db.SubmitChanges();

            message.Text = "Article created successfully";


        }
        catch {
            message.Text = "Something went wrong in article add";
        }
    }

    protected void add_action(object sender, EventArgs e) {
        DataClassesDataContext db = new DataClassesDataContext();
        Action a = new Action();

        

        try
        {
            a.creator = 2;
            a.creationTime = DateTime.Now;
            a.ticker = Convert.ToInt32(tickerid.Text);

            var article = (from temp in db.articles where temp.idarticle == Convert.ToInt32(articleid.Text) select temp).First();
            var price = (from temp in db.fund_values where temp.fundID == Convert.ToInt32(tickerid.Text) && temp.date == article.date select temp).First();

            a.article = article.idarticle;
            a.startDate = article.date;
            a.targetDate = article.date.AddDays(365);
            a.active = true;
            a.actualStartDate = price.date;
            a.beta = 0;
            a.breached = false;
            a.currentValue = price.closeValue.Value;
            a.date_feed = price.date;
            a.days_gain = 0;
            a.deleted = false;
            a.dividend = 0;
            a.expired = false;
            a.is_ticket = false;
            a.lastUpdated = price.date;

            a.startValue = price.closeValue.Value;
            a.matured = false;
            a.maxValue = price.closeValue.Value;
            a.minValue = a.maxValue;
            a.premium = false;
            a.price = 0;
            a.progress = 0;
            a.return_overall = 0;
            a.targetValue = Convert.ToDouble(target.Text);
            a.@short = a.targetValue < price.closeValue.Value;
            a.lowerValue = a.@short?(a.startValue*2):0;
            a.skin_in_the_game = false;
            a.TotalReturn = true;

            db.Actions.InsertOnSubmit(a);
            db.SubmitChanges();
            message.Text = "Action created successfully";
            
        }
        catch {
            message.Text = "Error in action";
        }

    }

}