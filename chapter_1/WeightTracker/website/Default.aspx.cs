using System;
using System.Collections.Generic;

using System.Web;

using Microsoft.Health;
using Microsoft.Health.Web;
using Microsoft.Health.ItemTypes;
using Microsoft.Health;

public partial class HelloWorldPage : HealthServicePage
{
    protected void Page_Load(object sender, EventArgs e)
    {

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
}
