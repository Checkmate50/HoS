﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Board
{
  public interface ITiledBoardElement : IBoardElement
  {
    int Opacity { get; }  // How difficult it is to see through this board element
  }
}