namespace Common.Zeebe;

public interface IZeebeJob
{
    IJob Job { get; set; }
}