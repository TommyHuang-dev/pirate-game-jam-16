using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public class BacteriaSpit : Enemy
{
    protected override void setType()
    {
        this.type = AIType.BasicRanged;
    }
}




