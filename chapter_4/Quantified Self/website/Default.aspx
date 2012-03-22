<%@ Page Title="" Language="C#" MasterPageFile="~/QuantifiedSelf.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<%@ Register TagPrefix="uc" TagName="Timeplot" Src="~/Timeplot.ascx" %>
<%@ Register TagPrefix="HV" Namespace="Microsoft.Health.Web"  Assembly="Microsoft.Health.Web" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

<!-- header -->
<div id="header" class="container_12">
<h1 class="grid_8">Quantified Self</h1>
<div id="site-nav" class="grid_4">
    Welcome <asp:Label ID="Lbl_UserName" runat="server"></asp:Label> | 
    <asp:LinkButton ID="Lnk_SwitchAccount" 
                    runat="server" onclick="Lnk_SwitchAccount_Click">Switch Account</asp:LinkButton> |
    <asp:LinkButton ID="Lnk_SignOut" runat="server" onclick="Lnk_SignOut_Click">Sign Out</asp:LinkButton>
</div>
</div>
<hr /><div class="clear"></div>

<!-- main content -->
<div class="container_12">

<div class="grid_12">
<h2>Self Experimentation</h2>
<p>Keeping track of weight, sleep, exercise, mood & diet. View past hypothesis or input new ones.</p>
<div class="datainput">
    <h3>Current hypothesis</h3>
    <b>I'm happier if I sleep well. I sleep well if i dink less alcohol and exercise sufficiently.</b>
    <asp:GridView ID="SelfExperimentsView" runat="server"></asp:GridView>
</div>

<div>
    <h4>Enter new hypothesis("I'm less stressed if I exercise more"):</h4>
    <asp:TextBox ID="Txt_Hypothesis" runat="server"></asp:TextBox>
    <asp:Button ID="Btn_Submit_Hypothesis" runat="server" Text="Submit" 
        onclick="Btn_Submit_Hypothesis_Click" />
</div>
</div>
<div class="clear"></div>
<hr />

<div class="grid_6">
    <h2>Mood</h2>
    <div class="bar">
        <p>Input & track mood. You are tracking happiness.</p>
        <div class="datainput">
            Record today's mood (Format-'{Happy,Stress}:Feeling good'):
            <br />
            Mood:<asp:DropDownList ID="Dd_List_Mood" runat="server"></asp:DropDownList>
            Stress:<asp:DropDownList ID="Dd_List_Stress" runat="server"></asp:DropDownList>
            Wellbeing:<asp:DropDownList ID="Dd_List_Wellbeing" runat="server"></asp:DropDownList>
            <br />
            Note:<asp:TextBox ID="Txt_Mood" runat="server"></asp:TextBox>
            <asp:Button ID="Btn_Submit_Mood" runat="server" Text="Submit" onclick="Btn_Submit_Mood_Click" 
                />
        </div>

        <div class="datatable">
            Last Week's readings:
            <asp:GridView ID="MoodView" runat="server">
            </asp:GridView>
        </div>
    </div>
</div>

<div class="grid_6">
    <h2>Daily Dietary Intake</h2>
    <p>Input & track dialy dietary intake. You are tracking carbs.</p>
    <div class="datainput">
        Record today's carb intake('600gm carbs, 2 drinks and note: Eat burger bun'):
        <br />
        Carbs (in gms): <asp:TextBox ID="Txt_DailyDietCarbs" runat="server"></asp:TextBox>
        <br />
        Alchohol (drinks/day) <asp:TextBox ID="Txt_DailyDietAlcohol" runat="server"></asp:TextBox>
        <br />
        Note: <asp:TextBox ID="Txt_DailyDietNote" runat="server"></asp:TextBox>
        <br />
        <asp:Button ID="Submit_Daily_Diet" runat="server" Text="Submit" onclick="Submit_Daily_Diet_Click" 
            />
    </div>
    <div class="datatable">
        Last Week's readings:
        <asp:GridView ID="DailyDietView" runat="server">
        </asp:GridView>
    </div>
</div>
<div class="clear"></div>


<div class="grid_4">
    <h2>Weight</h2>
    <div class="bar">
    <p>Information on Weight.</p>
    <div class="datainput">
        Record today's weight (in lbs):
        <asp:TextBox ID=Txt_Weight runat="server"></asp:TextBox>
        <asp:Button ID="Btn_SubmitWeight" runat="server" Text="Submit" 
            onclick="Btn_SubmitWeight_Click"/>
    </div>
    <div class="datatable">
        Last Week's readings:
        <asp:GridView ID="WeightView" runat="server">
        </asp:GridView>
    </div>
    </div>
</div>

<div class="grid_4">
    <h2>Sleep</h2>
    <p>Information on sleep.</p>
    <div class="bar">
        <div class="datatable grid_4">
            Last Week's readings:
            <asp:GridView ID="SleepView" runat="server"></asp:GridView>
        </div>
    </div>
</div>

<div class="grid_4">
    <h2>Exercise</h2>
    <p>Exercise details.</p>
    <div class="datatable">
        Last Week's readings:
        <asp:GridView ID="ExerciseView" runat="server"></asp:GridView>
    </div>
</div>
<div class="clear"></div>

</div>

<!-- graph -->
<hr />
<div class="clear"></div>
<div class="container_12">
    <h2>Self Analysis</h2>
    <p>Graphical display to help with correlation.</p>
    <div class="clear"></div>
    <div class="grid_1 prefix_10 suffix_1">
        <asp:Button ID="Btn_ShowWeeklyWeightReadings" runat="Server" 
            Text="Weekly Readings Graph" onclick="Btn_ShowWeeklyWeightReadings_Click"/>
    </div>
    <div class="clear"></div>
    <uc:Timeplot runat="server" ID="TimeplotView" Visible="false" class="grid_12"/>

    <h3>Summary</h3>
    <div class="grid_1 prefix_10 suffix_1">
        <asp:Button ID="Btn_ShowWeeklyReadingsTextSummary" runat="Server" 
            Text="Weekly Readings Summary" 
            onclick="Btn_ShowWeeklyReadingsTextSummary_Click"/>
    </div>
    <asp:GridView ID="Grid_ReadingsTextSummary" runat="server"></asp:GridView>
</div>

<!-- footer -->
<hr />
<div class="clear"></div>


</asp:Content>

