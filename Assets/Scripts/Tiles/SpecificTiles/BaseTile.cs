using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BaseTile : TileMain
{
    public int startingHealthPoints = 20;
    private int healthPoints;
    public Text healthtext;

    private void Start()
    {
        healthPoints = startingHealthPoints;
        healthtext.text = healthPoints.ToString();
    }

    public void Damage(int atkPower)
    {
        healthPoints -= atkPower;
        healthtext.text = healthPoints.ToString();
    }
    public int GetHealth()
    {
        return healthPoints;
    }
}
