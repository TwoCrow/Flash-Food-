using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Manages various aspects about state changes.
public class StateManager : MonoBehaviour
{
    public Text stateDisplay;

    public Constants.StationState currentStation;

    public int currentStationIndex = 0;
    public int activeOrderIndex;
    public int activePrepIndex;

    public bool prepStationsActive;

    GameSetup gameSetup;

    // Initializes the station states within the stationQueue, which is used to loop through
    // stations using keyboard input.
    public void setupStationStates()
    {
        // Get a reference to GameSetup. Yes I realize I just passed a ton of stuff from there to here.
        GameObject gameSetupGO = GameObject.Find("GameSetup");
        gameSetup = gameSetupGO.GetComponent<GameSetup>();

        // Set the first station as the ORDER and enqueue it at the end. This allows for a constant
        // loop between the states so that the player can seamlessly move through all stations.
        // Station Order: [ <-> ORDER <-> GRILL <-> TOPPING <-> FRY <-> DRINK <-> ]
        currentStationIndex = 0;
        currentStation = (Constants.StationState)currentStationIndex;
        updateState(currentStation);

        activeOrderIndex = activePrepIndex = - 1;

        prepStationsActive = false;
    }

    // Go to the next station in the list of station states.
    public void nextStation(OrderStationManager osm)
    {
        currentStationIndex++;

        // Go to the first index if we've exceeded the capacity.
        if (currentStationIndex >= Enum.GetValues(typeof(Constants.StationState)).Length)
        {
            currentStationIndex = 0;
        }

        currentStation = (Constants.StationState)currentStationIndex;
        osm.updateAllTickets();
        updateState(currentStation);
    }

    // Go to the previous station in the list of station states.
    public void previousStation(OrderStationManager osm)
    {
        currentStationIndex--;

        // If the index dips below zero, loop back around to the end
        if (currentStationIndex < 0)
        {
            currentStationIndex = Enum.GetValues(typeof(Constants.StationState)).Length - 1;
        }

        currentStation = (Constants.StationState)currentStationIndex;
        osm.updateAllTickets();
        updateState(currentStation);
    }

    // Update the state in the debug text graphic.
    void updateState(Constants.StationState currentStation)
    {
        // If the current state is the PREP station, then disable all order stations and enable all prep stations.
        if (currentStation == (Constants.StationState)1)
        {
            foreach (GameObject station in gameSetup.orderStationsGO)
            {
                station.SetActive(false);
            }

            foreach (GameObject station in gameSetup.prepStationsGO)
            {
                station.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject station in gameSetup.orderStationsGO)
            {
                station.SetActive(true);
            }

            foreach (GameObject station in gameSetup.prepStationsGO)
            {
                station.SetActive(false);
            }
        }

        stateDisplay.text = currentStation.ToString();
    }
}
