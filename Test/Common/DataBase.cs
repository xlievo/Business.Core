using Business.Utils;
using System.Runtime.CompilerServices;

public class DataBase : Business.Data.DataBase<DataModel.Connection>
{
    public static readonly DataBase DB = new DataBase();

    static DataBase()
    {
        LinqToDB.Common.Configuration.Linq.AllowMultipleQuery = true;
        LinqToDB.Data.DataConnection.TurnTraceSwitchOn();
        LinqToDB.Data.DataConnection.OnTrace = c =>
        {
            switch (c.TraceInfoStep)
            {
                case LinqToDB.Data.TraceInfoStep.Error:
                    c.Exception.Console();
                    return;
                case LinqToDB.Data.TraceInfoStep.Completed:
                    break;
                default: return;
            }

            var con = c.DataConnection as LinqToDB.LinqToDBConnection;

            System.Console.WriteLine($"{c.StartTime}{con?.TraceMethod}:{con?.TraceId}{System.Environment.NewLine}{c.SqlText}{System.Environment.NewLine}{c.ExecutionTime}");
        };
    }

    public override DataModel.Connection GetConnection([CallerMemberName] string callMethod = null) => new DataModel.Connection(LinqToDB.Data.DataConnection.DefaultSettings.DefaultConfiguration) { TraceMethod = callMethod };
}