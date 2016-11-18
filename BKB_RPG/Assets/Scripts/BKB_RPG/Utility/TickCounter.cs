
public class TickCounter {
    private int _value;
    private int _max;
    private bool _pause;

    public TickCounter(int max) {
        _max = max;
        _value = 0;
    }

    public bool Tick {
        get{
            if (_pause)
                return false;
            _value++;
            if (_value >= _max)
            {
                _value = 0;
                return true;
            }
            return false;
        }
    }

    public void Pause() {
        _pause = true;
    }

    public void Resume() {
        _pause = false;
    }

    public void Restart() {
        _value = 0;
        Resume();
    }

    public void ResetInterval(int interval)
    {
        _max = interval;
        Restart();
    }

}
