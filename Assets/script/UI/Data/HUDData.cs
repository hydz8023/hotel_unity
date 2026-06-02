[System.Serializable]
public class HUDData
{
    public int silver;
    public float satisfaction;

    public HUDData(int silver, float satisfaction)
    {
        this.silver = silver;
        this.satisfaction = satisfaction;
    }
}

[System.Serializable]
public class DailyReportData
{
    public int day;
    public int income;
    public int expense;
    public int profit;
    public float satisfaction;

    public DailyReportData(int day, int income, int expense, float satisfaction)
    {
        this.day = day;
        this.income = income;
        this.expense = expense;
        this.profit = income - expense;
        this.satisfaction = satisfaction;
    }
}
