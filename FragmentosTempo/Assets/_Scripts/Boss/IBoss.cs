using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBoss
{   // Define um m�todo que deve ser implementado por qualquer classe que queira ser considerada um "Boss" no jogo.
    // O m�todo SetCanMove controla a capacidade de movimento do boss.
    void SetCanMove(bool canMove);
}
