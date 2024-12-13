using AntDesign;

namespace JobHunter.WebApp.Components.Helpers;

public class DynamicTagCollection
{
    public bool inputVisible { get; set; } = false;
    public string _inputValue { get; set; }
    public Input<string> _inputRef;
    public List<string> lstTags { get; private set; }

    public DynamicTagCollection(List<string> lstTags)
    {
        this.lstTags = lstTags;
    }

    public void SetItems(List<string> tags)
    {
        lstTags = tags;
    }

    public void HandleInputConfirm()
    {
        if (string.IsNullOrEmpty(_inputValue))
        {
            CancelInput();
            return;
        }

        string res = lstTags.Find(s => s == _inputValue);

        if (string.IsNullOrEmpty(res))
        {
            lstTags.Add(_inputValue);
        }

        CancelInput();
    }

    public void OnRemove(string tag)
    {
        lstTags.Remove(tag);
    }

    public void CancelInput()
    {
        this._inputValue = "";
        this.inputVisible = false;
    }
}