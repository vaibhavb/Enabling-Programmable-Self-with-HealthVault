using System;
using System.Collections.Generic;

using System.Web;

using Microsoft.Health;
using Microsoft.Health.Web;
using Microsoft.Health.ItemTypes;

public partial class HelloWorldPage : HealthServicePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Lbl_UserName.Text = this.PersonInfo.SelectedRecord.DisplayName;

        HealthRecordSearcher searcher = PersonInfo.SelectedRecord.CreateSearcher();
        HealthRecordFilter filter = new HealthRecordFilter(Weight.TypeId);
        searcher.Filters.Add(filter);
        HealthRecordItemCollection items = searcher.GetMatchingItems()[0];

        Dictionary<string, string> weights = new Dictionary<string, string>();

        foreach (Weight item in items)
        {
            weights[item.When.ToString()] = item.Value.ToString();
        }

        WeightView.DataSource = weights;
        WeightView.DataBind();
    }
    
    
    protected void Btn_SubmitWeight_Click(object sender, EventArgs e)
    {
        double weight = double.Parse(Txt_Weight.Text);
        Weight w = new Weight( 
                new HealthServiceDateTime(DateTime.Now),
                new WeightValue(
                    weight * 1.6, new DisplayValue(weight, "lbs", "lbs")));
        
        PersonInfo.SelectedRecord.NewItem(w);
    }

    protected void Lnk_SwitchAccount_Click(object sender, EventArgs e)
    {
        this.RedirectToShellUrl("AUTH", "appid=" + this.ApplicationId.ToString() + 
            "&forceappauth=true", "passthroughParam=optional");
    }

    protected void Btn_ShowWeeklyWeightReadings_Click(object sender, EventArgs e)
    {
        HealthRecordSearcher searcher = PersonInfo.SelectedRecord.CreateSearcher();
        HealthRecordFilter filter = new HealthRecordFilter(Weight.TypeId);
        filter.EffectiveDateMin = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0));
        searcher.Filters.Add(filter);

        HealthRecordItemCollection items = searcher.GetMatchingItems()[0];

        TimeSeries t = new TimeSeries("Weight Graph");

        foreach (Weight item in items)
        {
            //Assuming all data is in one unit
            t.SeriesValue.Add(new TimeSeries.TimeSeriesValues(
                 item.EffectiveDate, item.Value.DisplayValue.Value));
        }
        TimeplotView.Plots.Add(t);
        TimeplotView.DataBind();
        TimeplotView.Visible = true;
    }
}
