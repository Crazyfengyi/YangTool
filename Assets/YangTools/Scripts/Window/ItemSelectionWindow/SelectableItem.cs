using UnityEngine;

public interface ISelectableItem
{
    string Id { get; }
    string DisplayName { get; }
    Sprite Icon { get; }
    object Payload { get; }
}

public class SelectableItem : ISelectableItem
{
    public string Id { get; private set; }
    public string DisplayName { get; private set; }
    public Sprite Icon { get; private set; }
    public object Payload { get; private set; }

    public SelectableItem(string id, string displayName, Sprite icon = null, object payload = null)
    {
        Id = id;
        DisplayName = displayName;
        Icon = icon;
        Payload = payload;
    }
}
