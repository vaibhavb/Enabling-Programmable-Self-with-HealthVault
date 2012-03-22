using System;
using System.Collections.Generic;
using System.Web;
using System;
using System.Collections.Generic;

using System.Web;

using Microsoft.Health;
using Microsoft.Health.Web;
using Microsoft.Health.ItemTypes;
using System.Xml;
using System.Data;
using System.Xml.XPath;

public partial class _Default : HealthServicePage 
{
    private string _appName = "Quantified Self";
    private string _appDailyAlcoholExtensionName = "QuantifiedSelf.DailyAlchohol";

    protected void Page_Load(object sender, EventArgs e)
    {
        Lbl_UserName.Text = this.PersonInfo.SelectedRecord.DisplayName;

        HealthRecordSearcher searcher = PersonInfo.SelectedRecord.CreateSearcher();
        HealthRecordFilter filter = new HealthRecordFilter(
            ApplicationSpecific.TypeId,
            Emotion.TypeId,
            DietaryDailyIntake.TypeId,           
            Weight.TypeId,
            SleepJournalAM.TypeId,
            Exercise.TypeId);

        filter.EffectiveDateMin = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0));
        searcher.Filters.Add(filter);
        
        HealthRecordItemCollection items = searcher.GetMatchingItems()[0];

        List<Weight> weights = new List<Weight>();
        List<Emotion> emotions = new List<Emotion>();
        List<SleepJournalAM> sleep = new List<SleepJournalAM>();
        List<DietaryDailyIntake> dietaryintake = new List<DietaryDailyIntake>();
        List<Exercise> exercise = new List<Exercise>();
        List<ApplicationSpecific> selfexperiements = new List<ApplicationSpecific>();
        
        foreach (HealthRecordItem item in items)
        {
            if (item is Weight){
                weights.Add((Weight)item);
            }
            if (item is Emotion){
                emotions.Add((Emotion)item);
            }
            if (item is DietaryDailyIntake)
            {
                dietaryintake.Add((DietaryDailyIntake)item);
            }
            if (item is SleepJournalAM)
            {
                sleep.Add((SleepJournalAM)item);
            }
            if (item is Exercise)
            {
                exercise.Add((Exercise)item);
            }
            if (item is ApplicationSpecific)
            {
                selfexperiements.Add((ApplicationSpecific)item);
            }
        }


        DisplayWeight(weights);
        DisplayMood(emotions);
        DisplayDailyDiet(dietaryintake);
        DisplaySleep(sleep);
        DisplayExercise(exercise);
        DisplaySelfExperiments(selfexperiements);
        DataBindMoodInputControls();
    }

    private void DisplayWeight(List<Weight> weights)
    {
        DataTable weight = new DataTable("weight");
        weight.Columns.Add(new DataColumn("Date"));
        weight.Columns.Add(new DataColumn("Weight"));
        weight.Columns.Add(new DataColumn("Source"));
        foreach (Weight w in weights)
        {
            DataRow row = weight.NewRow();
            row["Date"] = w.EffectiveDate.ToShortDateString().ToString();
            row["Weight"] = w.Value.DisplayValue.ToString();
            row["Source"] = w.CommonData.Source;
            weight.Rows.Add(row);
        }
        WeightView.DataSource = weight;
        WeightView.DataBind();
    }

    protected void Btn_SubmitWeight_Click(object sender, EventArgs e)
    {
        double weight = double.Parse(Txt_Weight.Text);
        Weight w = new Weight(
                new HealthServiceDateTime(DateTime.Now),
                new WeightValue(
                    weight * 1.6, new DisplayValue(weight, "lbs", "lbs")));
        w.CommonData.Source = _appName;
        PersonInfo.SelectedRecord.NewItem(w);
    }

    private void DisplaySleep(List<SleepJournalAM> sleeps)
    {
        DataTable sleep = new DataTable("sleep");
        sleep.Columns.Add(new DataColumn("Date"));
        sleep.Columns.Add(new DataColumn("Sleep"));
        sleep.Columns.Add(new DataColumn("Awakenings"));
        foreach (SleepJournalAM s in sleeps)
        {
            DataRow row = sleep.NewRow();
            row["Date"] = s.EffectiveDate.ToShortDateString().ToString();
            row["Sleep"] = s.SleepMinutes/60;
            row["Awakenings"] = s.Awakenings.Count;
            sleep.Rows.Add(row);
        }
        SleepView.DataSource = sleep;
        SleepView.DataBind();
    }

    private void DisplayExercise(List<Exercise> exercises)
    {
        DataTable exercise = new DataTable("exercise");
        exercise.Columns.Add(new DataColumn("Date"));
        exercise.Columns.Add(new DataColumn("ExerciseType"));
        exercise.Columns.Add(new DataColumn("CaloriesBurned"));
        foreach (Exercise e in exercises)
        {
            DataRow row = exercise.NewRow();
            row["Date"] = e.EffectiveDate.ToShortDateString().ToString();
            row["ExerciseType"] = e.Activity.Text;
            if (e.Details.ContainsKey(ExerciseDetail.CaloriesBurned_calories))
            {
                row["CaloriesBurned"] = e.Details[ExerciseDetail.CaloriesBurned_calories];
            }
            exercise.Rows.Add(row);
        }
        ExerciseView.DataSource = exercise;
        ExerciseView.DataBind();
    }

    private void DisplayMood(List<Emotion> emotions)
    {
        DataTable emotion = new DataTable("emotion");
        emotion.Columns.Add(new DataColumn("Date"));
        emotion.Columns.Add(new DataColumn("Emotion"));
        emotion.Columns.Add(new DataColumn("Note"));
        foreach (Emotion e in emotions)
        {
            DataRow row = emotion.NewRow();
            row["Date"] = e.EffectiveDate.ToShortDateString().ToString();
            row["Emotion"] = e.ToString();
            row["Note"] = e.CommonData.Note;
            emotion.Rows.Add(row);
        }
        MoodView.DataSource = emotion;
        MoodView.DataBind();
    }

    private void DataBindMoodInputControls()
    {
        Dd_List_Mood.DataSource = Enum.GetValues(typeof(Mood));
        Dd_List_Stress.DataSource = Enum.GetValues(typeof(RelativeRating));
        Dd_List_Wellbeing.DataSource = Enum.GetValues(typeof(Wellbeing));
        Dd_List_Mood.DataBind();
        Dd_List_Stress.DataBind();
        Dd_List_Wellbeing.DataBind();
    }

 
    protected void Btn_Submit_Mood_Click(object sender, System.EventArgs e)
    {
        //Post Mood
        Emotion emotion = new Emotion();
        emotion.Mood = (Mood) Enum.Parse(typeof(Mood), Dd_List_Mood.SelectedValue);
        emotion.Stress = (RelativeRating)Enum.Parse(typeof(Mood), Dd_List_Stress.SelectedValue);
        emotion.Wellbeing = (Wellbeing)Enum.Parse(typeof(Mood), Dd_List_Wellbeing.SelectedValue);
        emotion.CommonData.Note = Txt_Mood.Text;
        PersonInfo.SelectedRecord.NewItem(emotion);
    }

    protected void Submit_Daily_Diet_Click(object sender, System.EventArgs e)
    {
        //Post Diet
        DietaryDailyIntake diet = new DietaryDailyIntake();
        int totalCarbs;
        int.TryParse(Txt_DailyDietCarbs.Text, out totalCarbs);
        diet.TotalCarbohydrates.Kilograms = totalCarbs * 1000;
        diet.CommonData.Note = Txt_DailyDietNote.Text;

        //Adding extension data
        string drinks = Txt_DailyDietAlcohol.Text;
        HealthRecordItemExtension extension =
            new HealthRecordItemExtension(_appDailyAlcoholExtensionName);
        diet.CommonData.Extensions.Add(extension);
        XPathNavigator navigator = extension.ExtensionData.CreateNavigator();
        navigator.InnerXml = @"<extension source=""" + _appDailyAlcoholExtensionName + @""">
                <alcoholic-drinks>" + drinks + "</alcoholic-drinks>";

        PersonInfo.SelectedRecord.NewItem(diet);
    }

    private void DisplayDailyDiet(List<DietaryDailyIntake> dailydiets)
    {
        DataTable dailydiet = new DataTable("DailyDiets");
        dailydiet.Columns.Add(new DataColumn("Date"));
        dailydiet.Columns.Add(new DataColumn("Carbs (in gm)"));
        dailydiet.Columns.Add(new DataColumn("Alcohol (#drinks)"));
        dailydiet.Columns.Add(new DataColumn("Note"));
        foreach (DietaryDailyIntake e in dailydiets)
        {
            DataRow row = dailydiet.NewRow();
            row["Date"] = e.EffectiveDate.ToShortDateString().ToString();
            row["Carbs (in gm)"] = e.ToString();
            foreach(HealthRecordItemExtension extension in e.CommonData.Extensions)
            {
                if (extension.Source == _appDailyAlcoholExtensionName)
                {
                    XPathNavigator navigator = extension.ExtensionData.CreateNavigator();
                    XPathNavigator alcoholicDrinksNavigator = 
                        navigator.SelectSingleNode("extension/alcoholic-drinks");
                    if (alcoholicDrinksNavigator != null)
                    {
                        row["Alcohol (#drinks)"] = alcoholicDrinksNavigator.Value;
                    }
                }
            }
            row["Note"] = e.CommonData.Note;
            dailydiet.Rows.Add(row);
        }
        DailyDietView.DataSource = dailydiet;
        DailyDietView.DataBind();
    }


    private void DisplaySelfExperiments(List<ApplicationSpecific> selfExperiments)
    {
        DataTable selfExperiment = new DataTable("SelfExperiments");
        selfExperiment.Columns.Add(new DataColumn("Date"));
        selfExperiment.Columns.Add(new DataColumn("Hypothesis"));
        selfExperiment.Columns.Add(new DataColumn("Status"));
        foreach (ApplicationSpecific s in selfExperiments)
        {
            
            DataRow row = selfExperiment.NewRow();
            row["Date"] = s.EffectiveDate.ToShortDateString().ToString();
            row["Hypothesis"] = s.ApplicationSpecificXml[0].CreateNavigator().
                                SelectSingleNode("hypothesis").Value;
            row["Status"] = s.CommonData.Note;
            selfExperiment.Rows.Add(row);
        }
        SelfExperimentsView.DataSource = selfExperiment;
        SelfExperimentsView.DataBind();
    }

    protected void Btn_ShowWeeklyWeightReadings_Click(object sender, EventArgs e)
    {
        HealthRecordSearcher searcher = PersonInfo.SelectedRecord.CreateSearcher();
        HealthRecordFilter filter = new HealthRecordFilter(
            Emotion.TypeId,
            DietaryDailyIntake.TypeId,
            Weight.TypeId,
            SleepJournalAM.TypeId,
            Exercise.TypeId);

        filter.EffectiveDateMin = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0));
        searcher.Filters.Add(filter);

        HealthRecordItemCollection items = searcher.GetMatchingItems()[0];

        TimeSeries weight = new TimeSeries("Weight Graph");
        TimeSeries emotions = new TimeSeries("Emotions");
        TimeSeries dietaryintakes = new TimeSeries("Dietary Intake (Carbs)");
        TimeSeries sleep = new TimeSeries("Sleep");
        TimeSeries exercise = new TimeSeries("Exercise");

        foreach (HealthRecordItem item in items)
        {
            if (item is Weight)
            {
                Weight w = (Weight)item;
                weight.SeriesValue.Add(new TimeSeries.TimeSeriesValues(
                    w.EffectiveDate, w.Value.DisplayValue.Value));
            }
            if (item is Emotion)
            {
                Emotion m = (Emotion)item;
                emotions.SeriesValue.Add(new TimeSeries.TimeSeriesValues(
                    m.EffectiveDate, 1.0));
            }
            if (item is DietaryDailyIntake)
            {

            }
            if (item is SleepJournalAM)
            {
                SleepJournalAM s = (SleepJournalAM)item;
                sleep.SeriesValue.Add(new TimeSeries.TimeSeriesValues(
                    s.EffectiveDate, s.SleepMinutes));
            }
            if (item is Exercise)
            {
            }
        }

        TimeplotView.Plots.Add(weight);
        TimeplotView.Plots.Add(emotions);
        TimeplotView.Plots.Add(dietaryintakes);
        TimeplotView.Plots.Add(sleep);
        TimeplotView.Plots.Add(exercise);
        TimeplotView.DataBind();
        TimeplotView.Visible = true;
    }


    protected void Btn_ShowWeeklyReadingsTextSummary_Click(object sender, System.EventArgs e)
    {
        HealthRecordSearcher searcher = PersonInfo.SelectedRecord.CreateSearcher();
        HealthRecordFilter filter = new HealthRecordFilter(
            Emotion.TypeId,
            DietaryDailyIntake.TypeId,
            Weight.TypeId,
            SleepJournalAM.TypeId,
            Exercise.TypeId);

        filter.EffectiveDateMin = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0));
        searcher.Filters.Add(filter);
        filter.View.TransformsToApply.Add("mtt");

        HealthRecordItemCollection items = searcher.GetMatchingItems()[0];

        DataTable dataTable = new DataTable();
        dataTable.Columns.Add(new DataColumn("Date", typeof(string)));
        dataTable.Columns.Add(new DataColumn("Type", typeof(string)));
        dataTable.Columns.Add(new DataColumn("Summary", typeof(string)));
        foreach (HealthRecordItem item in items)
        {
            XmlNode mttDocument = item.TransformedXmlData["mtt"].SelectSingleNode("data-xml/row");
            DataRow row = dataTable.NewRow();
            row["Date"] = mttDocument.Attributes["wc-date"].Value;
            row["Type"] = mttDocument.Attributes["wc-type"].Value;
            row["Summary"] = mttDocument.Attributes["summary"].Value;
            dataTable.Rows.Add(row);
        }

        Grid_ReadingsTextSummary.DataSource = dataTable;
        Grid_ReadingsTextSummary.DataBind();
        Grid_ReadingsTextSummary.Visible = true;
    }

    protected void Lnk_SwitchAccount_Click(object sender, EventArgs e)
    {
        this.RedirectToShellUrl("AUTH", "appid=" + this.ApplicationId.ToString() +
            "&forceappauth=true", "passthroughParam=optional");
    }

    protected void Lnk_SignOut_Click(object sender, System.EventArgs e)
    {
        this.SignOut();
    }

    protected void Btn_Submit_Hypothesis_Click(object sender, System.EventArgs e)
    {
        ApplicationSpecific appSpecific = new ApplicationSpecific();
        string hypothesis = Txt_Hypothesis.Text;
        appSpecific.ApplicationId = this.ApplicationConnection.ApplicationId.ToString();
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(
            string.Format("<self-experiment><hypothesis>{0}</hypothesis></self-experiment>", 
            hypothesis));
        appSpecific.ApplicationSpecificXml.Add(xml);
        appSpecific.SubtypeTag = "self-experiment";
        appSpecific.Description = hypothesis;
        // Default the status note to active when the hypothesis is created
        appSpecific.CommonData.Note = "Active";
        appSpecific.When = new HealthServiceDateTime(DateTime.Now);
        PersonInfo.SelectedRecord.NewItem(appSpecific); 
    }
}