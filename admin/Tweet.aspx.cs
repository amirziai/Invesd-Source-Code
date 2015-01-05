using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class admin_Tweet : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        decompose_the_tweet();
    }

    protected void decompose_the_tweet()
    {
        int found = 0;
        int found_and_updated = 0;

        DataClassesDataContext db = new DataClassesDataContext();
        var tweets = from temp in db.AnalystRatings select temp;
        if (tweets.Any())
        {
            foreach (var t in tweets)
            {
                TableRow tr = new TableRow();
                TableCell td1 = new TableCell(); td1.Font.Size = 8;
                TableCell td2 = new TableCell(); td2.Font.Size = 8;
                TableCell td3 = new TableCell(); td3.Font.Size = 8;
                TableCell td4 = new TableCell(); td4.Font.Size = 8;
                TableCell td5 = new TableCell(); td5.Font.Size = 8;
                TableCell td6 = new TableCell(); td6.Font.Size = 8; td6.Text = "";
                TableCell td7 = new TableCell(); td7.Font.Size = 8; td7.Text = "";
                TableCell td8 = new TableCell(); td8.Font.Size = 8; td8.Text = "";
                TableCell td9 = new TableCell(); td9.Font.Size = 8; td9.Text = "";
                TableCell td10 = new TableCell(); td10.Font.Size = 8; td10.Text = "";
                //TableCell td8 = new TableCell(); td8.Font.Size = 8; td8.Text = "";

                string[] ts = t.tweet.Split(' ');
                string ticker = "";
                double tp = -20;
                //string source = "";
                string broker = "";

                if (t.tweet.Contains(" by ") && t.tweet.Contains(" to "))
                {
                    try
                    {
                        broker = t.tweet.Substring(t.tweet.IndexOf(" by ") + 4, t.tweet.IndexOf(" to ") - (t.tweet.IndexOf(" by ") + 4)).Trim();
                    }
                    catch { }
                } // ARN

                if (t.tweet.Contains("? ") && t.tweet.Contains(" thinks "))
                {
                    try
                    {
                        broker = t.tweet.Substring(t.tweet.IndexOf("? ") + 2, t.tweet.IndexOf(" thinks ") - (t.tweet.IndexOf("? ") + 2)).Trim();
                    }
                    catch { }
                } // ARN

                if (t.tweet.Contains(" of ") && t.tweet.Contains(" thinks "))
                {
                    try
                    {
                        broker = t.tweet.Substring(t.tweet.IndexOf(" of ") + 4, t.tweet.IndexOf(" thinks ") - (t.tweet.IndexOf(" of ") + 4)).Trim();
                    }
                    catch { }
                } // ARN

                if (t.tweet.Contains(" of ") && t.tweet.Contains(" for "))
                {
                    try
                    {
                        broker = t.tweet.Substring(t.tweet.IndexOf(" of ") + 4, t.tweet.IndexOf(" for ") - (t.tweet.IndexOf(" of ") + 4)).Trim();
                    }
                    catch { }
                } // ARN

                if (t.tweet.Contains(" EPS estimates cut by "))
                {
                    int beg = t.tweet.IndexOf(" EPS estimates cut by ") + 22;
                    int end = (t.tweet.Length - 1);
                    foreach (string tt in t.tweet.Substring(beg, end - beg).Split(' '))
                    {
                        broker += tt.TrimEnd('.') + " ";
                        if (tt.EndsWith("."))
                            break;
                    }

                    broker = broker.Trim();
                }

                if (t.tweet.Contains(" reiterated at "))
                {
                    int start = t.tweet.IndexOf(" reiterated at ") + 15;
                    int end = t.tweet.IndexOf(" PT.");
                    try
                    {
                        foreach (string tt in t.tweet.Substring(start, end - start).Split(' '))
                        {
                            broker += tt.TrimEnd('.') + " ";
                            if (tt.EndsWith("."))
                            {
                                break;
                            }
                        }
                    }
                    catch
                    {
                        //Response.Write("<font color=red>start: " + start + "end: " + end + "</font><BR>");
                    }

                    broker = broker.Trim();
                    //try
                    //{
                    //    broker = t.tweet.Substring(t.tweet.IndexOf(" reiterated at ") + 15, t.tweet.IndexOf("..") - (t.tweet.IndexOf(" reiterated at ") + 15)).Trim();
                    //}
                    //catch { }
                } // ARN

                if ((t.tweet.Contains(" upgrades ") || t.tweet.Contains(" downgrades ")) && t.tweet.Contains(" from ") && t.tweet.Contains(" to "))
                {
                    int sub = 0;
                    if (t.tweet.Contains(" upgrades "))
                    {
                        sub = t.tweet.IndexOf(" upgrades ");
                    }
                    else
                    {
                        sub = t.tweet.IndexOf(" downgrades ");
                    }

                    broker = t.tweet.Substring(0, sub).Trim();
                }

                foreach (string tt in ts)
                {
                    try
                    {
                        string tt_tmp = tt;
                        if (tt.Contains("http") && tt.Contains("$"))
                        {
                            tt_tmp = tt.Substring(0, tt.IndexOf(":"));
                        }

                        tp = Convert.ToDouble(tt_tmp.TrimEnd('.').Replace("$", "").Replace(",", ""));
                    }
                    catch
                    {
                        if (tt.Contains("$") && !tt.Contains("http"))
                        {
                            ticker = tt.Replace("$", "").Replace("?", "");
                        }
                    }
                }

                if (tp <0)
                {
                    td1.ForeColor = System.Drawing.Color.Silver;
                    td5.ForeColor = System.Drawing.Color.Silver;
                    td5.Text = "No PT";
                }
                
                //Response.Write("<font color=" + (tp > 0 ? "black" : "silver") + ">" + t.tweet + "</font><br>");
                //if (tp > 0)
                //    Response.Write("<font color=green>" + ticker + " " + tp + " " + broker + "</font><br>");

                if (tp != -20 && !string.IsNullOrEmpty(ticker) && !string.IsNullOrEmpty(broker))
                {
                    if (broker.Contains("JPMorgan"))
                    {
                        broker = "JPM";
                    }
                    else if (broker.Contains("Moelis"))
                    {
                        broker = "Moelis & Company";
                    }
                    else if (broker.Contains("MLV"))
                    {
                        broker = "MLV";
                    }

                    var find_it = from temp in db.Actions where temp.active && temp.user.Bloomberg_Broker1.name == broker_name_mapping(broker) && temp.fund.ticker == ticker select temp;
                    if (find_it.Any())
                    {
                        found++;
                        //Response.Write("<font color = blue>exists, started " + Math.Round((DateTime.Now - find_it.First().startDate).TotalDays) + " days ago, last updated " + Math.Round((DateTime.Now - find_it.First().date_feed).TotalDays, 0) + " days ago </font><br>");
                        double previous = find_it.First().targetValue;
                        
                        td5.Text = "No change";
                        td5.ForeColor = System.Drawing.Color.Silver;

                        double days_feed = Math.Round((DateTime.Now - find_it.First().startDate).TotalDays);
                        double days_start = Math.Round((DateTime.Now - find_it.First().date_feed).TotalDays);
                        td9.Text = days_feed.ToString();
                        td10.Text = days_start.ToString();
                        td9.ForeColor = days_color(days_feed);
                        td10.ForeColor = days_color(days_start);

                        if (previous != tp)
                        {
                            td5.Text = "New PT";
                            td6.Text = previous.ToString();
                            td5.ForeColor = System.Drawing.Color.Green;

                            //Response.Write("<font color = blue>was " + previous + "</font>");
                            double jump = tp / previous - 1;
                            td7.Text = Math.Round(100* jump) + "%";
                            if (jump >= 0 && jump < 0.05)
                                td7.ForeColor = System.Drawing.Color.GreenYellow;
                            else if (jump >= 0.05 && jump < 0.1)
                                td7.ForeColor = System.Drawing.Color.LightGreen;
                            else if (jump >= 0.1)
                                td7.ForeColor = System.Drawing.Color.DarkGreen;
                            else if (jump < 0 && jump >= -.05)
                                td7.ForeColor = System.Drawing.Color.OrangeRed;
                            else if (jump < -0.05 && jump > -0.1)
                                td7.ForeColor = System.Drawing.Color.Red;
                            else if (jump <= -0.1)
                                td7.ForeColor = System.Drawing.Color.DarkRed;
                            //if (Math.Abs(jump) >= 0.05)
                            //{
                            //    Response.Write(" (<font color=red>" + Math.Round(jump * 100) + "% jump</font>)");
                            //}

                            found_and_updated++;
                        }
                    }
                    else
                    {
                        td5.Text = "No coverage";
                        td5.ForeColor = System.Drawing.Color.Silver;
                        var find_broker = from temp in db.Bloomberg_Brokers where temp.name == broker_name_mapping(broker) select temp;
                        if (!find_broker.Any()){
                            td5.Text = "No broker";
                            //Response.Write("<font color=red>Broker not found</font><br>");
                        }
                    }
                }

                
                td1.Text = Ancillary.string_cutter(t.tweet,70,false,null);
                td8.Text = Math.Round((DateTime.Now - t.timestamp).TotalDays).ToString() ;
                td2.Text = ticker;
                td3.Text = tp > 0 ? tp.ToString() : "";
                td4.Text = broker;

                tr.Cells.Add(td1);
                tr.Cells.Add(td2);
                
                tr.Cells.Add(td4);
                tr.Cells.Add(td5);
                tr.Cells.Add(td3);
                tr.Cells.Add(td6);
                tr.Cells.Add(td7);
                
                tr.Cells.Add(td8);
                tr.Cells.Add(td10);
                tr.Cells.Add(td9);
                
                tbl.Rows.Add(tr);

                //Response.Write("<br><br>");

            }
        }

        //Response.Write("Found: " + found + ", found & updated: " + found_and_updated);

    }

    protected System.Drawing.Color days_color(double days)
    {
        if (days <= 30)
            return System.Drawing.Color.Black;
        else if (days > 30 && days <= 60)
            return System.Drawing.Color.Gray;
        else if (days > 60 && days < 90)
            return System.Drawing.Color.Silver;
        else
            return System.Drawing.Color.Red;
    }

    protected string broker_name_mapping(string broker)
    {
        if (broker == "B. Riley & Company, Inc.")
            return "B Riley & Co";
        else if (broker == "Morningstar")
            return "Morningstar, Inc";
        else if (broker == "Argus Research Company")
            return "Argus Research Corp";
        else if (broker == "RBC Capital")
            return "RBC Capital Markets";
        else if (broker == "RBC Capital Mkts")
            return "RBC Capital Markets";
        else if (broker == "Needham")
            return "Needham & Co";
        else if (broker == "CRT Capital")
            return "CRT Capital Group";
        else if (broker == "Argus")
            return "Argus Research Corp";
        else if (broker == "Robert W. Baird")
            return "Robert W. Baird & Co";
        else if (broker == "Maxim Group")
            return "Maxim Group LLC";
        else if (broker == "R. F. Lafferty")
            return "R.F. Lafferty & Co";
        else if (broker == "Topeka Capital Markets")
            return "Topeka Capital Markets Inc";
        else if (broker == "Boenning Scattergood")
            return "Boenning & Scattergood Inc";
        else if (broker == "Jefferies Group")
            return "Jefferies";
        else if (broker == "Drexel Hamilton")
            return "Drexel Hamilton LLC";
        else if (broker == "SunTrust")
            return "SunTrust Robinson Humphrey";
        else if (broker == "BB &T Corp.")
            return "BB&T Capital Markets";
        else if (broker == "Wunderlich")
            return "Wunderlich Securities";
        else if (broker == "Stifel Nicolaus")
            return "Stifel";
        else if (broker == "KeyCorp")
            return "KeyBank";
        else if (broker == "Citigroup Inc")
            return "Citic Securities Co., Ltd";
        else if (broker == "JPM")
            return "JPMorgan";
        else if (broker == "Capital One Financial Corp.")
            return "Capital One Southcoast, Inc.";
        else if (broker == "Imperial Capital")
            return "Imperial Capital LLC";
        else if (broker == "Bernstein")
            return "Sanford C. Bernstein";
        else if (broker == "Ascendiant Capital Markets")
            return "Ascendiant";
        else if (broker == "MLV")
            return "MLV & Co";
        else if (broker == "N+1 Singer")
            return "N+1 Singer Ltd";
        else if (broker == "DA Davidson")
            return "D.A. Davidson & Co";
        else if (broker == "Guggenheim")
            return "Guggenheim Securities LLC";
        else if (broker == "Compass Point")
            return "Compass Point Research & Trading LLC";
        else if (broker == "Moelis & Company")
            return "Moelis & Company";
        else
            return broker;
    }
}