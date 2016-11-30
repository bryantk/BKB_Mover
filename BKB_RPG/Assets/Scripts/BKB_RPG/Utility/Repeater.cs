using UnityEngine;

public class Repeater {
    // Rate to re-trigger signal
    private float _rate = 0.01f;
    // Button name to listen for
    private readonly string _axis;

    private readonly bool _isAxis;
    private float _next;
    

    public float TriggerRate
    {
        set
        {
            _rate = Mathf.Max(value, 0.01f);
            _next = Time.time + _rate;
        }
        get { return _rate; }
    }

    public Repeater(string axisName, bool isAxis = true) {
        _axis = axisName;
        _isAxis = isAxis;
    }

    public int Update() {
        int value = _isAxis ? Mathf.RoundToInt(Input.GetAxisRaw(_axis)) : (Input.GetButton(_axis) ? 1 : 0);
        if (value != 0)
        {
            if (Time.time > _next)
            {
                _next = Time.time + _rate;
                return value;
            }
        }
        else
        {
            _next = 0;
        }
        return 0;
    }

}