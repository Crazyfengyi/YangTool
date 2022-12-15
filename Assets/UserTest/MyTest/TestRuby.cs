using UnityEngine;
using YangTools;

public class TestRuby : MonoBehaviour
{
    void Start()
    {

    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var textScript = GetComponentInChildren<CustomText>();
            textScript.ShowTextByTyping(textScript.text, () =>
            {
                Debug.LogError("≤‚ ‘Ω· ¯..");
            });
        }
    }

}
