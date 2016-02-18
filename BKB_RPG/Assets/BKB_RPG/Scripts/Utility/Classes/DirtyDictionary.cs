using System.Collections.Generic;

namespace BKB.Collections {
    [System.Serializable]
    // Used to cache the sum (additive and multiplicitve) of a dictionary of numbers
    public class DirtyDictionary<Tkey> : Dictionary<Tkey, float> {
        float sum;
        float mSum;

        public float Sum {
            get { return sum; }
        }
        public float MSum {
            get { return mSum; }
        }

        public new void Add(Tkey key, float value) {
            base.Add(key, value);
            UpdateCache();
        }

        public new bool Remove(Tkey key) {
            bool r = base.Remove(key);
            UpdateCache();
            return r;
        }

        private void UpdateCache() {
            float mySum = 0;
            float myMSum = 1;
            foreach (float v in this.Values)
            {
                mySum += v;
                myMSum *= v;
            }
            sum = mySum;
            mSum = myMSum;
        }

    }
}