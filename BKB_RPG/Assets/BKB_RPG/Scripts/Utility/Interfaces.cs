using UnityEngine;
using System.Collections;

public interface IPauseable {
    void iPause();
    void iResume();
}

public interface ISavable {
    string iSave();
    void iLoad(string json);
}

public interface ITick {
    void iTick();
}