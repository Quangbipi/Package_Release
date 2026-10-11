using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quang.DesignPattern
{
    public interface IOriginator
    {
        IMemento Save();
    }
}