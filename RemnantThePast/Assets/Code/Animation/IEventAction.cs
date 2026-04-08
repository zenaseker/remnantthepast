/// <summary>
/// 敌人意图-在时间轴上的显示
/// </summary>
public class IEventAction: ICacheable
{
    public string ID;
    public bool IsFinished = false;
    public IRoundQueneObject original;
    public void Init(string id, IRoundQueneObject original)
    {
        ID = id;
        this.original = original;
    }
    public void Reset()
    {
        ID = null;
        IsFinished = false;
        original = null;
    }
}
