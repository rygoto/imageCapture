using UnityEngine;

public class LineDraw : MonoBehaviour
{
    [SerializeField] private LineRenderer _rend;
    [SerializeField] private Camera _cam;

    private void Update()
    {
        Vector2 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(0))
            SetPosition(mousePos);
    }

    private void SetPosition(Vector2 pos)
    {
        _rend.positionCount++;
        _rend.SetPosition(_rend.positionCount - 1, pos);
    }
}