namespace Model;

/*
 * This is the place to subscribe to events surrounding the game where the private and public sections of code intersect.
 * Each subsystem that needs to access these events should subscribe themselves to the corresponding events
 * The code that calls these events is not public as it interacts with the gameserver, but the extracted data structures can be made public
 * Events will be added to this section as we learn how to save and extract more data from the game.
 */

public class GameEvents
{
    public event EventHandler<EndGameEvent>? GameEnd;
}