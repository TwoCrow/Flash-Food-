using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderStation
{
    public int stationNumber { get; set; }
    public GameObject station { get; set; }
    public Order order { get; set; }
    public Prep prep { get; set; }

    public OrderStation(int stationNumber, GameObject station)
    {
        this.stationNumber = stationNumber;
        this.station = station;
        this.order = null;
        this.prep = null;
    }
}
