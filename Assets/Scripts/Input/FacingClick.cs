using UnityEngine;
using UnityEngine.UI;
using MiniHexMap;

public enum TurningDirection { Left, Right }

[RequireComponent(typeof(Button))]
public class FacingClick : MonoBehaviour
{
    public Color highlighColor = Color.white;
    public Color normalColor = new Color(.45f, .45f, .45f);
    public TurningDirection direction = TurningDirection.Left;

    public delegate void OnAlterDirection();
    public static OnAlterDirection FormationFacingEvent;

    private Button btn;

    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(MakeFormation);
    }

    public void MakeFormation()
    {
        Formation formation = TrackClick.instance.ctx.currFormation;

        if (formation != null)
        {
            if (direction == TurningDirection.Left)
                formation.face = formation.face.Previous();
            if (direction == TurningDirection.Right)
                formation.face = formation.face.Next();
            formation.MakeFormation();
        }
        ToggleButton();
    }

    private void ToggleButton()
    {
        ColorBlock colors = btn.colors;
        colors.normalColor = highlighColor;
        btn.colors = colors;

        Invoke("ResetButton", .1f);
    }

    private void ResetButton()
    {
        ColorBlock colors = btn.colors;
        colors.normalColor = normalColor;
        btn.colors = colors;
    }

    private void OnEnable()
    {
        FormationFacingEvent += ToggleButton;
    }

    private void OnDisable()
    {
        FormationFacingEvent -= ToggleButton;
    }
}
