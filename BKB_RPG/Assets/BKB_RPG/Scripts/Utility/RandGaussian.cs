using UnityEngine;
using System.Collections;

public class RandGaussian {

    private int seed = 1;
    private System.Random rand;

    public RandGaussian(int seed=0) {
        this.seed = seed;
        rand = new System.Random(seed);
    }

    public float Next() {
        float u1 = (float)rand.NextDouble(); //these are uniform(0,1) random doubles
        float u2 = (float)rand.NextDouble();
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
            Mathf.Sin(2.0f * Mathf.PI * u2); //random normal(0,1)
        float value = Mathf.Abs(randStdNormal/3);
        return value < 1 ? value : 1;
    }
}


class StateRandom : System.Random {
    System.Int32 _numberOfInvokes;

    public System.Int32 NumberOfInvokes { get { return _numberOfInvokes; } }

    public StateRandom(int Seed, int forward = 0) : base(Seed) {
        for (int i = 0; i < forward; ++i)
            Next(0);
    }

    public override System.Int32 Next(System.Int32 maxValue) {
        _numberOfInvokes += 1;
        return base.Next(maxValue);
    }
}