using UnityEngine;

class Repeater {
    const float threshold = 0.01f;
    const float rate = 0.01f;
    float _next;
    bool _hold;
    string _axis;

    public Repeater(string axisName) {
        _axis = axisName;
    }

    public int Update() {
        int retValue = 0;
        int value = Mathf.RoundToInt(Input.GetAxisRaw(_axis));

        if (value != 0)
        {
            if (Time.time > _next)
            {
                retValue = value;
                _next = Time.time + (_hold ? rate : threshold);
                _hold = true;
            }
        }
        else
        {
            _hold = false;
            _next = 0;
        }

        return retValue;
    }
}