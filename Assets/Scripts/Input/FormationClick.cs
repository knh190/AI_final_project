using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FormationClick : MonoBehaviour
{
    public Color highlighColor = Color.white;
    public Color normalColor = new Color(.45f, .45f, .45f);
    public FormationType formationType = FormationType.Line;

    public delegate void OnFormationSelect();
    public static OnFormationSelect ToggleButtonEvent;

    private Button btn;

    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(MakeFormation);
    }

    // set button color according to current selected formation type
    // if current formation is the same as the button, highlight then
    private void ToggleButton()
    {
        Formation formation = TrackClick.instance.ctx.currFormation;

        if (formation != null && formation.currFormation == formationType)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = highlighColor;
            btn.colors = colors;
        }
        else
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = normalColor;
            btn.colors = colors;
        }
    }

    // send command to make formation
    public void MakeFormation()
    {
        Formation formation = TrackClick.instance.ctx.currFormation;

        if (formation != null && formation.currFormation != formationType)
        {
            formation.currFormation = formationType;
            formation.MakeFormation();
        }
        // toggle highlight
        ToggleButtonEvent?.Invoke();
    }

    private void OnEnable()
    {
        ToggleButtonEvent += ToggleButton;
    }

    private void OnDisable()
    {
        ToggleButtonEvent -= ToggleButton;
    }
}
