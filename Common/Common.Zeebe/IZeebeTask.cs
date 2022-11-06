namespace Common.Zeebe;

public interface IZeebeTask
{
    IJob Job { get; set; }
}