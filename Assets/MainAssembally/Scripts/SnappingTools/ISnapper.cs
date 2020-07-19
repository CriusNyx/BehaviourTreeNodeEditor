using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Snapper;

public interface ISnapper
{
    void Itterate();
    void Snap(GameObject gameObject, SnapMode snapMode);
}