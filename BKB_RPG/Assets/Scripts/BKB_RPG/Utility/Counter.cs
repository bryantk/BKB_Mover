
public class Counter {

    int value;
    int max;
    bool pause = false;

    public Counter(int max) {
        this.max = max;
        value = 0;
    }

    public bool Tick() {
        if (pause)
            return false;
        value++;
        if (value >= max)
        {
            value = 0;
            return true;
        }
        return false;
    }

    public void Pause() {
        pause = true;
    }

    public void Resume() {
        pause = false;
    }

    public void Restart() {
        value = 0;
        Resume();
    }

}
