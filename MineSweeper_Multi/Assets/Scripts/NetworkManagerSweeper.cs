using UnityEngine;
using Mirror;

public class NetworkManagerSweeper : NetworkManager
{
    private readonly byte[] _playerNums = { 1, 2, 3, 4 };

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        conn.identity.GetComponent<Player>().SetPlayerNum(GetFirstAvailableNum(null));
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        // AssignOneToHost();

        foreach (NetworkConnectionToClient connection in NetworkServer.connections.Values)
        {
            if (!connection.identity || ConnectionIsHost(connection)) continue;

            if (connection != null && connection != conn)
            {
                connection.identity.GetComponent<Player>().SetPlayerNum(GetFirstAvailableNum(conn));
            }
        }
    }

    /// <summary>
    /// Gets the first available player number that is not currently being used by another client
    /// </summary>
    /// <param name="exception">Connection to ignore (typically the one that is currently disconnecting)</param>
    /// <returns>Available player number, or 0 if none available</returns>
    private byte GetFirstAvailableNum(NetworkConnectionToClient exception)
    {
        foreach (byte num in _playerNums)
        {
            // Debug.Log($"Starting search for {num} in all clients");
            bool found = false;
            foreach (NetworkConnectionToClient connection in NetworkServer.connections.Values)
            {
                if (connection != null && connection != exception && connection.identity.GetComponent<Player>().GetPlayerNum() == num)
                {
                    // Debug.Log($"{num} was found in {connection.identity.name}");
                    found = true;
                    break;
                }
            }

            if (!found)
                return num;
        }

        return 0;
    }

    public int ConnectionAmount()
    {
        int count = 0;
        foreach (NetworkConnectionToClient connection in NetworkServer.connections.Values)
            if (connection != null)
                count++;

        return count;
    }

    private bool ConnectionIsHost(NetworkConnectionToClient connection) => connection.connectionId == 0;
}
