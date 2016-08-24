
public class Counter {

    int value;
    int max;

    public Counter(int max) {
        this.max = max;
        value = 0;
    }

    public bool Tick() {
        value++;
        if (value >= max)
        {
            value = 0;
            return true;
        }
        return false;
    }

}
