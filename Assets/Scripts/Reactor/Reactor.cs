﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Reactor
{
    public int gradeType;
    public float heat;
    public float power;

    public SerializableCell[,] serializableCells;
}
