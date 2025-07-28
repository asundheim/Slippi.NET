using Newtonsoft.Json;

namespace ComboRenderer;

[JsonObject(MissingMemberHandling = MissingMemberHandling.Ignore)]
internal class ComboRendererSettings
{
    public List<string> ConnectCodes { get; set; } = new List<string>();
    public List<string> DisplayNames { get; set; } = new List<string>();

    public bool FollowDolphin
    { 
        get; 
        set
        {
            if (field != value)
            {
                field = value;
                FollowDolphinChanged?.Invoke(this, value);
            }
        }
    } = true;
    public event EventHandler<bool>? FollowDolphinChanged;

    public double ExplicitWidth 
    { 
        get; 
        set
        {
            if (field != value)
            {
                field = value;
                ExplicitWidthChanged?.Invoke(this, value);
            }
        }
    } = 500;
    public event EventHandler<double>? ExplicitWidthChanged;
    
    public double ExplicitHeight 
    { 
        get; 
        set
        {
            if (field != value)
            {
                field = value;
                ExplicitHeightChanged?.Invoke(this, value);
            }
        }
    } = 500;
    public event EventHandler<double>? ExplicitHeightChanged;

    public string TrackWindow
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                TrackWindowChanged?.Invoke(this, value);
            }
        }
    } = "Live";
    public event EventHandler<string>? TrackWindowChanged;
}
